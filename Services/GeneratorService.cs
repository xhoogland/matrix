using Matrix.FileModels.Configuration;
using Matrix.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Matrix.Services
{
    public class GeneratorService<TParserInterface> where TParserInterface : Parser
    {
        private readonly string _currentDirectory;
        private readonly Config _config;

        public JsonSerializerSettings JsonConfig { get; }

        public string SavePath { get; }
        public string ApiUrl { get; }

        public GeneratorService(TypeNameHandling typeNameHandling = TypeNameHandling.None)
        {
            var cultureInfo = new CultureInfo("nl-NL");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;

            _currentDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            _config = GetConfig();
            JsonConfig = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = typeNameHandling
            };
            SavePath = Path.Combine(_config.StartPath, _config.DataPath);
            ApiUrl = _config.Url;
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
            if (config.StartPath != null && config.StartPath.StartsWith("__") && config.StartPath.EndsWith("__"))
                config.StartPath = Directory.GetCurrentDirectory();
            if (config.DataPath != null && config.DataPath.StartsWith("__") && config.DataPath.EndsWith("__"))
                config.DataPath = string.Empty;
            if (config.Url != null && config.Url.StartsWith("__") && config.Url.EndsWith("__"))
                config.Url = "http://localhost:52486";

            return config;
        }

        public void WriteJsonFile(string json, string fileName)
        {
            var fullPathToFile = Path.Combine(_config.StartPath, _config.DataPath);
            if (!Directory.Exists(fullPathToFile))
                Directory.CreateDirectory(fullPathToFile);

            File.WriteAllText(Path.Combine(fullPathToFile, fileName), json);
        }

        private TParserInterface CreateObjectInstance(Type type)
        {
            var flProperties = _config.SaveFileName.GetType().GetProperties();
            var dlProperties = _config.DownloadUrl.GetType().GetProperties();

            // Retrieves property in class named after specified type
            bool getPropertyByType(Type pType, PropertyInfo propertyInfo) => pType.ToString().Contains(string.Format(".{0}", propertyInfo.Name));

            var flProperty = flProperties.First(p => getPropertyByType(type, p));
            var dlProperty = dlProperties.First(p => getPropertyByType(type, p));

            var filePath = Path.Combine(_currentDirectory, "Import", flProperty.GetValue(_config.SaveFileName).ToString());

            var downloadUrl = dlProperty.GetValue(_config.DownloadUrl);

            var objectInstance = Activator.CreateInstance(type, downloadUrl, filePath);
            return (TParserInterface)objectInstance;
        }

        public IList<TParserInterface> GetParserImplementations()
        {
            var interfaceImplemented = typeof(TParserInterface);
            Assembly.Load("Parsers");
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
            var derivedClasses = assemblies.Where(t => interfaceImplemented.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            var objectInstances = derivedClasses.Select(CreateObjectInstance).ToList();

            DownloadDataForImport(objectInstances);
            return objectInstances;
        }

        private static void DownloadDataForImport(IList<TParserInterface> locationParsers)
        {
            var importDirectory = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Import");
            Directory.CreateDirectory(importDirectory);
            foreach (var locationParser in locationParsers)
            {
                locationParser.DownloadImportableFile();
            }
        }
    }
}
