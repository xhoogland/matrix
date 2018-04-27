using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
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

        public static string DownloadFile(string url)
        {
            //var fileContents = string.Empty;
            //using (var webClient = new WebClient())
            //{
            //    var localFilePath = Path.GetTempFileName();
            //    webClient.Headers.Add("Accept: text/html, application/xhtml+xml, */*");
            //    webClient.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
            //    webClient.DownloadFile(new Uri(url), localFilePath);
            //    fileContents = await File.ReadAllTextAsync(localFilePath);
            //}
            //return fileContents;

            //var webRequest = WebRequest.Create(url);
            //var response = await webRequest.GetResponseAsync();
            //var stream = response.GetResponseStream();
            //var streamReader = new StreamReader(stream);
            //return streamReader.ReadToEnd();

            var fileContents = string.Empty;
            using (var webClient = new WebClient())
            {
                var localFilePath = Path.GetTempFileName();
                webClient.DownloadFile(new Uri(url), localFilePath);
                fileContents = File.ReadAllText(localFilePath);
            }
            return fileContents;

            //using (var httpClient = new HttpClient())
            //{
            //    using (var result = await httpClient.GetAsync(url))
            //    {
            //        if (result.IsSuccessStatusCode)
            //        {
            //            return await result.Content.ReadAsStringAsync();
            //        }
            //    }
            //}
            //return null;
        }
    }
}
