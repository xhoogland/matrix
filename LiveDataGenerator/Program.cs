using Matrix.Interfaces;
using Matrix.SpecificImplementations;
using Matrix.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LiveDataGenerator
{
    class Program
    {
        private static string SavePath;

        static void Main(string[] args)
        {
            var serviceHandler = new ServiceHandler<LiveDataParser>();
            SavePath = serviceHandler.SavePath;

            var liveDataParsers = serviceHandler.GetParserImplementations();

            DownloadDataForImport(liveDataParsers);

            var liveData = FillLiveDataAsync(liveDataParsers, serviceHandler.SavePath).Result;

            var json = JsonConvert.SerializeObject(liveData, serviceHandler.JsonConfig);
            serviceHandler.WriteJsonFile(json, "liveData.json");
        }

        // TODO: Deduplicate!
        private static void DownloadDataForImport(IList<LiveDataParser> liveDataParsers)
        {
            var importDirectory = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Import");
            Directory.CreateDirectory(importDirectory);
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

            var fullJsonPath = Path.Combine(SavePath, "liveData.json");
            IEnumerable<VariableMessageSign> currentJson = new List<VariableMessageSign>();
            if (File.Exists(fullJsonPath))
            {
                var jsonFile = await File.ReadAllTextAsync(fullJsonPath);
                currentJson = JsonConvert.DeserializeObject<IEnumerable<VariableMessageSign>>(jsonFile);
            }

            foreach (var liveObject in allLiveData)
            {
                var sign = string.Empty;
                if (!liveObject.IsValid)
                    continue;
                else
                {
                    var prefix = "TP|";
                    if (liveObject.Sign.StartsWith(prefix))
                    {
                        sign = liveObject.Sign.Remove(0, prefix.Length);
                    }
                    else
                    {
                        if (liveObject.Sign.Length > 35)
                        {
                            var fullFilePath = Path.Combine(vmsPath, StripTextOfInvalidCharsForSaveToFileSystem(liveObject.Id));
                            var oldObject = currentJson.FirstOrDefault(v => v.Id == liveObject.Id);
                            var base64SignLength = liveObject.Sign.Length.ToString();
                            if (oldObject == null || oldObject.Sign != base64SignLength)
                                await File.WriteAllBytesAsync(fullFilePath, Convert.FromBase64String(liveObject.Sign));

                            sign = base64SignLength;
                        }
                        else
                        {
                            sign = liveObject.Sign;
                        }
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
