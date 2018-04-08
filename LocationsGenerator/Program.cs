using Matrix.Interfaces;
using Matrix.LocationsGenerator.Configuration;
using Matrix.Parsers;
using Matrix.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Matrix.LocationsGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = File.ReadAllText("config.json");
            var fileLocation = JsonConvert.DeserializeObject<Config>(config).FileLocation;

            // By calling something from the Parsers-dll, we ensure having it - and
            // used types - available in the list returned by GetAssemblies.
            var parsersName = ParserHelper.GetAssemblyName();

            var interfaceImplemented = typeof(LocationParser);
            var parsers = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName == parsersName).SelectMany(a => a.GetTypes())
                          .Where(t => interfaceImplemented.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                          .Select(t => CreateObjectInstance(t, fileLocation)).ToList();

            var portals = FillPortals(parsers);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(portals, settings);

            File.WriteAllText("variableMessageSignPortalLocations.json", json);
        }

        private static List<VariableMessageSignPortal> FillPortals(List<LocationParser> locationParsers)
        {
            return FillPortalsAsync(locationParsers).Result;
        }

        private static async Task<List<VariableMessageSignPortal>> FillPortalsAsync(List<LocationParser> locationParsers)
        {
            var retrieveLocationsTasks = new List<Task<IEnumerable<Location>>>();
            foreach (var locationParser in locationParsers)
            {
                retrieveLocationsTasks.Add(locationParser.RetrieveLocationsFromContent());
            }

            var locationsWhenAll = await Task.WhenAll(retrieveLocationsTasks);
            var locations = locationsWhenAll.SelectMany(result => result);
            var portals = new List<VariableMessageSignPortal>();
            foreach (var location in locations.OrderBy(l => l.Lane))
            {
                if (!location.HasCoordinates)
                    continue;

                var isLaneSpecific = location.IsLaneSpecific;
                var lane = location.Lane;
                if (isLaneSpecific && !lane.HasValue)
                    continue;

                var km = location.Km;
                var coordinates = location.Coordinates;
                var roadName = location.RoadName;
                var roadSide = location.RoadSide;
                var id = location.Id;
                var hmLocation = string.Format("{0} {1} {2}", roadName, roadSide, km).Trim();

                AddVmsToPortal(portals, isLaneSpecific, lane, coordinates, id, hmLocation);
            }

            return portals;
        }

        private static void AddVmsToPortal(List<VariableMessageSignPortal> portals, bool isLaneSpecific, int? lane, Coordinates coordinates, string id, string hmLocation)
        {
            VariableMessageSign vms;
            if (isLaneSpecific)
                vms = new LaneControlSignal
                {
                    Id = id,
                    Number = lane.Value

                };
            else
                vms = new VariableMessageSign
                {
                    Id = id
                };

            var portal = portals.FirstOrDefault(p => p.Coordinates.AreCoordinatesInRange(coordinates) && isLaneSpecific);
            if (portal == null)
            {
                portal = new VariableMessageSignPortal
                {
                    Coordinates = coordinates,
                    Country = "NL"
                };
                portals.Add(portal);
            }

            var roadWay = portal.RoadWays.FirstOrDefault(r => r.HmLocation == hmLocation);
            if (roadWay == null)
            {
                roadWay = new RoadWay
                {
                    HmLocation = hmLocation
                };
                portal.RoadWays.Add(roadWay);
            }

            roadWay.VariableMessageSigns.Add(vms);
        }

        private static LocationParser CreateObjectInstance(Type type, FileLocation fileLocation)
        {
            var flProperties = fileLocation.GetType().GetProperties();
            bool getPropertyByType(Type pType, PropertyInfo propertyInfo) => pType.ToString().Contains(string.Format(".{0}", propertyInfo.Name));
            var property = flProperties.First(p => getPropertyByType(type, p));
            var filePath = Path.Combine("Import", property.GetValue(fileLocation).ToString());

            var objectInstance = Activator.CreateInstance(type, filePath);
            return (LocationParser)objectInstance;
        }
    }
}
