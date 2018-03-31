using System.Collections.Generic;

namespace Matrix.Interfaces
{
    public interface LocationParser
    {
        IEnumerable<Location> Locations { get; }

        IEnumerable<Location> GetLocationsByFileContent(string fileContent);
    }
}
