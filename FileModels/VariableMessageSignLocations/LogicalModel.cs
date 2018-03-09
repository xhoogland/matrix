namespace Matrix.FileModels.VariableMessageSignLocations
{
    public class LogicalModel
    {
        public Exchange Exchange { get; set; }

        public PayloadPublication PayloadPublication { get; set; }

        public byte ModelBaseVersion { get; set; }
    }
}
