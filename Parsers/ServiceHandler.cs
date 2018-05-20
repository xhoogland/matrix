using Matrix.FileModels.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Matrix.SpecificImplementations
{
    public class ServiceHandler<TParserInterface>
    {
        private readonly string _currentDirectory;
        private readonly Config _config;

        public JsonSerializerSettings JsonConfig { get; }

        public string SavePath { get; }

        public ServiceHandler()
        {
            _currentDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            _config = GetConfig();
            JsonConfig = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            SavePath = Path.Combine(_config.StartPath, _config.DataPath);
        }

        private Config GetConfig()
        {
            var configFile = JObject.Parse(File.ReadAllText(Path.Combine(_currentDirectory, "config.json")));
            var configPrivateFile = JObject.Parse(File.ReadAllText(Path.Combine(_currentDirectory, "config.private.json")));
            configFile.Merge(configPrivateFile, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });
            var config = JsonConvert.DeserializeObject<Config>(configFile.ToString());
            if (config.StartPath.StartsWith("__") && config.StartPath.EndsWith("__"))
                config.StartPath = Directory.GetCurrentDirectory();
            if (config.DataPath.StartsWith("__") && config.DataPath.EndsWith("__"))
                config.DataPath = string.Empty;

            return config;
        }

        public void WriteJsonFile(string json, string fileName)
        {
            File.WriteAllText(Path.Combine(_config.StartPath, _config.DataPath, fileName), json);
        }

        private TParserInterface CreateObjectInstance(Type type)
        {
            var flProperties = _config.SaveFileName.GetType().GetProperties();
            var dlProperties = _config.DownloadUrl.GetType().GetProperties();

            bool getPropertyByType(Type pType, PropertyInfo propertyInfo) => pType.ToString().Contains(string.Format(".{0}", propertyInfo.Name));
            var flProperty = flProperties.First(p => getPropertyByType(type, p));
            var dlProperty = flProperties.First(p => getPropertyByType(type, p));

            var filePath = Path.Combine(_currentDirectory, "Import", flProperty.GetValue(_config.SaveFileName).ToString());

            var downloadUrl = flProperty.GetValue(_config.DownloadUrl);
            if (downloadUrl != null)
                downloadUrl = downloadUrl.ToString();

            var objectInstance = Activator.CreateInstance(type, downloadUrl, filePath);
            return (TParserInterface)objectInstance;
        }

        public IList<TParserInterface> GetParserImplementations()
        {
            var interfaceImplemented = typeof(TParserInterface);

            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                          .Where(t => interfaceImplemented.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                          .Select(CreateObjectInstance).ToList();
        }
    }
}
