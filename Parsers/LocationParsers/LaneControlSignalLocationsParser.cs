using Matrix.FileModels.LaneControlSignalLocations;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Matrix.Parsers.LocationParsers
{
    class LaneControlSignalLocationsParser : LocationParser
    {
        public IEnumerable<Location> Locations { get; }

        public LaneControlSignalLocationsParser()
        {
            var jsonContent = File.ReadAllText("matrixLocaties.json");
            var data = JsonConvert.DeserializeObject<List<LaneControlSignalLocations>>(jsonContent)[0];
            Locations = data.Features;
        }
    }
}
