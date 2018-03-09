namespace Matrix.FileModels.VariableMessageSignLocations
{
    public class VmsRecord
    {
        public VmsDescription VmsDescription { get; set; }

        public string VmsPhysicalMounting { get; set; }

        public string VmsType { get; set; }

        public VmsTextDisplayCharacteristics VmsTextDisplayCharacteristics { get; set; }

        public VmsLocation VmsLocation { get; set; }
    }
}
