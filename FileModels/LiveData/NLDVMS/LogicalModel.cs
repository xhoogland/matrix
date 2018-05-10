namespace Matrix.FileModels.LiveData.NLDVMS
{
    public class LogicalModel
    {
        public string Xmlns { get; set; }

        public string ModelBaseVersion { get; set; }

        public string XmlnsXsi { get; set; }

        public Exchange Exchange { get; set; }

        public PayloadPublication PayloadPublication { get; set; }
    }
}
