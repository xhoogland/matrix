using Matrix.FileModels.LaneControlSignalLocations;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Matrix.Parsers.LocationParsers
{
    public class A1ReversibleLaneLaneControlSignalLocationsParser : LocationParser
    {
        public IEnumerable<Location> Locations { get; }

        public A1ReversibleLaneLaneControlSignalLocationsParser(string fileLocation)
        {
            var jsonContent = File.ReadAllText(fileLocation);
            Locations = GetLocationsByFileContent(jsonContent);
        }

        public IEnumerable<Location> GetLocationsByFileContent(string fileContent)
        {
            var data = JsonConvert.DeserializeObject<LaneControlSignalLocations>(fileContent);
            return data.Features;
        }
    }
}
