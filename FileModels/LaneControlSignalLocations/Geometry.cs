using System.Collections.Generic;

namespace Matrix.FileModels.LaneControlSignalLocations
{
    public class Geometry
    {
        public string Type { get; set; }

        public IEnumerable<float> Coordinates { get; set; }
    }
}
