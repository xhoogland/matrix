using System.Collections.Generic;

namespace FileModels.LaneVariableMessageSignLocations
{
    public class LaneVariableMessageSignLocations
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public Crs Crs { get; set; }

        public IEnumerable<Feature> Features { get; set; }
    }
}
