using System.Collections.Generic;

namespace Matrix.FileModels.LaneControlSignalLocations
{
    public class LaneControlSignalLocations
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public Crs Crs { get; set; }

        public IEnumerable<Feature> Features { get; set; }
    }
}
