using Matrix.FileModels.Configuration;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using IoFile = System.IO.File;

namespace Matrix.NotificationsApi.Controllers
{
    [Route("api/usersubscription")]
    public class UserSubscriptionController : Controller
    {
        private readonly Config _config;

        public UserSubscriptionController()
        {
            _config = Startup.GetConfig();
        }

        // POST api/usersubscription
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]NotificationSubscription notificationSubscription)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var locationsFile = await IoFile.ReadAllTextAsync(_config.LocationsPath);
            var locations = JsonConvert.DeserializeObject<IEnumerable<VariableMessageSignPortal>>(locationsFile, settings);
            var roadWay = locations.SelectMany(l => l.RoadWays).FirstOrDefault(r => r.HmLocation == notificationSubscription.HmLocation);
            var chosenRoadWay = new PushRoadWay
            {
                Coordinates = locations.FirstOrDefault(l => l.RoadWays.Contains(roadWay)).Coordinates,
                HmLocation = roadWay.HmLocation,
                VariableMessageSigns = roadWay.VariableMessageSigns
            };

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

            await IoFile.WriteAllTextAsync(_config.SubscriptionsPath, JsonConvert.SerializeObject(userSubscriptions, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            }));

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

            await IoFile.WriteAllTextAsync(_config.SubscriptionsPath, JsonConvert.SerializeObject(userSubscriptions, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            }));

            return Ok();
        }

        [HttpOptions]
        public IActionResult Options()
        {
            return Ok();
        }

        private async Task<ICollection<PushUser>> GetPushUserSubscriptionsFromFile(NotificationSubscription notificationSubscription)
        {
            var userSubscriptionsFile = await IoFile.ReadAllTextAsync(_config.SubscriptionsPath);
            ICollection<PushUser> userSubscriptions;
            try
            {
                userSubscriptions = JsonConvert.DeserializeObject<ICollection<PushUser>>(userSubscriptionsFile, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            catch (JsonSerializationException)
            {
                userSubscriptions = new Collection<PushUser>();
            }

            return userSubscriptions;
        }
    }
}
