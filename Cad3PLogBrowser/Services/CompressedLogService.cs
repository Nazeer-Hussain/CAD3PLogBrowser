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

        public List<string> ReadLines(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".gz"  ? ReadGzip(filePath) :
                   ext == ".zip" ? ReadZip(filePath)  :
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

        private static List<string> ReadZip(string filePath)
        {
            var lines = new List<string>();
            using (var archive = ZipFile.OpenRead(filePath))
            {
                foreach (var entry in archive.Entries)
                {
                    // Read text files inside the zip
                    if (entry.FullName.EndsWith("/")) continue;
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
