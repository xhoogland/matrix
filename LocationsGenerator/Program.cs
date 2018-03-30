using Matrix.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LocationsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var interfaceImplemented = typeof(LocationParser);
            //var a = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes());
            var assemblyTypes = AppDomain.CurrentDomain.GetAssemblies().Select(a => a);
            var parsers = new List<LocationParser>();
            foreach (object assemblyType in assemblyTypes)
            {
                var o = assemblyType as LocationParser;
                if (o != null)
                {
                    parsers.Add(o);
                }
            }

            //var classes = assemblies
            //    .Where(t => interfaceImplemented.IsAssignableFrom(t));// && !t.IsAbstract && !t.IsInterface);
           // var parsers = classes.Select(Activator.CreateInstance).ToList();

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
