using Matrix.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Matrix.Parsers
{
    public abstract class BaseLocationParser : LocationParser
    {
        protected string _fileLocation;

        protected string _downloadLocation;

        protected BaseLocationParser(string fileLocation, string downloadLocation)
        {
            _fileLocation = fileLocation;
            _downloadLocation = downloadLocation;
        }

        public void DownloadImportableFile()
        {
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(new Uri(_downloadLocation), _fileLocation);
            }
        }

        public abstract Task<IEnumerable<Location>> RetrieveLocationsFromContent();
    }
}
