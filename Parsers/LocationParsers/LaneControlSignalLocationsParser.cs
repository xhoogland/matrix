using Matrix.FileModels;
using Matrix.FileModels.LaneControlSignalLocations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Matrix.Parsers.LocationParsers
{
    class LaneControlSignalLocationsParser : ILocationParser
    {
        public IEnumerable<ILocation> Locations { get; }

        public LaneControlSignalLocationsParser()
        {
            var jsonContent = File.ReadAllText("matrixLocaties.json");
            var data = JsonConvert.DeserializeObject<List<LaneControlSignalLocations>>(jsonContent)[0];
            Locations = data.Features;
        }
    }
}
