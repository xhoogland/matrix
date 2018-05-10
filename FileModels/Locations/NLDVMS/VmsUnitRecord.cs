using Matrix.Interfaces;
using Matrix.SpecificImplementations;

namespace Matrix.FileModels.Locations.NLDVMS
{
    public class VmsUnitRecord : Location
    {
        public byte NumberOfVms { get; set; }

        public VmsRecordVmsRecord VmsRecord { get; set; }

        public byte Version { get; set; }

        public bool HasCoordinates => VmsRecord.VmsRecord.VmsLocation != null &&
                                   VmsRecord.VmsRecord.VmsLocation.LocationForDisplay != null &&
                                   VmsRecord.VmsRecord.VmsLocation.LocationForDisplay.Latitude != 0 &&
                                   VmsRecord.VmsRecord.VmsLocation.LocationForDisplay.Longitude != 0;

        public Coordinates Coordinates => new GpsCoordinates
        {
            X = VmsRecord.VmsRecord.VmsLocation.LocationForDisplay.Latitude,
            Y = VmsRecord.VmsRecord.VmsLocation.LocationForDisplay.Longitude
        };

        public string RoadName => VmsRecord.VmsRecord.VmsDescription.Values.Value.Text.Split(' ')[1];

        public string RoadSide => null;

        public float? Km => null;

        public string Id { get; set; }

        public int? Lane => null;

        public bool IsLaneSpecific => false;
    }
}
