using Matrix.FileModels.Locations.LCSLocations;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Matrix.Parsers.LocationParsers
{
    public class NLDSAAReversibleLaneLCSLocationsParser : BaseParser, LocationParser
    {
        public NLDSAAReversibleLaneLCSLocationsParser(string downloadLocation, string fileLocation)
            : base(downloadLocation, fileLocation)
        {
        }

        public async Task<IEnumerable<Location>> RetrieveLocationsFromContent()
        {
            var jsonContent = await File.ReadAllTextAsync(_fileLocation);
            var data = JsonConvert.DeserializeObject<LCSLocations>(jsonContent);
            return data.Features;
        }
    }
}
