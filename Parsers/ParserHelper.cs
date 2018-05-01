using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Matrix.Parsers
{
    public class ParserHelper
    {
        public static string GetAssemblyName()
        {
            var type = MethodBase.GetCurrentMethod().DeclaringType;
            return Assembly.GetAssembly(type).FullName;
        }

        public async static Task ExtractGzToFile(string gzFile, string outputFile)
        {
            var gzipFileName = new FileInfo(gzFile);
            using (var fileToDecompressAsStream = gzipFileName.OpenRead())
            {
                using (var decompressedStream = File.Create(outputFile))
                {
                    using (var decompressionStream = new GZipStream(fileToDecompressAsStream, CompressionMode.Decompress))
                    {
                        await decompressionStream.CopyToAsync(decompressedStream);
                    }
                }
            }
        }

        public static void DownloadFile(string url, string savePath)
        {
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(new Uri(url), savePath);
            }
        }
    }
}
