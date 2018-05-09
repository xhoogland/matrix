using Matrix.Interfaces;
using Matrix.SpecificImplementations;
using System.Linq;

namespace Matrix.FileModels.Locations.LCSLocations
{
    public class Feature : Location
    {
        public string Type { get; set; }

        public Properties Properties { get; set; }

        public Geometry Geometry { get; set; }

        public bool HasCoordinates => Geometry.Coordinates != null && Geometry.Coordinates.Count() == 2 &&
                                   Geometry.Coordinates.Last() != 0 && Geometry.Coordinates.First() != 0;

        public float? Km => Properties.Km;

        public int? Lane => Properties.Lane;

        public Coordinates Coordinates => new GpsCoordinates
        {
            X = Geometry.Coordinates.Last(),
            Y = Geometry.Coordinates.First()

        };

        public string RoadName => Properties.Road;

        public string RoadSide => Properties.Carriagew0;

        public string Id => Properties.Uuid;

        public bool IsLaneSpecific => true;
    }
}
