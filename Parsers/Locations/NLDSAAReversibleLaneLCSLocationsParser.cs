using Matrix.FileModels.Locations.NLDLCS;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Matrix.Parsers.Locations
{
    public class NLDSAAReversibleLaneLCSLocationsParser : BaseParser, LocationParser
    {
        public NLDSAAReversibleLaneLCSLocationsParser(string downloadLocation, string fileLocation)
            : base(downloadLocation, fileLocation)
        {
        }

        public async Task<IEnumerable<Location>> RetrieveLocationsFromContent()
        {
            var jsonContent = await File.ReadAllTextAsync(_fileLocation);
            var data = JsonConvert.DeserializeObject<NLDLCSLocations>(jsonContent);
            return data.Features;
        }
    }
}
