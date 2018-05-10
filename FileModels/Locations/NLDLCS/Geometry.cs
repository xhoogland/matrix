using System.Collections.Generic;

namespace Matrix.FileModels.Locations.NLDLCS
{
    public class Geometry
    {
        public string Type { get; set; }

        public IEnumerable<double> Coordinates { get; set; }
    }
}
