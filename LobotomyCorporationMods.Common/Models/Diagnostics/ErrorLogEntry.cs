// SPDX-License-Identifier: MIT

#region

using Hemocode.Common.Implementations;

#endregion

namespace Hemocode.Common.Models.Diagnostics
{
    public sealed class ErrorLogEntry
    {
        public ErrorLogEntry(string fileName, string content, string filePath)
        {
            ThrowHelper.ThrowIfNull(fileName);
            FileName = fileName;
            ThrowHelper.ThrowIfNull(content);
            Content = content;
            ThrowHelper.ThrowIfNull(filePath);
            FilePath = filePath;
        }

        public string FileName { get; private set; }

        public string Content { get; private set; }

        public string FilePath { get; private set; }
    }
}
