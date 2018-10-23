using Matrix.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using IoFile = System.IO.File;

namespace Matrix.NotificationsApi.Controllers
{
    [Route("api/usersubscription")]
    public class UserSubscriptionController : Controller
    {
        private readonly string _subscriptionsPath;
        private readonly string _locationsPath;

        public UserSubscriptionController()
        {
            var config = Startup.GetConfig();
            _subscriptionsPath = Path.Combine(config.SubscriptionsPath, "userSubscriptions.json");
            _locationsPath = Path.Combine(config.LocationsPath, "locations.json");
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
            var locations = JsonConvert.DeserializeObject<IEnumerable<VariableMessageSignPortal>>(locationsFile, settings).ToList();
            var roadWay = locations.SelectMany(l => l.RoadWays).FirstOrDefault(r => r.HmLocation == notificationSubscription.HmLocation);
            var chosenRoadWay = new PushRoadWay
            {
                Coordinates = locations.FirstOrDefault(l => l.RoadWays.Contains(roadWay)).Coordinates,
                HmLocation = roadWay.HmLocation,
                VariableMessageSigns = roadWay.VariableMessageSigns
            };

            var userSubscriptions = await GetPushUserSubscriptionsFromFile();
            var subscription = userSubscriptions.FirstOrDefault(u => JsonConvert.SerializeObject(u.PushSubscription) == JsonConvert.SerializeObject(notificationSubscription.PushSubscription));
            if (subscription == null)
            {
                subscription = new PushUser
                {
                    PushSubscription = notificationSubscription.PushSubscription
                };
                userSubscriptions.Add(subscription);
            }

            if (subscription.RoadWays.All(r => r.HmLocation != chosenRoadWay.HmLocation))
            {
                subscription.RoadWays.Add(chosenRoadWay);
            }
            else
            {
                return Conflict();
            }

            await WritePushUserSubscriptionsToFile(userSubscriptions);

            return Created(new Uri("http://place.holder"), chosenRoadWay.HmLocation);
        }

        // DELETE api/usersubscription
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody]NotificationSubscription notificationSubscription)
        {
            var userSubscriptions = await GetPushUserSubscriptionsFromFile();
            var subscription = userSubscriptions.FirstOrDefault(u => JsonConvert.SerializeObject(u.PushSubscription) == JsonConvert.SerializeObject(notificationSubscription.PushSubscription));
            if (subscription == null)
            {
                return Gone();
            }

            if (subscription.RoadWays.Any(r => r.HmLocation == notificationSubscription.HmLocation))
            {
                subscription.RoadWays = subscription.RoadWays.Where(r => r.HmLocation != notificationSubscription.HmLocation).ToList();
            }
            else
            {
                return Conflict();
            }

            if (!subscription.RoadWays.Any())
            {
                userSubscriptions = userSubscriptions.Where(u => JsonConvert.SerializeObject(u.PushSubscription) != JsonConvert.SerializeObject(notificationSubscription.PushSubscription)).ToList();
                await WritePushUserSubscriptionsToFile(userSubscriptions);

                return NoContent();
            }

            await WritePushUserSubscriptionsToFile(userSubscriptions);
            return Ok();
        }

        // GET api/usersubscription
        [HttpGet]
        public async Task<IActionResult> Get(string pushSubscriptionEndpoint)
        {
            var endpoint = HttpUtility.UrlDecode(pushSubscriptionEndpoint);
            var pushUserSubscriptions = await GetPushUserSubscriptionsFromFile();
            var userSubscription = pushUserSubscriptions.FirstOrDefault(p => p.PushSubscription.Endpoint == endpoint);
            if (userSubscription == null)
                return Gone();

            var roadWays = userSubscription.RoadWays;
            var list = new HmLocationList
            {
                NotificationList = roadWays.Select(k => k.HmLocation)
            };
            return Ok(JsonConvert.SerializeObject(list));
        }

        public class HmLocationList
        {
            public IEnumerable<string> NotificationList { get; set; }
        }

        [HttpOptions]
        public IActionResult Options()
        {
            return Ok();
        }

        private async Task<ICollection<PushUser>> GetPushUserSubscriptionsFromFile()
        {
            var userSubscriptionsFile = await IoFile.ReadAllTextAsync(_subscriptionsPath);
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

        private async Task WritePushUserSubscriptionsToFile(ICollection<PushUser> userSubscriptions)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var serializedObject = JsonConvert.SerializeObject(userSubscriptions, jsonSerializerSettings);

            await IoFile.WriteAllTextAsync(_subscriptionsPath, serializedObject);
        }

        /// <summary>
        /// 410
        /// </summary>
        /// <returns></returns>
        private StatusCodeResult Gone()
        {
            return StatusCode(410);
        }
    }
}
