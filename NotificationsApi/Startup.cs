using Matrix.FileModels.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace Matrix.NotificationsApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        public static Config GetConfig()
        {
            var currentDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var configFile = JObject.Parse(File.ReadAllText(Path.Combine(currentDirectory, "config.json")));
            var configPrivateFile = JObject.Parse(File.ReadAllText(Path.Combine(currentDirectory, "config.private.json")));
            configFile.Merge(configPrivateFile, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });
            var config = JsonConvert.DeserializeObject<Config>(configFile.ToString());
            if (config.StartPath.StartsWith("__") && config.StartPath.EndsWith("__"))
                config.StartPath = Directory.GetCurrentDirectory();
            if (config.DataPath.StartsWith("__") && config.DataPath.EndsWith("__"))
                config.DataPath = Path.Combine(config.StartPath, "..", "LiveDataGenerator", "bin", "Debug", "netcoreapp2.0");
            if (config.SubscriptionsPath.StartsWith("__") && config.SubscriptionsPath.EndsWith("__"))
                config.SubscriptionsPath = Path.Combine(config.StartPath, "..", "NotificationsApi");
            if (config.LocationsPath.StartsWith("__") && config.LocationsPath.EndsWith("__"))
                config.LocationsPath = Path.Combine(config.StartPath, "..", "LocationsGenerator", "bin", "Debug", "netcoreapp2.0");
            if (config.Url.StartsWith("__") && config.Url.EndsWith("__"))
                config.Url = "https://matrix-vnext.xanland.nl";

            return config;
        }
    }
}
