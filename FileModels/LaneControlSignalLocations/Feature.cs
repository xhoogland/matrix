using Matrix.ViewModels;
using System.Linq;

namespace Matrix.FileModels.LaneControlSignalLocations
{
    public class Feature : ILocation
    {
        public string Type { get; set; }

        public Properties Properties { get; set; }

        public Geometry Geometry { get; set; }

        public float GetKm()
        {
            return Properties.Km;
        }

        public int GetLane()
        {
            return Properties.Lane;
        }

        public Location GetLocation()
        {
            var coordinates = Geometry.Coordinates;
            return new Location
            {
                Latitude = coordinates.Last(),
                Longitude = coordinates.First()

            };
        }

        public string GetRoadName()
        {
            return Properties.Road;
        }

        public string GetRoadSide()
        {
            return Properties.Carriagew0;
        }

        public string GetUuid()
        {
            return Properties.Uuid;
        }
    }
}
