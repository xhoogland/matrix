namespace Matrix.FileModels.LiveData.NLDVMS
{
    public class VmsUnit : Interfaces.LiveData
    {
        public VmsUnitTableReference VmsUnitTableReference { get; set; }

        public VmsUnitReference VmsUnitReference { get; set; }

        public Vms Vms { get; set; }

        public string Id => VmsUnitReference.Id.Replace('_'.ToString(), string.Empty);

        public string Sign
        {
            get
            {
                if (VmsMessageExtension == null)
                    return null;

                return VmsMessageExtension.vmsMessageExtension.VmsImage.ImageData.Binary;
            }
        }

        public bool? HasBinary => VmsMessageExtension.vmsMessageExtension.VmsImage.ImageData.Binary != null;

        private VmsMessageExtension VmsMessageExtension => Vms.vms.VmsMessage.vmsMessage.VmsMessageExtension;
    }
}
