using System.Collections.Generic;

namespace Matrix.FileModels.LaneControlSignalLocations
{
    public class Geometry
    {
        public string Type { get; set; }

        public IEnumerable<double> Coordinates { get; set; }
    }
}
