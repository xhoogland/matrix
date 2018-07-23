using System.Collections.Generic;

namespace Matrix.ViewModels
{
    public class PushUser
    {
        public PushSubscription PushSubscription { get; set; }

        public IList<PushRoadWay> RoadWays { get; set; }

        public PushUser()
        {
            RoadWays = new List<PushRoadWay>();
        }
    }
}
