using Matrix.Enums;
using Matrix.Interfaces;
using Matrix.SpecificImplementations;
using Matrix.ViewModels;
using Newtonsoft.Json;
using System;
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

            var liveData = FillLiveDataAsync(liveDataParsers, serviceHandler.SavePath).Result;

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

        private static async Task<List<VariableMessageSign>> FillLiveDataAsync(IList<LiveDataParser> liveDataParsers, string savePath)
        {
            var vmsPath = Path.Combine(savePath, "images", "VMS");
            Directory.CreateDirectory(vmsPath);
            var retrieveLiveDataTasks = new List<Task<IEnumerable<LiveData>>>();
            foreach (var locationParser in liveDataParsers)
            {
                retrieveLiveDataTasks.Add(locationParser.RetrieveLiveDataFromContent());
            }

            var liveDataWhenAll = await Task.WhenAll(retrieveLiveDataTasks);
            var allLiveData = liveDataWhenAll.SelectMany(result => result);
            var liveDataList = new List<VariableMessageSign>();
            foreach (var liveObject in allLiveData)
            {
                var sign = string.Empty;
                switch(liveObject.DataType)
                {
                    case DataType.Image:
                    {
                        sign = liveObject.Sign;
                        break;
                    }
                    case DataType.Base64:
                    {
                        var fullPath = Path.Combine(vmsPath, StripTextOfInvalidCharsForSaveToFileSystem(liveObject.Id));
                        File.WriteAllBytes(fullPath, Convert.FromBase64String(liveObject.Sign));
                        sign = liveObject.Id;
                        break;
                    }
                    case DataType.TextPage:
                    {
                        sign = liveObject.Sign.Replace("|", "\r\n");
                        break;
                    }
                    default:
                    {
                        continue;
                    }
                }

                liveDataList.Add(new VariableMessageSign
                {
                    Id = liveObject.Id,
                    Sign = sign
                });
            }

            return liveDataList;
        }

        private static string StripTextOfInvalidCharsForSaveToFileSystem(string text)
        {
            var invalidChars = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).ToArray();
            var resultText = string.Empty;

            foreach (var invalidChar in invalidChars)
            {
                resultText = text.Replace(invalidChar.ToString(), string.Empty);
            }

            return resultText;
        }
    }
}
