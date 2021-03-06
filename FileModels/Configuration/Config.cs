﻿namespace Matrix.FileModels.Configuration
{
    public class Config
    {
        public DownloadUrl DownloadUrl { get; set; }

        public SaveFileName SaveFileName { get; set; }

        public string StartPath { get; set; }

        public string DataPath { get; set; }

        public string LocationsPath { get; set; }

        public string SubscriptionsPath { get; set; }

        public WebPushNotification WebPushNotification { get; set; }

        public string Url { get; set; }
    }
}
