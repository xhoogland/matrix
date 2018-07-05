using System.Collections.Generic;

namespace Matrix.ViewModels
{
    public class Notification
    {
        public string Title { get; set; }

        public string Icon { get; set; }

        public string Body { get; set; }

        public string Image { get; set; }

        public string Tag { get; set; }

        public NotificationData Data { get; set; }

        public IList<NotificationAction> Actions { get; set; }

        public string HmLocation { get; set; }

        public string LanesShownSign { get; set; }

        public bool DripShownSign { get; set; }

        public Notification()
        {
            Actions = new List<NotificationAction>();
        }
    }
}
