﻿using Matrix.Interfaces;
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
        static void Main(string[] args)
        {
            var serviceHandler = new ServiceHandler<LiveDataParser>();

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
                        if(liveObject.Sign.Length > 35)
                        {
                            var fullPath = Path.Combine(vmsPath, StripTextOfInvalidCharsForSaveToFileSystem(liveObject.Id));
                            File.WriteAllBytes(fullPath, Convert.FromBase64String(liveObject.Sign));
                            sign = liveObject.Id;
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
