using Matrix.FileModels;
using Matrix.Parsers;
using System;
using System.Linq;

namespace LocationsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var interfaceImplemented = typeof(ILocationParser);
            var parsers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes())
                          .Where(t => interfaceImplemented.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                          .Select(t => Activator.CreateInstance(t)).ToList();
            
            foreach (ILocationParser parser in parsers)
            {
                foreach (ILocation location in parser.Locations)
                {
                    var km = location.Km;
                    var lane = location.Lane;
                    var loc = location.Location;
                    var roadName = location.RoadName;
                    var roadSide = location.RoadSide;
                    var uuid = location.Id;
                }
            }
        }
    }
}
