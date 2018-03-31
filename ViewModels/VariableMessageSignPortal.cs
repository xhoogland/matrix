using Matrix.Interfaces;
using System.Collections.Generic;

namespace Matrix.ViewModels
{
    public class VariableMessageSignPortal
    {
        public Coordinates Coordinates { get; set; }

        public string Country { get; set; }

        public IList<RoadWay> RoadWays { get; set; }

        public VariableMessageSignPortal()
        {
            RoadWays = new List<RoadWay>();
        }
    }
}
