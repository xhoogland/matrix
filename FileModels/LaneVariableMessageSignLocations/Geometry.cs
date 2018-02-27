using System.Collections.Generic;

namespace FileModels.LaneVariableMessageSignLocations
{
    public class Geometry
    {
        public string Type { get; set; }

        public IEnumerable<float> Coordinates { get; set; }
    }
}
