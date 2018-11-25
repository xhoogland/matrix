namespace Matrix.Interfaces
{
    public interface Location
    {
        Coordinates Coordinates { get; }

        string RoadName { get; }

        string RoadSide { get; }

        float? Km { get; }

        string Id { get; }

        int? Lane { get; }

        bool IsValid { get; }

        bool IsLaneSpecific { get; }

        string Country { get; }
    }
}
