using Matrix.FileModels;
using System.Collections.Generic;

namespace Matrix.Parsers
{
    public interface ILocationParser
    {
        IEnumerable<ILocation> Locations { get; }
    }
}
