using System;

namespace Matrix.FileModels.LiveData.BELLCS
{
    public class TechnischeToestand
    {
        public DateTime Laatst_Gewijzigd { get; set; }

        public string InDienst { get; set; }

        public string Defect { get; set; }
    }
}
