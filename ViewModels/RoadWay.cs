using System.Collections.Generic;

namespace Matrix.ViewModels
{
    public class RoadWay
    {
        public string HmLocation { get; set; }

        public IList<VariableMessageSign> VariableMessageSigns { get; set; }

        public RoadWay()
        {
            VariableMessageSigns = new List<VariableMessageSign>();
        }
    }
}
