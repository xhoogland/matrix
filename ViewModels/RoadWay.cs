using System.Collections.Generic;

namespace Matrix.ViewModels
{
    public class RoadWay
    {
        public string HmLocation { get; set; }

        public IEnumerable<VariableMessageSign> VariableMessageSigns { get; set; }
    }
}
