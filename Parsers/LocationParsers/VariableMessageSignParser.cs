using Matrix.FileModels;
using Matrix.FileModels.VariableMessageSignLocations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Matrix.Parsers.LocationParsers
{
    class VariableMessageSignParser// : ILocationParser
    {
        public IEnumerable<ILocation> Locations { get; }

        public VariableMessageSignParser()
        {
            var xmlDocument = new XmlDocument();
            var xmlFile = File.ReadAllText("LocatietabelDRIPS.xml");
            //var xmlNamespace = "http://datex2.eu/schema/2/2_0";
            //var xmlContent = xmlFile.Descendants(XName.Get("d2LogicalModel", xmlNamespace)).Descendants(XName.Get("payloadPublication", xmlNamespace)).Descendants(XName.Get("vmsUnitTable", xmlNamespace)).First().ToString();
            xmlDocument.LoadXml(xmlFile);
            xmlFile = null;
            var xmlContent = xmlDocument.GetElementsByTagName("SOAP:Envelope")[0].InnerXml;
            xmlDocument.LoadXml(xmlContent);
            var xmlAsJsonContent = JsonConvert.SerializeXmlNode(xmlDocument).Replace("SOAP:", "SOAP");
            var data = JsonConvert.DeserializeObject<VariableMessageSignLocations>(xmlAsJsonContent);
            Locations = data.SOAPBody.D2LogicalModel.PayloadPublication.VmsUnitTable.VmsUnitRecord;
        }
    }
}
