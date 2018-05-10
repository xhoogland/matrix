namespace Matrix.FileModels.Configuration
{
    public abstract class Parsers
    {
        public string NLDAllVMSLocationsParser { get; set; }

        public string NLDGeneralLCSLocationsParser { get; set; }

        public string NLDSAAReversibleLaneLCSLocationsParser { get; set; }

        public string NLDAllLCSLiveDataParser { get; set; }

        public string NLDAllVMSLiveDataParser { get; set; }
    }
}
