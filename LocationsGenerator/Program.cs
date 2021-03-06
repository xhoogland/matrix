﻿using Matrix.Interfaces;
using Matrix.Services;
using Matrix.ViewModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Matrix.LocationsGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var serviceHandler = new GeneratorService<LocationParser>(TypeNameHandling.Auto);

            var locationParsers = serviceHandler.GetParserImplementations();

            var portalLocations = FillPortalsAsync(locationParsers).Result;

            var json = JsonConvert.SerializeObject(portalLocations, serviceHandler.JsonConfig);
            serviceHandler.WriteJsonFile(json, serviceHandler.SavePath, "locations.json");
        }

        private static async Task<List<VariableMessageSignPortal>> FillPortalsAsync(IList<LocationParser> locationParsers)
        {
            var retrieveLocationsTasks = new List<Task<IEnumerable<Location>>>();
            foreach (var locationParser in locationParsers)
            {
                retrieveLocationsTasks.Add(locationParser.RetrieveLocationsFromContent());
            }

            var locationsWhenAll = await Task.WhenAll(retrieveLocationsTasks);
            var locations = locationsWhenAll.SelectMany(result => result).OrderBy(l => string.Format("{0} {1} {2}", l.RoadName, l.RoadSide, l.Km));
            var portals = new List<VariableMessageSignPortal>();
            foreach (var location in locations.OrderBy(l => l.Lane))
            {
                if (!location.IsValid)
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
                var country = location.Country;

                AddVmsToPortal(portals, isLaneSpecific, lane, coordinates, id, hmLocation, country);
            }

            return portals;
        }

        private static void AddVmsToPortal(IList<VariableMessageSignPortal> portals, bool isLaneSpecific, int? lane, Coordinates coordinates, string id, string hmLocation, string country)
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
                    Country = country,
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
