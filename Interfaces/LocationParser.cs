using System.Collections.Generic;
using System.Threading.Tasks;

namespace Matrix.Interfaces
{
    public interface LocationParser
    {
        Task<IEnumerable<Location>> RetrieveLocationsFromContent();
    }
}
