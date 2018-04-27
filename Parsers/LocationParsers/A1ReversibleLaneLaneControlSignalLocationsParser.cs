using Matrix.FileModels.LaneControlSignalLocations;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Matrix.Parsers.LocationParsers
{
    public class A1ReversibleLaneLaneControlSignalLocationsParser : LocationParser
    {
        private string _fileLocation;

        private string _downloadLocation;

        public A1ReversibleLaneLaneControlSignalLocationsParser(string fileLocation, string downloadLocation)
        {
            _fileLocation = fileLocation;
            _downloadLocation = downloadLocation;
        }

        public async Task DownloadImportableFile()
        {
            var file = ParserHelper.DownloadFile(_downloadLocation);
            await File.WriteAllTextAsync(_fileLocation, file);
        }

        public async Task<IEnumerable<Location>> RetrieveLocationsFromContent()
        {
            var jsonContent = await File.ReadAllTextAsync(_fileLocation);
            var data = JsonConvert.DeserializeObject<LaneControlSignalLocations>(jsonContent);
            return data.Features;
        }
    }
}
