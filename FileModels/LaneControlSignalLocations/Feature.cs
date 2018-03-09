using Matrix.ViewModels;
using System.Linq;

namespace Matrix.FileModels.LaneControlSignalLocations
{
    public class Feature : ILocation
    {
        private bool _hasLocation;

        public string Type { get; set; }

        public Properties Properties { get; set; }

        public Geometry Geometry { get; set; }

        public bool HasLocation => Geometry.Coordinates != null && Geometry.Coordinates.Count() == 2 &&
                                   Geometry.Coordinates.Last() != 0 && Geometry.Coordinates.First() != 0;

        public float? Km => Properties.Km;

        public int? Lane => Properties.Lane;

        public Location Location => new Location
        {
            Latitude = Geometry.Coordinates.Last(),
            Longitude = Geometry.Coordinates.First()

        };

        public string RoadName => Properties.Road;

        public string RoadSide => Properties.Carriagew0;

        public string Id => Properties.Uuid;
    }
}
