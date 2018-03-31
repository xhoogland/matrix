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

        bool HasCoordinates { get; }

        bool IsLaneSpecific { get; }
    }
}
