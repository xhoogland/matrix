namespace Matrix.FileModels.Locations.VMS
{
    public class AlertCPoint
    {
        public string XsiType { get; set; }

        public byte AlertCLocationCountryCode { get; set; }

        public decimal AlertCLocationTableNumber { get; set; }

        public string AlertCLocationTableVersion { get; set; }

        public AlertCDirection AlertCDirection { get; set; }

        public AlertCMethod4PrimaryPointLocation AlertCMethod4PrimaryPointLocation { get; set; }
    }
}
