using Matrix.ViewModels;
using System;

namespace Matrix.Parsers
{
    public interface ILocationParser
    {
        Location GetLocation();

        string GetRoadName();

        string GetRoadSide();

        float GetKm();

        Guid GetUuid();

        int GetLane();
    }
}
