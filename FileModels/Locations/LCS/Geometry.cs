using System.Collections.Generic;

namespace Matrix.FileModels.Locations.LCS
{
    public class Geometry
    {
        public string Type { get; set; }

        public IEnumerable<double> Coordinates { get; set; }
    }
}
