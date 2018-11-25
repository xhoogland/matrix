using Matrix.FileModels.LiveData.BELLCS;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Matrix.Parsers.LiveData
{
    public class BELAllLCSLiveDataParser : BaseParser, LiveDataParser
    {
        public BELAllLCSLiveDataParser(string downloadLocation, string fileLocation)
            : base(downloadLocation, fileLocation)
        {
        }

        public async Task<IEnumerable<Interfaces.LiveData>> RetrieveLiveDataFromContent()
        {
            var xmlFile = await File.ReadAllTextAsync(_fileLocation);

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlFile);
            var xmlAsJsonContent = JsonConvert.SerializeXmlNode(xmlDocument);

            var atSignRegex = new Regex("\"@([a-z])");
            xmlAsJsonContent = atSignRegex.Replace(xmlAsJsonContent, "\"$1");

            var data = JsonConvert.DeserializeObject<RootObject>(xmlAsJsonContent);
            return data.RssVerkeersData.Rss_Bord;
        }
    }
}
