using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Cad3PLogBrowser.Services.Core
{
    /// <summary>
    /// A7 — Compressed Log Support (.zip / .gz).
    /// Reads log lines from .gz or .zip archives transparently.
    /// </summary>
    public class CompressedLogService
    {
        public static bool IsCompressed(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".gz" || ext == ".zip";
        }

        /// <summary>
        /// A7: names of the real (non-directory) entries inside a .zip, for prompting a
        /// picker when there's more than one instead of silently concatenating them all.
        /// </summary>
        public static List<string> GetZipEntryNames(string filePath)
        {
            var names = new List<string>();
            using (var archive = ZipFile.OpenRead(filePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.FullName.EndsWith("/") && !string.IsNullOrEmpty(entry.Name))
                        names.Add(entry.FullName);
                }
            }
            return names;
        }

        /// <param name="zipEntryName">
        /// When reading a .zip, restricts to this one entry. Null (the default) reads and
        /// concatenates every entry, same as before entry-picking existed.
        /// </param>
        public List<string> ReadLines(string filePath, string zipEntryName = null)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".gz"  ? ReadGzip(filePath) :
                   ext == ".zip" ? ReadZip(filePath, zipEntryName) :
                   new List<string>();
        }

        private static List<string> ReadGzip(string filePath)
        {
            var lines = new List<string>();
            using (var fs = new FileStream(filePath, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            using (var reader = new StreamReader(gz, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    lines.Add(line);
            }
            return lines;
        }

        private static List<string> ReadZip(string filePath, string zipEntryName = null)
        {
            var lines = new List<string>();
            using (var archive = ZipFile.OpenRead(filePath))
            {
                foreach (var entry in archive.Entries)
                {
                    // Read text files inside the zip
                    if (entry.FullName.EndsWith("/")) continue;
                    if (zipEntryName != null && entry.FullName != zipEntryName) continue;

                    using (var stream = entry.Open())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                            lines.Add(line);
                    }
                }
            }
            return lines;
        }
    }
}
