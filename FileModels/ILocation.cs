using Matrix.ViewModels;

namespace Matrix.FileModels
{
    public interface ILocation
    {
        Location Location { get; }

        string RoadName { get; }

        string RoadSide { get; }

        float? Km { get; }

        string Id { get; }

        int? Lane { get; }

        bool HasLocation { get; }
    }
}
