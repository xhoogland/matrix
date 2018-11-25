using System;

namespace Matrix.FileModels.Locations.BELLCS
{
    public class RssConfiguratie
    {
        public string XmlnsXsi { get; set; }

        public string XsiNoNamespaceSchemaLocation { get; set; }

        public string SchemaVersion { get; set; }

        public DateTime Tijd_Laatste_Config_Wijziging { get; set; }

        public RssBord[] Rss_Bord { get; set; }
    }
}
