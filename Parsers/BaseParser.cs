using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace Matrix.Parsers
{
    public abstract class BaseParser
    {
        protected string _fileLocation;

        protected Uri _downloadLocation;

        protected BaseParser(string downloadLocation, string fileLocation)
        {
            _fileLocation = fileLocation;
            _downloadLocation = new Uri(downloadLocation);
        }

        public void DownloadImportableFile()
        {
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(_downloadLocation, _fileLocation);
            }
        }

        protected async Task ExtractGzToFile(string outputFile)
        {
            var gzipFileName = new FileInfo(_fileLocation);
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
    }
}
