using Matrix.FileModels.Locations.BELLCS;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Matrix.Parsers.Locations
{
    public class BELAllLCSLocationsParser : BaseParser, LocationParser
    {
        public BELAllLCSLocationsParser(string downloadLocation, string fileLocation)
            : base(downloadLocation, fileLocation)
        {
        }

        public async Task<IEnumerable<Location>> RetrieveLocationsFromContent()
        {
            var xmlFile = await File.ReadAllTextAsync(_fileLocation);

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlFile);
            var xmlAsJsonContent = JsonConvert.SerializeXmlNode(xmlDocument);

            var atSignRegex = new Regex("\"@([a-z])");
            xmlAsJsonContent = atSignRegex.Replace(xmlAsJsonContent, "\"$1");

            var data = JsonConvert.DeserializeObject<RootObject>(xmlAsJsonContent);
            return data.RssConfiguratie.Rss_Bord;
        }
    }
}
