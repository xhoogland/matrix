using System.Collections.Generic;
using System.Threading.Tasks;

namespace Matrix.Interfaces
{
    public interface LiveDataParser
    {
        Task<IEnumerable<LiveData>> RetrieveLiveDataFromContent();

        void DownloadImportableFile();
    }
}
