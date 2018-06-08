using Matrix.Interfaces;
using Matrix.SpecificImplementations;
using Matrix.ViewModels;
using Newtonsoft.Json;
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
            var serviceHandler = new ServiceHandler<LocationParser>(TypeNameHandling.Auto);

            var locationParsers = serviceHandler.GetParserImplementations();

            DownloadDataForImport(locationParsers);

            var portalLocations = FillPortalsAsync(locationParsers).Result;

            var json = JsonConvert.SerializeObject(portalLocations, serviceHandler.JsonConfig);
            serviceHandler.WriteJsonFile(json, "locations.json");
        }

        // TODO: Deduplicate!
        private static void DownloadDataForImport(IList<LocationParser> locationParsers)
        {
            var importDirectory = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Import");
            Directory.CreateDirectory(importDirectory);
            foreach (var locationParser in locationParsers)
            {
                locationParser.DownloadImportableFile();
            }
        }

        private static async Task<List<VariableMessageSignPortal>> FillPortalsAsync(IList<LocationParser> locationParsers)
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

        private static void AddVmsToPortal(IList<VariableMessageSignPortal> portals, bool isLaneSpecific, int? lane, Coordinates coordinates, string id, string hmLocation)
        {
            VariableMessageSign vms;
            if (isLaneSpecific)
            {
                vms = new LaneControlSignal
                {
                    Id = id,
                    Number = lane.Value

                };
            }
            else
            {
                vms = new VariableMessageSign
                {
                    Id = id
                };
            }

            var portal = portals.FirstOrDefault(p => p.Coordinates.AreCoordinatesInRange(coordinates) && p.IsLaneSpecific == isLaneSpecific);
            if (portal == null)
            {
                portal = new VariableMessageSignPortal
                {
                    Coordinates = coordinates,
                    Country = "NL",
                    IsLaneSpecific = isLaneSpecific
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
    }
}
