using Matrix.Interfaces;
using System;
using System.Linq;

namespace LocationsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var interfaceImplemented = typeof(LocationParser);
            var parsers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes())
                          .Where(t => interfaceImplemented.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                          .Select(t => Activator.CreateInstance(t)).ToList();
            
            foreach (LocationParser parser in parsers)
            {
                foreach (Location location in parser.Locations)
                {
                    if (!location.HasCoordinates)
                        continue;

                    var km = location.Km;
                    var lane = location.Lane;
                    var loc = location.Coordinates;
                    var roadName = location.RoadName;
                    var roadSide = location.RoadSide;
                    var uuid = location.Id;
                }
            }
        }
    }
}
