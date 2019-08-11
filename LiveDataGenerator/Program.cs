using Matrix.Interfaces;
using Matrix.Services;
using Matrix.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Matrix.LiveDataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceHandler = new GeneratorService<LiveDataParser>();

            var liveDataParsers = serviceHandler.GetParserImplementations();

            var liveData = FillLiveDataAsync(liveDataParsers, serviceHandler.SavePath).GetAwaiter().GetResult();

            var liveDataJson = JsonConvert.SerializeObject(liveData, serviceHandler.JsonConfig);
            serviceHandler.WriteJsonFile(liveDataJson, serviceHandler.SavePath, "liveData.json");
            var historyPath = Path.Combine(serviceHandler.SavePath, "history");
            string historyJson;
            var mostRecentHistoryFile = new DirectoryInfo(historyPath).GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            if (mostRecentHistoryFile == null)
            {
                historyJson = string.Empty;
            }
            else
            {
                historyJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<IEnumerable<VariableMessageSign>>(File.ReadAllTextAsync(mostRecentHistoryFile.FullName).GetAwaiter().GetResult()));
            }
            
            if (historyJson != liveDataJson)
            {
                var fileName = string.Format("{0}.json", DateTime.UtcNow.ToString("yyyy-MM-ddTHH_mm"));
                serviceHandler.WriteJsonFile(liveDataJson, historyPath, fileName);
            }

            TriggerNotificationSending(serviceHandler.ApiUrl);
        }

        public static void TriggerNotificationSending(string apiUrl)
        {
            var webRequest = WebRequest.Create(string.Format("{0}/api/notification", apiUrl));
            webRequest.Method = "POST";
            var bytes = Encoding.ASCII.GetBytes(string.Empty);

            using (var requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            webRequest.GetResponse();
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

            var fullPath = Path.Combine(savePath, "liveData.json");
            IEnumerable<VariableMessageSign> currentJson = new List<VariableMessageSign>();
            if (File.Exists(fullPath))
            {
                var jsonFile = await File.ReadAllTextAsync(fullPath);
                currentJson = JsonConvert.DeserializeObject<IEnumerable<VariableMessageSign>>(jsonFile);
            }

            foreach (var liveObject in allLiveData)
            {
                string sign;
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
