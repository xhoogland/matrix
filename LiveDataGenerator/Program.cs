using Matrix.Interfaces;
using Matrix.SpecificImplementations;
using Matrix.ViewModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceHandler = new ServiceHandler<LiveDataParser>();

            var liveDataParsers = serviceHandler.GetParserImplementations();

            DownloadDataForImport(liveDataParsers);

            var liveData = FillLiveDataAsync(liveDataParsers).Result;

            var json = JsonConvert.SerializeObject(liveData, serviceHandler.JsonConfig);
            serviceHandler.WriteJsonFile(json, "liveData.json");
        }

        private static void DownloadDataForImport(IList<LiveDataParser> liveDataParsers)
        {
            Directory.CreateDirectory("Import");
            foreach (var liveDataParser in liveDataParsers)
            {
                liveDataParser.DownloadImportableFile();
            }
        }

        private static async Task<List<VariableMessageSign>> FillLiveDataAsync(IList<LiveDataParser> liveDataParsers)
        {
            var retrieveLiveDataTasks = new List<Task<IEnumerable<LiveData>>>();
            foreach (var locationParser in liveDataParsers)
            {
                retrieveLiveDataTasks.Add(locationParser.RetrieveLiveDataFromContent());
            }

            var liveDataWhenAll = await Task.WhenAll(retrieveLiveDataTasks);
            var allLiveData = liveDataWhenAll.SelectMany(result => result);
            var liveDataList = new List<VariableMessageSign>();
            foreach (var location in allLiveData)
            {

            }

            return liveDataList;
        }
    }
}
