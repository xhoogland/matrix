using Matrix.Interfaces;
using System;

namespace Matrix.FileModels.LaneControlSignalLiveData
{
    public class Event : LiveData
    {
        public DateTime TsEvent { get; set; }

        public DateTime TsState { get; set; }

        public SignId SignId { get; set; }

        public LaneLocation LaneLocation { get; set; }

        public Display Display { get; set; }

        public string Id => SignId.Uuid;

        public string Sign => throw new NotImplementedException();
    }
}
