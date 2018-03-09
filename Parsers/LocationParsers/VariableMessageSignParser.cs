using Matrix.FileModels;
using Matrix.FileModels.LaneControlSignalLocations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Matrix.Parsers.LocationParsers
{
    class VariableMessageSignParser : ILocationParser
    {
        public IEnumerable<ILocation> Locations { get; }

        public VariableMessageSignParser()
        {
            //var jsonFile = File.ReadAllText("locationsFile.json");
            //var data = JsonConvert.DeserializeObject<List<LaneControlSignalLocations>>(jsonFile)[0];
            //Locations = data.Features;
        }
    }
}
