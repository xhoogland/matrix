using Matrix.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Matrix.FileModels.LiveData.NLDVMS
{
    public class VmsUnit : Interfaces.LiveData
    {
        public VmsUnitTableReference VmsUnitTableReference { get; set; }

        public VmsUnitReference VmsUnitReference { get; set; }

        public Vms Vms { get; set; }

        public string Id => VmsUnitReference.Id.Replace("_", string.Empty);

        public string Sign
        {
            get
            {
                if (Vms.vms.VmsMessage.vmsMessage.VmsMessageExtension != null)
                    return Vms.vms.VmsMessage.vmsMessage.VmsMessageExtension.vmsMessageExtension.VmsImage.ImageData.Binary;

                var vmsTextLine = Vms.vms.VmsMessage.vmsMessage.TextPage.VmsText.VmsTextLine.ToString();
                if (vmsTextLine.StartsWith('{') && vmsTextLine.EndsWith('}'))
                    vmsTextLine = string.Format("[{0}]", vmsTextLine);

                var vmsTextLineList = JsonConvert.DeserializeObject<IEnumerable<VmsTextLine>>(vmsTextLine);
                return FormatLinesForParser(vmsTextLineList);
            }
        }

        public DataType DataType
        {
            get
            {
                var vme = Vms.vms.VmsMessage.vmsMessage.VmsMessageExtension;
                if (vme != null && vme.vmsMessageExtension.VmsImage.ImageData.Binary != null)
                    return DataType.Base64;

                if (Vms.vms.VmsMessage.vmsMessage.TextPage != null)
                    return DataType.TextPage;

                return DataType.Unknown;
            }
        }

        private string FormatLinesForParser(IEnumerable<VmsTextLine> vmsTextLineList)
        {
            var result = new StringBuilder();

            foreach (var line in vmsTextLineList)
            {
                var vmsTextLine = line.vmsTextLine.VmsTextLine;
                if (vmsTextLine == null)
                    vmsTextLine = ".";

                result.AppendFormat("{0}|", vmsTextLine);
            }

            return result.ToString().TrimEnd('|');
        }
    }
}
