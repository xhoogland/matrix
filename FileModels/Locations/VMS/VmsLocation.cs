namespace Matrix.FileModels.Locations.VMS
{
    public class VmsLocation
    {
        public string XsiType { get; set; }

        public LocationForDisplay LocationForDisplay { get; set; }

        public SupplementaryPositionalDescription SupplementaryPositionalDescription { get; set; }

        public AlertCPoint AlertCPoint { get; set; }
    }
}
