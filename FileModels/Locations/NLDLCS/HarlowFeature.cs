﻿using System.Collections.Generic;

namespace Matrix.FileModels.Locations.NLDLCS
{
    public class HarlowFeature
    {
        public string Type { get; set; }

        public Properties Properties { get; set; }

        public IEnumerable<double> Coordinates { get; set; }
    }
}
