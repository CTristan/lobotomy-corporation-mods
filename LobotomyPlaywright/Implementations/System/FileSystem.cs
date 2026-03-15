// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using System.IO;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Implementations.System
{
    /// <summary>
    /// Implementation of IFileSystem using System.IO.
    /// </summary>
    public sealed class FileSystem : IFileSystem
    {
        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public string? ReadAllText(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            _ = Directory.CreateDirectory(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern);
        }

        public DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        public long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public string[] GetDirectories(string path, string searchPattern)
        {
            return Directory.GetDirectories(path, searchPattern);
        }

        public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
        {
            File.Copy(sourceFileName, destFileName, overwrite);
        }

        public void SetFileExecutable(string path)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"+x \"{path}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                _ = process.Start();
                process.WaitForExit();
            }
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public void CopyDirectory(string sourceDir, string destDir, bool overwrite)
        {
            _ = Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite);
            }

            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir, overwrite);
            }
        }
    }
}
