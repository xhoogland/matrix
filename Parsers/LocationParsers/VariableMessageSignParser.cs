using Matrix.FileModels.VariableMessageSignLocations;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Matrix.Parsers.LocationParsers
{
    public class VariableMessageSignParser : LocationParser
    {
        public IEnumerable<Location> Locations { get; }

        public VariableMessageSignParser(string fileLocation)
        {
            ParserHelper.ExtractGzToFile("LocatietabelDRIPS.xml.gz", fileLocation);
            var xmlFile = File.ReadAllText(fileLocation);

            Locations = GetLocationsByFileContent(xmlFile);
        }

        public IEnumerable<Location> GetLocationsByFileContent(string fileContent)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(fileContent);
            var xmlContent = xmlDocument.InnerXml;
            xmlDocument.LoadXml(xmlContent);
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
