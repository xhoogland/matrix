using System;

namespace Matrix.FileModels.LiveData.BELLCS
{
    public class Boodschap
    {
        public DateTime Laatst_Gewijzigd { get; set; }

        public string Verkeersteken_Status { get; set; }

        public string Onderbord_Status { get; set; }

        public string Pijl_Status { get; set; }

        public string Knipperlicht_Status { get; set; }
    }
}
