using System;
using Matrix.ViewModels;

namespace Matrix.FileModels.VariableMessageSignLocations
{
    public class VmsUnitRecord : ILocation
    {
        public byte NumberOfVms { get; set; }

        public VmsRecordVmsRecord VmsRecord { get; set; }

        public byte Version { get; set; }

        public bool HasLocation => VmsRecord.VmsRecord.VmsLocation != null &&
                                   VmsRecord.VmsRecord.VmsLocation.LocationForDisplay != null &&
                                   VmsRecord.VmsRecord.VmsLocation.LocationForDisplay.Latitude != 0 &&
                                   VmsRecord.VmsRecord.VmsLocation.LocationForDisplay.Longitude != 0;

        public Location Location => new Location
        {
            Latitude = VmsRecord.VmsRecord.VmsLocation.LocationForDisplay.Latitude,
            Longitude = VmsRecord.VmsRecord.VmsLocation.LocationForDisplay.Longitude
        };

        public string RoadName => VmsRecord.VmsRecord.VmsDescription.Values.Value.Value.Split(' ')[1];

        public string RoadSide => null;

        public float? Km => null;

        public string Id { get; set; }

        public int? Lane => null;
    }
}
