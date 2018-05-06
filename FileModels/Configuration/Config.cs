namespace Matrix.FileModels.Configuration
{
    public class Config
    {
        public DownloadLocation DownloadLocation { get; set; }

        public FileLocation FileLocation { get; set; }

        public string StartPath { get; set; }

        public string DataPath { get; set; }
    }
}
