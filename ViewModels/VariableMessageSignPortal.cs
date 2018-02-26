using System.Collections.Generic;

namespace Matrix.ViewModels
{
    public class VariableMessageSignPortal
    {
        public Location Location { get; set; }

        public string Country { get; set; }

        public IEnumerable<RoadWay> RoadWays { get; set; }
    }
}
