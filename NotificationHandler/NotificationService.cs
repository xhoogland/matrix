using Matrix.FileModels.Configuration;
using Matrix.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebPush;

namespace Matrix.NotificationHandler
{
    public class NotificationService
    {
        private readonly string _subscriptionsPath;

        private readonly IEnumerable<VariableMessageSign> _liveData;
        private readonly IEnumerable<PushUser> _pushUsers;
        private readonly IEnumerable<PushUser> _lastSentNotifications;
        private readonly WebPushNotification _webPushNotificationSettings;
        private readonly string _webUrl;

        public NotificationService(IEnumerable<VariableMessageSign> liveData, string subscriptionsPath, WebPushNotification webPushNotificationSettings, string webUrl)
        {
            _liveData = liveData;
            _subscriptionsPath = Path.Combine(subscriptionsPath, "userSubscriptions.json");

            _pushUsers = ReadJsonFromFile<PushUser>(_subscriptionsPath).Result;
            _lastSentNotifications = ReadJsonFromFile<PushUser>(_subscriptionsPath).Result;
            _webPushNotificationSettings = webPushNotificationSettings;
            _webUrl = webUrl;
        }

        public void Preprocess()
        {
            foreach (var pushUser in _pushUsers)
            {
                foreach (var roadWay in pushUser.RoadWays)
                {
                    foreach (var vms in roadWay.VariableMessageSigns)
                    {
                        var id = vms.Id;

                        vms.Sign = _liveData.SingleOrDefault(l => l.Id == id).Sign;
                    }
                }
            }
        }

        public void PostProcess()
        { 
            foreach (var pushUser in _lastSentNotifications)
            {
                foreach (var roadWay in pushUser.RoadWays)
                {
                    foreach (var vms in roadWay.VariableMessageSigns)
                    {
                        var id = vms.Id;

                        vms.Sign =  _liveData.SingleOrDefault(l => l.Id == id).Sign;
                    }
                }
            }

            File.WriteAllText(_subscriptionsPath, JsonConvert.SerializeObject(_lastSentNotifications, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            }));
        }

        private string Reformat(string id, string sign)
        {
            var result = string.Empty;
            var resultSb = new StringBuilder();

            if (int.TryParse(sign, out int signLength))
            {
                result = string.Format("live/images/VMS/{0}", id);
            }
            else
            {
                if (sign == "lane_closed")
                    resultSb.Append("\u274C");
                else if (sign == "lane_open")
                    resultSb.Append("\u2B07\uFE0F");
                else if (sign == "restriction_end")
                    resultSb.Append("\uD83D\uDEAB");
                else if (sign.StartsWith("speed_"))
                {
                    resultSb.Append("[");
                    resultSb.Append(sign.Split('_').Last());
                    resultSb.Append("]");
                }
                else if (sign.StartsWith("lane_closed_ahead_merge_"))
                {
                    if (sign.Split('_').Last() == "left")
                        resultSb.Append("\u2199\uFE0F");
                    else
                        resultSb.Append("\u2198\uFE0F");
                }
                else if (sign == "blank")
                    resultSb.Append("[_]");
                else if (sign == "unknown")
                    resultSb.Append("\u2753");
                else
                {
                    resultSb.Append(ChangeHtmlEntityEmojiToCEmoji(sign).Replace("<br />", Environment.NewLine));
                }

                result = resultSb.ToString();
            }

            return result;
        }

        public async Task Send()
        {
            var vapidDetails = new VapidDetails(_webPushNotificationSettings.Subject, _webPushNotificationSettings.PublicKey, _webPushNotificationSettings.PrivateKey);
            var webPushClient = new WebPushClient();

            foreach (var pushUser in _pushUsers)
            {
                var pushSubscription = pushUser.PushSubscription;
                var subscription = new WebPush.PushSubscription(pushSubscription.Endpoint, pushSubscription.Keys.P256dh, pushSubscription.Keys.Auth);

                foreach (var roadWay in pushUser.RoadWays)
                {
                    var previousRoadWay = _lastSentNotifications.FirstOrDefault(l => l.PushSubscription.Endpoint == pushUser.PushSubscription.Endpoint).RoadWays.FirstOrDefault(r => r.HmLocation == roadWay.HmLocation);
                    if (JsonConvert.SerializeObject(roadWay) == JsonConvert.SerializeObject(previousRoadWay))
                        continue;

                    foreach (var vms in roadWay.VariableMessageSigns)
                    {
                        var id = vms.Id;

                        vms.Sign = Reformat(id, _liveData.SingleOrDefault(l => l.Id == id).Sign);
                    }

                    var body = string.Join(' ', roadWay.VariableMessageSigns);
                    string image = null;

                    var lastSign = roadWay.VariableMessageSigns.Last().Sign;
                    var isLaneSpecific = !(lastSign.StartsWith("TP|") || lastSign.StartsWith("live/"));
                    if (!isLaneSpecific)
                    {
                        if (lastSign.StartsWith("TP|"))
                            body = lastSign;
                        else if (lastSign.StartsWith("live/"))
                            image = string.Format("{0}/{1}", _webUrl, lastSign);
                    }

                    var notification = new Notification
                    {
                        Title = roadWay.HmLocation,
                        Icon = "images/xanland.png",
                        Body = isLaneSpecific || lastSign.StartsWith("TP|") ? body : "Klik op de afbeelding voor de volledige beeldstand.",
                        Image = isLaneSpecific ? null : image,
                        Tag = isLaneSpecific ? string.Format("{0}={1}@{2}", roadWay.HmLocation, DateTime.UtcNow.Hour, DateTime.UtcNow.Day) : roadWay.HmLocation,
                        Data = new NotificationData
                        {
                            CoordinatesUrl = roadWay.Coordinates != null ? string.Format("{0}/?lat={1}&lon={2}&zoom=17", _webUrl, roadWay.Coordinates.X.ToString().Replace(',', '.'), roadWay.Coordinates.Y.ToString().Replace(',', '.')) : null,
                        },
                        Actions = new List<NotificationAction>
                        {
                            new NotificationAction
                            {
                                Action = "go-to-location",
                                Icon = "images/xanland.png",
                                Title = "Open locatie"
                            }
                        },
                        HmLocation = roadWay.HmLocation,
                        LanesShownSign = string.Join(' ', roadWay.VariableMessageSigns),
                        DripShownSign = false
                    };

                    try
                    {
                        await webPushClient.SendNotificationAsync(subscription, JsonConvert.SerializeObject(notification, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }), vapidDetails);
                    } catch (Exception) { }
                }
            }

            PostProcess();
        }

        private async Task<IEnumerable<T>> ReadJsonFromFile<T>(string fullJsonPath)
        {
            IEnumerable<T> currentJson = new List<T>();

            if (File.Exists(fullJsonPath))
            {
                var jsonFile = await File.ReadAllTextAsync(fullJsonPath);
                currentJson = JsonConvert.DeserializeObject<IEnumerable<T>>(jsonFile, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }

            return currentJson;
        }

        private string ChangeHtmlEntityEmojiToCEmoji(string vmsTextLine)
        {
            var specialCharactersToReplace = new Dictionary<string, string>
            {
                { "&#11014;", "\u2B06\uFE0F" },
                { "&#8599;", "\u2197\uFE0F" },
                { "&#10035;", "\u2733\uFE0F" },
                { "&#128119;", "\uD83D\uDC77\u200D\u2642\uFE0F" },
                { "&#127359;", "\uD83C\uDD7F\uFE0F" },
            };

            var result = vmsTextLine;
            foreach (var specialChar in specialCharactersToReplace)
            {
                result = result.Replace(specialChar.Key, specialChar.Value);
            }

            return result;
        }
    }
}
