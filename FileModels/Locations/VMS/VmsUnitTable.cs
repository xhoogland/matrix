using System.Collections.Generic;

namespace Matrix.FileModels.Locations.VMS
{
    public class VmsUnitTable
    {
        public IEnumerable<VmsUnitRecord> VmsUnitRecord { get; set; }

        public string Id { get; set; }

        public byte Version { get; set; }
    }
}
