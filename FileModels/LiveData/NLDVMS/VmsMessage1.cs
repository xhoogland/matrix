using System;

namespace Matrix.FileModels.LiveData.NLDVMS
{
    public class VmsMessage1
    {
        public DateTime TimeLastSet { get; set; }

        public TextPage TextPage { get; set; }

        public VmsMessageExtension VmsMessageExtension { get; set; }
    }
}
