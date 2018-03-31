using Matrix.Interfaces;
using Matrix.LocationsGenerator.Configuration;
using Matrix.Parsers;
using Matrix.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Matrix.LocationsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            var dirName = "Import";
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
#endif

            var config = File.ReadAllText("config.json");
            var fileLocation = JsonConvert.DeserializeObject<Config>(config).FileLocation;

            // By calling something from the Parsers-dll, we ensure having it - and
            // used types - available in the list returned by GetAssemblies.
            var parsersName = ParserHelper.GetAssemblyName();

            var interfaceImplemented = typeof(LocationParser);
            var parsers = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName == parsersName).SelectMany(a => a.GetTypes())
                          .Where(t => interfaceImplemented.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                          .Select(t => Activator.CreateInstance(t, fileLocation.GetType().GetProperties().Where(tt => t.ToString().Contains(tt.Name)).First().GetValue(fileLocation))).ToList();

            var portals = new List<VariableMessageSignPortal>();
            foreach (LocationParser parser in parsers)
            {
                foreach (Location location in parser.Locations.OrderBy(l => l.Lane))
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
            }

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(portals, settings);

            File.WriteAllText("VariableMessageSignPortalLocations.js", json);
        }
    }
}
