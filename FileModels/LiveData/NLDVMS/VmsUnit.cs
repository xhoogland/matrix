namespace Matrix.FileModels.LiveData.NLDVMS
{
    public class VmsUnit : Interfaces.LiveData
    {
        public VmsUnitTableReference VmsUnitTableReference { get; set; }

        public VmsUnitReference VmsUnitReference { get; set; }

        public Vms Vms { get; set; }

        public string Id => VmsUnitReference.Id;

        public string Sign
        {
            get
            {
                var vmsMessageExtension = Vms.vms.VmsMessage.vmsMessage.VmsMessageExtension;
                if (vmsMessageExtension == null)
                    return null;

                var imageData = vmsMessageExtension.vmsMessageExtension.VmsImage.ImageData;
                return string.Format("data:{0};{1},{2}", imageData.MimeType, imageData.Encoding, imageData.Binary);
            }
        }
    }
}
