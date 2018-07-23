using Matrix.FileModels.LiveData.NLDVMS;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Matrix.Parsers.LiveData
{
    public class NLDAllVMSLiveDataParser : BaseParser, LiveDataParser
    {
        public NLDAllVMSLiveDataParser(string downloadLocation, string fileLocation)
            : base(downloadLocation, fileLocation)
        {
        }

        public async Task<IEnumerable<Interfaces.LiveData>> RetrieveLiveDataFromContent()
        {
            var destinationFile = string.Format("{0}.{1}", _fileLocation, "txt");
            await ExtractGzToFile(destinationFile);
            var xmlFile = await File.ReadAllTextAsync(destinationFile);

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlFile);
            var xmlAsJsonContent = JsonConvert.SerializeXmlNode(xmlDocument);

            var colonRegex = new Regex("([a-zA-Z]):([a-zA-Z])");
            xmlAsJsonContent = colonRegex.Replace(xmlAsJsonContent, "$1$2");
            var atSignRegex = new Regex("\"@([a-z])");
            xmlAsJsonContent = atSignRegex.Replace(xmlAsJsonContent, "\"$1");
            var hashRegex = new Regex("\"#([a-z])");
            xmlAsJsonContent = hashRegex.Replace(xmlAsJsonContent, "\"$1");
            var underscoreRegex = new Regex("([a-zA-Z])_([a-zA-Z])");
            xmlAsJsonContent = underscoreRegex.Replace(xmlAsJsonContent, "$1$2");

            var data = JsonConvert.DeserializeObject<NLDVMSLiveData>(xmlAsJsonContent);
            return data.SoapEnvelope.SoapBody.D2LogicalModel.PayloadPublication.VmsUnit;
        }
    }
}
