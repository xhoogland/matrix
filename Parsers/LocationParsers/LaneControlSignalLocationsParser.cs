using Harlow;
using Matrix.FileModels.LaneControlSignalLocations;
using Matrix.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Matrix.Parsers.LocationParsers
{
    public class LaneControlSignalLocationsParser : LocationParser
    {
        public IEnumerable<Location> Locations { get; }

        public LaneControlSignalLocationsParser(string fileLocation)
        {
            var fileLocationSplit = fileLocation.Split(Path.DirectorySeparatorChar);
            ZipFile.ExtractToDirectory(fileLocation, fileLocationSplit[0], true);
            var shapeFileDirectory = Path.Combine(fileLocationSplit[0], "MSI");
            var shapeFileLocation = Directory.EnumerateFiles(shapeFileDirectory, "*.shp").First();
            var shape = new ShapeFileReader(shapeFileLocation);

            var jsonContent = shape.FeaturesAsJson().Replace("\\u0000", string.Empty);
            jsonContent = MapJsonToCorrectFormat(jsonContent);
            Locations = GetLocationsByFileContent(jsonContent);
        }

        private string MapJsonToCorrectFormat(string jsonContent)
        {
            var harlowFeatures = JsonConvert.DeserializeObject<IEnumerable<HarlowFeature>>(jsonContent);
            var features = new List<Feature>();
            foreach (var harlowFeature in harlowFeatures)
            {
                var feature = new Feature
                {
                    Geometry = new Geometry
                    {
                        Coordinates = harlowFeature.Coordinates
                    },
                    Properties = harlowFeature.Properties,
                    Type = harlowFeature.Type
                };

                features.Add(feature);
            }

            return JsonConvert.SerializeObject(features);
        }

        public IEnumerable<Location> GetLocationsByFileContent(string fileContent)
        {
            var data = JsonConvert.DeserializeObject<IEnumerable<Feature>>(fileContent);
            return data;
        }
    }
}
