using Matrix.ViewModels;

namespace Matrix.FileModels
{
    public interface ILocation
    {
        Location GetLocation();

        string GetRoadName();

        string GetRoadSide();

        float GetKm();

        string GetUuid();

        int GetLane();
    }
}
