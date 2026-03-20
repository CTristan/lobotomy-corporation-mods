// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Text;
using DebugPanel.Common.Implementations;

#endregion

namespace DebugPanel.Implementations
{
    /// <summary>
    ///     Reads assembly references from the AssemblyRef metadata table in .NET PE files.
    ///     Parses PE → CLI header → metadata root → #~ stream → AssemblyRef table → #Strings heap.
    ///     Works on raw byte arrays without requiring Mono.Cecil.
    /// </summary>
    public sealed class PeAssemblyRefReader
    {
        private const int AssemblyRefTableIndex = 0x23;

        public IList<string> ReadAssemblyReferences(byte[] peBytes)
        {
            ThrowHelper.ThrowIfNull(peBytes);

            if (peBytes.Length < 128)
            {
                return new List<string>();
            }

            try
            {
                return ParseAssemblyRefs(peBytes);
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        private static IList<string> ParseAssemblyRefs(byte[] pe)
        {
            // DOS header: "MZ" magic
            if (pe[0] != 0x4D || pe[1] != 0x5A)
            {
                return new List<string>();
            }

            var peOffset = BitConverter.ToInt32(pe, 0x3C);
            if (peOffset < 0 || peOffset + 4 > pe.Length)
            {
                return new List<string>();
            }

            // PE signature: "PE\0\0"
            if (pe[peOffset] != 0x50 || pe[peOffset + 1] != 0x45 ||
                pe[peOffset + 2] != 0 || pe[peOffset + 3] != 0)
            {
                return new List<string>();
            }

            // COFF header (20 bytes after PE signature)
            var coffOffset = peOffset + 4;
            var sectionCount = BitConverter.ToUInt16(pe, coffOffset + 2);
            var optHeaderSize = BitConverter.ToUInt16(pe, coffOffset + 16);

            var optOffset = coffOffset + 20;
            if (optOffset + optHeaderSize > pe.Length)
            {
                return new List<string>();
            }

            // Optional header magic determines CLI data directory offset
            var magic = BitConverter.ToUInt16(pe, optOffset);
            int cliDirOffset;
            if (magic == 0x10B)
            {
                cliDirOffset = optOffset + 208;
            }
            else if (magic == 0x20B)
            {
                cliDirOffset = optOffset + 224;
            }
            else
            {
                return new List<string>();
            }

            if (cliDirOffset + 8 > pe.Length)
            {
                return new List<string>();
            }

            var cliHeaderRva = BitConverter.ToUInt32(pe, cliDirOffset);
            if (cliHeaderRva == 0)
            {
                return new List<string>();
            }

            // Read section headers for RVA-to-file-offset mapping
            var sectionsStart = optOffset + optHeaderSize;
            var sectionVAs = new uint[sectionCount];
            var sectionVSizes = new uint[sectionCount];
            var sectionRawPtrs = new uint[sectionCount];
            ReadSections(pe, sectionsStart, sectionCount, sectionVAs, sectionVSizes, sectionRawPtrs);

            // CLI header -> metadata RVA
            var cliOffset = RvaToOffset(cliHeaderRva, sectionVAs, sectionVSizes, sectionRawPtrs, sectionCount);
            if (cliOffset < 0 || cliOffset + 16 > pe.Length)
            {
                return new List<string>();
            }

            var metadataRva = BitConverter.ToUInt32(pe, cliOffset + 8);
            var metadataOffset = RvaToOffset(metadataRva, sectionVAs, sectionVSizes, sectionRawPtrs, sectionCount);
            if (metadataOffset < 0 || metadataOffset + 16 > pe.Length)
            {
                return new List<string>();
            }

            // Metadata root: verify "BSJB" signature (0x424A5342)
            if (BitConverter.ToUInt32(pe, metadataOffset) != 0x424A5342)
            {
                return new List<string>();
            }

            var versionLength = BitConverter.ToInt32(pe, metadataOffset + 12);
            var streamHeadersStart = metadataOffset + 16 + ((versionLength + 3) & ~3);
            if (streamHeadersStart + 4 > pe.Length)
            {
                return new List<string>();
            }

            var streamCount = BitConverter.ToUInt16(pe, streamHeadersStart + 2);

            // Find #~ (or #-) and #Strings streams
            if (!FindStreams(pe, streamHeadersStart + 4, streamCount,
                    out var tablesRelOffset, out var stringsRelOffset, out var stringsSize))
            {
                return new List<string>();
            }

            var tablesStart = metadataOffset + tablesRelOffset;
            var stringsStart = metadataOffset + stringsRelOffset;
            var stringsEnd = stringsStart + stringsSize;

            if (tablesStart + 24 > pe.Length)
            {
                return new List<string>();
            }

            // #~ stream header: heap sizes at +6, valid bitmask at +8
            var heapSizes = pe[tablesStart + 6];
            var valid = BitConverter.ToUInt64(pe, tablesStart + 8);

            // Read row counts for each present table
            var rowCounts = new int[64];
            var pos = tablesStart + 24;
            for (var i = 0; i < 64; i++)
            {
                if ((valid & (1UL << i)) != 0)
                {
                    if (pos + 4 > pe.Length)
                    {
                        return new List<string>();
                    }

                    rowCounts[i] = BitConverter.ToInt32(pe, pos);
                    pos += 4;
                }
            }

            if (rowCounts[AssemblyRefTableIndex] == 0)
            {
                return new List<string>();
            }

            // Compute index sizes from heap sizes and row counts
            var strIdx = (heapSizes & 1) != 0 ? 4 : 2;
            var guidIdx = (heapSizes & 2) != 0 ? 4 : 2;
            var blobIdx = (heapSizes & 4) != 0 ? 4 : 2;
            var coded = ComputeCodedIndexSizes(rowCounts);

            // Skip all table data before AssemblyRef (table 0x23)
            var tableDataStart = pos;
            for (var t = 0; t < AssemblyRefTableIndex; t++)
            {
                if ((valid & (1UL << t)) == 0)
                {
                    continue;
                }

                var rowSize = GetTableRowSize(t, strIdx, guidIdx, blobIdx, rowCounts, coded);
                if (rowSize < 0)
                {
                    return new List<string>();
                }

                tableDataStart += rowCounts[t] * rowSize;
            }

            // AssemblyRef row: Major(2) Minor(2) Build(2) Rev(2) Flags(4) PubKey(blob) Name(str) Culture(str) Hash(blob)
            var asmRefRowSize = 8 + 4 + blobIdx + strIdx + strIdx + blobIdx;
            var nameColumnOffset = 8 + 4 + blobIdx;

            var results = new List<string>();
            for (var r = 0; r < rowCounts[AssemblyRefTableIndex]; r++)
            {
                var rowStart = tableDataStart + (r * asmRefRowSize);
                if (rowStart + nameColumnOffset + strIdx > pe.Length)
                {
                    break;
                }

                int nameIndex;
                if (strIdx == 4)
                {
                    nameIndex = BitConverter.ToInt32(pe, rowStart + nameColumnOffset);
                }
                else
                {
                    nameIndex = BitConverter.ToUInt16(pe, rowStart + nameColumnOffset);
                }

                var name = ReadStringFromHeap(pe, stringsStart, stringsEnd, nameIndex);
                if (!string.IsNullOrEmpty(name))
                {
                    results.Add(name);
                }
            }

            return results;
        }

        private static void ReadSections(byte[] pe, int offset, int count,
            uint[] virtualAddresses, uint[] virtualSizes, uint[] rawDataPointers)
        {
            for (var i = 0; i < count; i++)
            {
                var o = offset + (i * 40);
                if (o + 24 > pe.Length)
                {
                    break;
                }

                virtualSizes[i] = BitConverter.ToUInt32(pe, o + 8);
                virtualAddresses[i] = BitConverter.ToUInt32(pe, o + 12);
                rawDataPointers[i] = BitConverter.ToUInt32(pe, o + 20);
            }
        }

        private static int RvaToOffset(uint rva, uint[] vas, uint[] sizes, uint[] ptrs, int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (rva >= vas[i] && rva < vas[i] + sizes[i])
                {
                    return (int)(rva - vas[i] + ptrs[i]);
                }
            }

            return -1;
        }

        private static bool FindStreams(byte[] pe, int offset, int count,
            out int tablesOffset, out int stringsOffset, out int stringsSize)
        {
            tablesOffset = -1;
            stringsOffset = -1;
            stringsSize = 0;

            var pos = offset;
            for (var i = 0; i < count; i++)
            {
                if (pos + 8 > pe.Length)
                {
                    break;
                }

                var streamOffset = BitConverter.ToInt32(pe, pos);
                var streamSize = BitConverter.ToInt32(pe, pos + 4);

                var nameStart = pos + 8;
                var nameEnd = nameStart;
                while (nameEnd < pe.Length && pe[nameEnd] != 0)
                {
                    nameEnd++;
                }

                var name = Encoding.ASCII.GetString(pe, nameStart, nameEnd - nameStart);

                if (name == "#~" || name == "#-")
                {
                    tablesOffset = streamOffset;
                }
                else if (name == "#Strings")
                {
                    stringsOffset = streamOffset;
                    stringsSize = streamSize;
                }

                // Advance past name + null terminator, aligned to 4-byte boundary
                pos = nameStart + ((nameEnd - nameStart + 1 + 3) & ~3);
            }

            return tablesOffset >= 0 && stringsOffset >= 0;
        }

        private static string ReadStringFromHeap(byte[] pe, int heapStart, int heapEnd, int index)
        {
            var start = heapStart + index;
            if (start < 0 || start >= heapEnd)
            {
                return null;
            }

            var end = start;
            while (end < heapEnd && pe[end] != 0)
            {
                end++;
            }

            if (end == start)
            {
                return null;
            }

            return Encoding.UTF8.GetString(pe, start, end - start);
        }

        // ECMA-335 II.24.2.6 — coded index families
        private static int[] ComputeCodedIndexSizes(int[] rc)
        {
            var sizes = new int[13];
            sizes[0] = CodedSize(2, MaxRow(rc, 0x02, 0x01, 0x1B));
            sizes[1] = CodedSize(2, MaxRow(rc, 0x04, 0x08, 0x17));
            sizes[2] = CodedSize(5, MaxRow(rc, 0x06, 0x04, 0x01, 0x02, 0x08, 0x09, 0x0A, 0x00,
                0x0E, 0x17, 0x14, 0x11, 0x1A, 0x1B, 0x20, 0x23, 0x26, 0x27, 0x28, 0x2A, 0x2C, 0x2B));
            sizes[3] = CodedSize(1, MaxRow(rc, 0x04, 0x08));
            sizes[4] = CodedSize(2, MaxRow(rc, 0x02, 0x06, 0x20));
            sizes[5] = CodedSize(3, MaxRow(rc, 0x02, 0x01, 0x1A, 0x06, 0x1B));
            sizes[6] = CodedSize(1, MaxRow(rc, 0x14, 0x17));
            sizes[7] = CodedSize(1, MaxRow(rc, 0x06, 0x0A));
            sizes[8] = CodedSize(1, MaxRow(rc, 0x04, 0x06));
            sizes[9] = CodedSize(2, MaxRow(rc, 0x26, 0x23, 0x27));
            sizes[10] = CodedSize(3, MaxRow(rc, 0x06, 0x0A));
            sizes[11] = CodedSize(2, MaxRow(rc, 0x00, 0x1A, 0x23, 0x01));
            sizes[12] = CodedSize(1, MaxRow(rc, 0x02, 0x06));

            return sizes;
        }

        private static int CodedSize(int tagBits, int maxRows)
        {
            return maxRows < (1 << (16 - tagBits)) ? 2 : 4;
        }

        private static int MaxRow(int[] rc, params int[] tables)
        {
            var max = 0;
            foreach (var t in tables)
            {
                if (t < rc.Length && rc[t] > max)
                {
                    max = rc[t];
                }
            }

            return max;
        }

        private static int Idx(int[] rc, int table)
        {
            return table < rc.Length && rc[table] > 0xFFFF ? 4 : 2;
        }

        // ECMA-335 II.22 — row sizes for metadata tables 0x00 through 0x22
        private static int GetTableRowSize(int table, int s, int g, int b, int[] rc, int[] ci)
        {
            switch (table)
            {
                case 0x00: return 2 + s + g + g + g;
                case 0x01: return ci[11] + s + s;
                case 0x02: return 4 + s + s + ci[0] + Idx(rc, 0x04) + Idx(rc, 0x06);
                case 0x03: return Idx(rc, 0x04);
                case 0x04: return 2 + s + b;
                case 0x05: return Idx(rc, 0x06);
                case 0x06: return 4 + 2 + 2 + s + b + Idx(rc, 0x08);
                case 0x07: return Idx(rc, 0x08);
                case 0x08: return 2 + 2 + s;
                case 0x09: return Idx(rc, 0x02) + ci[0];
                case 0x0A: return ci[5] + s + b;
                case 0x0B: return 2 + ci[1] + b;
                case 0x0C: return ci[2] + ci[10] + b;
                case 0x0D: return ci[3] + b;
                case 0x0E: return 2 + ci[4] + b;
                case 0x0F: return 2 + 4 + Idx(rc, 0x02);
                case 0x10: return 4 + Idx(rc, 0x04);
                case 0x11: return b;
                case 0x12: return Idx(rc, 0x02) + Idx(rc, 0x14);
                case 0x13: return Idx(rc, 0x14);
                case 0x14: return 2 + s + ci[0];
                case 0x15: return Idx(rc, 0x02) + Idx(rc, 0x17);
                case 0x16: return Idx(rc, 0x17);
                case 0x17: return 2 + s + b;
                case 0x18: return 2 + Idx(rc, 0x06) + ci[6];
                case 0x19: return Idx(rc, 0x02) + ci[7] + ci[7];
                case 0x1A: return s;
                case 0x1B: return b;
                case 0x1C: return 2 + ci[8] + s + Idx(rc, 0x1A);
                case 0x1D: return 4 + Idx(rc, 0x04);
                case 0x1E: return 4 + 4;
                case 0x1F: return 4;
                case 0x20: return 4 + 2 + 2 + 2 + 2 + 4 + b + s + s;
                case 0x21: return 4;
                case 0x22: return 4 + 4 + 4;
                default: return -1;
            }
        }
    }
}
