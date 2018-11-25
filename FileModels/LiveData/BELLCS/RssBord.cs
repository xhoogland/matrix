namespace Matrix.FileModels.LiveData.BELLCS
{
    public class RssBord : Interfaces.LiveData
    {
        public string Unieke_Id { get; set; }

        public string AbbameldaNaam { get; set; }

        public TechnischeToestand Technische_Toestand { get; set; }

        public AangevraagdeBoodschap Aangevraagde_Boodschap { get; set; }

        public BevestigdeBoodschap Bevestigde_Boodschap { get; set; }

        public string Id => Unieke_Id;

        public string Sign
        {
            get
            {
                var sign = Bevestigde_Boodschap.Verkeersteken_Status.ToLower();
                if (sign == "doven")
                    return "blank";
                if (string.IsNullOrWhiteSpace(sign) || sign == "?")
                    return "unknown";

                return sign;
            }
        }

        public bool IsValid => Technische_Toestand.InDienst == 1.ToString() && Technische_Toestand.Defect == 0.ToString();
    }
}
