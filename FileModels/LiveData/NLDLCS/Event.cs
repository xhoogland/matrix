using System;

namespace Matrix.FileModels.LiveData.NLDLCS
{
    public class Event : Interfaces.LiveData
    {
        public DateTime TsEvent { get; set; }

        public DateTime TsState { get; set; }

        public SignId SignId { get; set; }

        public LaneLocation LaneLocation { get; set; }

        public Display Display { get; set; }

        public string Id => SignId.Uuid;

        public string Sign
        {
            get
            {
                if (Display == null)
                    return null;

                if (Display.Blank != null)
                    return "blank";
                else if (Display.LaneClosed != null)
                    return "lane_closed";
                else if(Display.Speedlimit != null)
                    return string.Format("speed_{0}", Display.Speedlimit.Text);
                else if(Display.LaneOpen != null)
                    return "lane_open";
                else if (Display.RestrictionEnd != null)
                    return "restriction_end";
                else if(Display.LaneClosedAhead != null)
                    return string.Format("lane_closed_ahead_merge_{0}", Display.LaneClosedAhead.MergeLeft ? "left" : "right");

                return "unknown";
            }
        }

        public bool? HasBinary => null;
    }
}
