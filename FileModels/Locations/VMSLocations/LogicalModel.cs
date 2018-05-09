namespace Matrix.FileModels.Locations.VMSLocations
{
    public class LogicalModel
    {
        public string Xmlns { get; set; }

        public string XmlnsXi { get; set; }

        public Exchange Exchange { get; set; }

        public PayloadPublication PayloadPublication { get; set; }

        public byte ModelBaseVersion { get; set; }
    }
}
