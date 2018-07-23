using System;
using System.Collections.Generic;

namespace Matrix.FileModels.LiveData.NLDVMS
{
    public class PayloadPublication
    {
        public string XmlnsXsi { get; set; }

        public string XsiType { get; set; }

        public string Lang { get; set; }

        public DateTime PublicationTime { get; set; }

        public PublicationCreator PublicationCreator { get; set; }

        public HeaderInformation HeaderInformation { get; set; }

        public IEnumerable<VmsUnit> VmsUnit { get; set; }
    }
}
