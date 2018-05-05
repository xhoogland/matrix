using Matrix.FileModels.LaneControlSignalLocations;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Matrix.Parsers.LocationParsers
{
    public class A1ReversibleLaneLaneControlSignalLocationsParser : BaseLocationParser
    {
        public A1ReversibleLaneLaneControlSignalLocationsParser(string fileLocation, string downloadLocation)
            : base(fileLocation, downloadLocation)
        {
        }

        public override async Task<IEnumerable<Location>> RetrieveLocationsFromContent()
        {
            var jsonContent = await File.ReadAllTextAsync(_fileLocation);
            var data = JsonConvert.DeserializeObject<LaneControlSignalLocations>(jsonContent);
            return data.Features;
        }
    }
}
