using Matrix.FileModels.VariableMessageSignLocations;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Matrix.Parsers.LocationParsers
{
    public class VariableMessageSignParser : BaseLocationParser
    {
        public VariableMessageSignParser(string fileLocation, string downloadLocation)
            : base (fileLocation, downloadLocation)
        {
        }

        public override async Task<IEnumerable<Location>> RetrieveLocationsFromContent()
        {
            var destinationFile = string.Format("{0}.{1}", _fileLocation, "txt");
            await ParserHelper.ExtractGzToFile(_fileLocation, destinationFile);
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

            var data = JsonConvert.DeserializeObject<VariableMessageSignLocations>(xmlAsJsonContent);
            return data.SoapEnvelope.SoapBody.D2LogicalModel.PayloadPublication.VmsUnitTable.VmsUnitRecord;
        }
    }
}
