using Matrix.Interfaces;
using Matrix.Parsers;
using System;
using System.Linq;

namespace Matrix.LocationsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // By calling something from the Parsers-dll, we ensure having it - and
            // used types - available in the list returned by GetAssemblies.
            var parsersName = ParserHelper.GetAssemblyName();

            var interfaceImplemented = typeof(LocationParser);
            var parsers = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName == parsersName).SelectMany(t => t.GetTypes())
                          .Where(t => interfaceImplemented.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                          .Select(Activator.CreateInstance).ToList();
            //var parsers = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.FullName == parsersName).ExportedTypes;

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
