using System.Collections.Generic;
using System.Threading.Tasks;

namespace Matrix.Interfaces
{
    public interface LiveDataParser : Parser
    {
        Task<IEnumerable<LiveData>> RetrieveLiveDataFromContent();
    }
}
