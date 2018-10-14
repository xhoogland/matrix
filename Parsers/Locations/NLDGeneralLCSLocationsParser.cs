using Harlow;
using Matrix.FileModels.Locations.NLDLCS;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Matrix.Parsers.Locations
{
    public class NLDGeneralLCSLocationsParser : BaseParser, LocationParser
    {
        public NLDGeneralLCSLocationsParser(string downloadLocation, string fileLocation)
            : base(downloadLocation, fileLocation)
        {
        }

        private string MapJsonToCorrectFormat(string jsonContent)
        {
            var harlowFeatures = JsonConvert.DeserializeObject<IEnumerable<HarlowFeature>>(jsonContent);
            var features = new List<Feature>();
            foreach (var harlowFeature in harlowFeatures)
            {
                var properties = harlowFeature.Properties;
                properties.Road = properties.Road.ToUpper();
                var cw = properties.Carriagew0.Trim().ToLower();
                properties.Carriagew0 = cw == "l" || cw == "r" ? cw.ToUpper() : cw;
                var feature = new Feature
                {
                    Geometry = new Geometry
                    {
                        Coordinates = harlowFeature.Coordinates
                    },
                    Properties = properties,
                    Type = harlowFeature.Type
                };

                features.Add(feature);
            }

            return JsonConvert.SerializeObject(features);
        }

        public async Task<IEnumerable<Location>> RetrieveLocationsFromContent()
        {
            var fileLocationSplit = _fileLocation.Split(Path.DirectorySeparatorChar);
            var downloadLocationSplit = _downloadLocation.ToString().Split('/');
            var importFolder = Directory.GetParent(_fileLocation).FullName;
            await Task.Run(() =>
            {
                ZipFile.ExtractToDirectory(_fileLocation, importFolder, true);
                var renamedNameOfFolder = Path.Combine(importFolder, fileLocationSplit.Last().Split('.').First());
                if (Directory.Exists(renamedNameOfFolder))
                    Directory.Delete(renamedNameOfFolder, true);

                Directory.Move(Path.Combine(importFolder, downloadLocationSplit.Last().Split('.').First()), renamedNameOfFolder);
            });
            var extractFolderName = Path.GetFileNameWithoutExtension(_fileLocation);
            var shapeFileDirectory = Path.Combine(importFolder, extractFolderName, "MSI");
            var shapeFileLocation = Directory.EnumerateFiles(shapeFileDirectory, "*.shp").First();
            var shape = new ShapeFileReader(shapeFileLocation);

            var featuresAsJson = await Task.Run(() => shape.FeaturesAsJson());
            var jsonContent = featuresAsJson.Replace("\\u0000", string.Empty);
            jsonContent = MapJsonToCorrectFormat(jsonContent);

            var data = JsonConvert.DeserializeObject<IEnumerable<Feature>>(jsonContent);
            return data;
        }
    }
}
