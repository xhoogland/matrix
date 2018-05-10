namespace Matrix.FileModels.LiveData.LCS
{
    public class Display
    {
        public Blank Blank { get; set; }

        public LaneClosed LaneClosed { get; set; }

        public Speedlimit Speedlimit { get; set; }

        public Msi Unknown { get; set; }

        public LaneOpen LaneOpen { get; set; }

        public RestrictionEnd RestrictionEnd { get; set; }

        public LaneClosedAhead LaneClosedAhead { get; set; }
    }
}
