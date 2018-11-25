using System;

namespace Matrix.FileModels.LiveData.BELLCS
{
    public class RssVerkeersData
    {
        public string XmlnsXsi { get; set; }

        public string XsiNoNamespaceSchemaLocation { get; set; }

        public string SchemaVersion { get; set; }

        public DateTime Tijd_Publicatie { get; set; }

        public DateTime Tijd_Laatste_Config_Wijziging { get; set; }

        public DateTime Tijd_Laatste_Boodschappen_Wijziging { get; set; }

        public RssBord[] Rss_Bord { get; set; }
    }
}
