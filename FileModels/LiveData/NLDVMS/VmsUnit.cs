using Newtonsoft.Json;
using System;
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

        public bool IsValid
        {
            get
            {
                var vme = Vms.vms.VmsMessage.vmsMessage.VmsMessageExtension;
                if ((vme != null && vme.vmsMessageExtension.VmsImage.ImageData.Binary != null) ||
                    Vms.vms.VmsMessage.vmsMessage.TextPage != null)
                    return true;

                return false;
            }
        }

        public DateTime LastModification => Vms.vms.VmsMessage.vmsMessage.TimeLastSet;

        private string FormatLinesForParser(IEnumerable<VmsTextLine> vmsTextLineList)
        {
            var result = new StringBuilder("TP|");
            var seperator = "<br />";

            foreach (var line in vmsTextLineList)
            {
                var vmsTextLine = line.vmsTextLine.VmsTextLine;
                if (vmsTextLine == null)
                    vmsTextLine = string.Empty;

                result.AppendFormat("{0}{1}", ChangeSpecialCharactersToHtmlEntityEmoji(vmsTextLine), seperator);
            }

            return result.ToString().Remove(result.Length - seperator.Length, seperator.Length);
        }

        private string ChangeSpecialCharactersToHtmlEntityEmoji(string vmsTextLine)
        {
            var specialCharactersToReplace = new Dictionary<string, string>
            {
                { "%s134", "&#11014;" },
                { "%s136", "&#8599;" },
                { "%s137", "&#11014;" },
                { "%s138", "&#11014;" },
                { "%s139", "&#8599;" },
                { "¡", "&#10035;" },
                { "£", "&#128119;" },
                { "º", "&#127359;" },
                { "¥", "&#11014;" }
            };

            var result = vmsTextLine;
            foreach (var specialChar in specialCharactersToReplace)
            {
                result = result.Replace(specialChar.Key, specialChar.Value);
            }

            return result;
        }
    }
}
