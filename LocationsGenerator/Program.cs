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
            var parsers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes()).Where(t => interfaceImplemented.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => Activator.CreateInstance(t)).ToList();
        }
    }
}
