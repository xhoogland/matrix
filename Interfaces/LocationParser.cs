using System.Collections.Generic;
using System.Threading.Tasks;

namespace Matrix.Interfaces
{
    public interface LocationParser : Parser
    {
        Task<IEnumerable<Location>> RetrieveLocationsFromContent();
    }
}
