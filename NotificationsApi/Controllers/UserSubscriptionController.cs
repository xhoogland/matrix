using Matrix.FileModels.Configuration;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IoFile = System.IO.File;

namespace Matrix.NotificationsApi.Controllers
{
    [Route("api/usersubscription")]
    public class UserSubscriptionController : Controller
    {
        private readonly string _userSubscriptionsPath;
        private readonly string _locationsPath;

        public UserSubscriptionController()
        {
            _userSubscriptionsPath = GetPaths().First();
            _locationsPath = GetPaths().Last();
        }

        // POST api/usersubscription
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]NotificationSubscription notificationSubscription)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var locationsFile = await IoFile.ReadAllTextAsync(_locationsPath);
            var locations = JsonConvert.DeserializeObject<IEnumerable<VariableMessageSignPortal>>(locationsFile, settings);
            var chosenRoadWay = locations.SelectMany(l => l.RoadWays).FirstOrDefault(r => r.HmLocation == notificationSubscription.HmLocation);

            var userSubscriptions = await GetPushUserSubscriptionsFromFile(notificationSubscription);
            var subscription = userSubscriptions.FirstOrDefault(u => JsonConvert.SerializeObject(u.PushSubscription) == JsonConvert.SerializeObject(notificationSubscription.PushSubscription));
            if (subscription == null)
            {
                subscription = new PushUser
                {
                    PushSubscription = notificationSubscription.PushSubscription
                };
                userSubscriptions.Add(subscription);
            }

            if (!subscription.RoadWays.Any(r => r.HmLocation == chosenRoadWay.HmLocation))
            {
                subscription.RoadWays.Add(chosenRoadWay);
            }
            else
            {
                return NoContent();
            }

            await IoFile.WriteAllTextAsync(_userSubscriptionsPath, JsonConvert.SerializeObject(userSubscriptions));

            return Created(new Uri("http://place.holder"), chosenRoadWay.HmLocation);
        }

        // DELETE api/usersubscription
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody]NotificationSubscription notificationSubscription)
        {
            var userSubscriptions = await GetPushUserSubscriptionsFromFile(notificationSubscription);
            var subscription = userSubscriptions.FirstOrDefault(u => JsonConvert.SerializeObject(u.PushSubscription) == JsonConvert.SerializeObject(notificationSubscription.PushSubscription));
            if (subscription == null)
            {
                return NoContent();
            }

            if (subscription.RoadWays.Any(r => r.HmLocation == notificationSubscription.HmLocation))
            {
                subscription.RoadWays = subscription.RoadWays.Where(r => r.HmLocation != notificationSubscription.HmLocation).ToList();
            }
            else
            {
                return NoContent();
            }

            if(!subscription.RoadWays.Any())
            {
                userSubscriptions = userSubscriptions.Where(u => JsonConvert.SerializeObject(u.PushSubscription) != JsonConvert.SerializeObject(notificationSubscription.PushSubscription)).ToList();
            }

            await IoFile.WriteAllTextAsync(_userSubscriptionsPath, JsonConvert.SerializeObject(userSubscriptions));

            return Ok();
        }

        [HttpOptions]
        public IActionResult Options()
        {
            return Ok();
        }

        private IEnumerable<string> GetPaths()
        {
            var paths = new List<string>();

            var currentDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var configFile = IoFile.ReadAllText(Path.Combine(currentDirectory, "config.json"));
            var config = JsonConvert.DeserializeObject<Config>(configFile);
            if (config.StartPath.StartsWith("__") && config.StartPath.EndsWith("__"))
                config.StartPath = Directory.GetCurrentDirectory();
            if (config.DataPath.StartsWith("__") && config.DataPath.EndsWith("__"))
                config.DataPath = string.Empty;
            if (config.LocationsPath.StartsWith("__") && config.LocationsPath.EndsWith("__"))
                config.LocationsPath = Path.Combine("..", "LocationsGenerator", "bin", "Debug", "netcoreapp2.0");

            paths.Add(Path.Combine(config.StartPath, config.DataPath, "userSubscriptions.json"));
            paths.Add(Path.Combine(config.StartPath, config.LocationsPath, "locations.json"));

            return paths;
        }

        private async Task<ICollection<PushUser>> GetPushUserSubscriptionsFromFile(NotificationSubscription notificationSubscription)
        {
            var userSubscriptionsFile = await IoFile.ReadAllTextAsync(_userSubscriptionsPath);
            ICollection<PushUser> userSubscriptions;
            try
            {
                userSubscriptions = JsonConvert.DeserializeObject<ICollection<PushUser>>(userSubscriptionsFile);
            }
            catch (JsonSerializationException)
            {
                userSubscriptions = new Collection<PushUser>();
            }

            return userSubscriptions;
        }
    }
}
