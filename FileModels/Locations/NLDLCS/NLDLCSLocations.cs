using System.Collections.Generic;

namespace Matrix.FileModels.Locations.NLDLCS
{
    public class NLDLCSLocations
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public Crs Crs { get; set; }

        public IEnumerable<Feature> Features { get; set; }
    }
}
