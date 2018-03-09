using System;
using Matrix.ViewModels;

namespace Matrix.FileModels.VariableMessageSignLocations
{
    public class VmsUnitRecord : ILocation
    {
        public byte NumberOfVms { get; set; }

        public VmsRecordVmsRecord VmsRecord { get; set; }

        public string Id { get; set; }

        public byte Version { get; set; }

        public bool HasLocation => throw new NotImplementedException();

        public float GetKm()
        {
            throw new NotImplementedException();
        }

        public int GetLane()
        {
            throw new NotImplementedException();
        }

        public Location GetLocation()
        {
            throw new NotImplementedException();
        }

        public string GetRoadName()
        {
            throw new NotImplementedException();
        }

        public string GetRoadSide()
        {
            throw new NotImplementedException();
        }

        public string GetId()
        {
            throw new NotImplementedException();
        }
    }
}
