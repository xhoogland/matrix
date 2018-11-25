using Matrix.Interfaces;
using Matrix.SpecificImplementations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Matrix.FileModels.Locations.BELLCS
{
    public class RssBord : Location
    {
        private MatchCollection AbbameldaNaamMatches => new Regex(@"([A-Z]){1}([0-9]){1,3}(P|N|M)([0-9]){1,3}\.?([0-9]){0,2}").Matches(AbbameldaNaam);

        public string Unieke_Id { get; set; }

        public string Naam { get; set; }

        public string AbbameldaNaam { get; set; }

        public string Ident_8 { get; set; }

        public string Bord_Type { get; set; }

        public string Lengtegraad_EPSG_4326 { get; set; }

        public string Breedtegraad_EPSG_4326 { get; set; }

        public string Voor_Of_Achterbord { get; set; }

        public string Rijstrook { get; set; }

        public Coordinates Coordinates => new GpsCoordinates
        {
            X = double.Parse(Breedtegraad_EPSG_4326),
            Y = double.Parse(Lengtegraad_EPSG_4326)
        };

        public string RoadName => AbbameldaNaamMatches.Any() ? AbbameldaNaamMatches.First().ToString() : AbbameldaNaam;

        public string RoadSide => null;

        public float? Km => null;

        public string Id => Unieke_Id;

        public int? Lane => int.Parse(new string(Rijstrook.Where(char.IsDigit).ToArray()));

        public bool IsValid => Breedtegraad_EPSG_4326 != null && Lengtegraad_EPSG_4326 != null &&
            double.Parse(Breedtegraad_EPSG_4326) != 0 && double.Parse(Lengtegraad_EPSG_4326) != 0 &&
            Voor_Of_Achterbord == "VOOR";

        public bool IsLaneSpecific => true;

        public string Country => "BE";
    }
}
