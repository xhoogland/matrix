using Matrix.FileModels.VariableMessageSignLocations;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Matrix.Parsers.LocationParsers
{
    public class VariableMessageSignParser : LocationParser
    {
        public IEnumerable<Location> Locations { get; }

        public VariableMessageSignParser()
        {
            var xmlDocument = new XmlDocument();
            var xmlFile = File.ReadAllText("LocatietabelDRIPS.xml");
            xmlDocument.LoadXml(xmlFile);
            xmlFile = null;
            var xmlContent = xmlDocument.GetElementsByTagName("SOAP:Envelope")[0].InnerXml;
            xmlDocument.LoadXml(xmlContent);
            var xmlAsJsonContent = JsonConvert.SerializeXmlNode(xmlDocument).Replace("SOAP:", "SOAP");
            xmlAsJsonContent = xmlAsJsonContent.Replace("#text", "value");
            xmlAsJsonContent = xmlAsJsonContent.Replace("@id", "id");
            var data = JsonConvert.DeserializeObject<VariableMessageSignLocations>(xmlAsJsonContent);
            Locations = data.SOAPBody.D2LogicalModel.PayloadPublication.VmsUnitTable.VmsUnitRecord;
        }
    }
}
