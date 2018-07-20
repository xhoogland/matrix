using Matrix.NotificationHandler;
using Matrix.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IoFile = System.IO.File;

namespace Matrix.NotificationsApi.Controllers
{
    [Route("api/notification")]
    public class NotificationController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var config = Startup.GetConfig();
            IEnumerable<VariableMessageSign> liveData = new List<VariableMessageSign>();

            var liveDataPath = Path.Combine(config.DataPath, "liveData.json");
            if (IoFile.Exists(liveDataPath))
            {
                var jsonFile = await IoFile.ReadAllTextAsync(liveDataPath);
                liveData = JsonConvert.DeserializeObject<IEnumerable<VariableMessageSign>>(jsonFile, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }

            var notificationService = new NotificationService(liveData, config.SubscriptionsPath, config.WebPushNotification, _config.Url);
            notificationService.Preprocess();
            await notificationService.Send();

            return Ok();
        }
    }
}
