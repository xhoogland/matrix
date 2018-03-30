using System;

namespace Matrix.FileModels.VariableMessageSignLocations
{
    public class PayloadPublication
    {
        public string XmlnsXi { get; set; }

        public string XsiType { get; set; }

        public DateTime PublicationTime { get; set; }

        public PublicationCreator PublicationCreator { get; set; }

        public HeaderInformation HeaderInformation { get; set; }

        public VmsUnitTable VmsUnitTable { get; set; }

        public string Lang { get; set; }
    }
}
