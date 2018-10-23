using System.Collections.Generic;

namespace Matrix.FileModels.LiveData.NLDLCS
{
    public class VariableMessageSignEvents
    {
        public string Xmlns { get; set; }

        public Meta Meta { get; set; }

        public IEnumerable<Event> Event { get; set; }
    }
}
