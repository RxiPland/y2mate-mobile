using System;
using System.Collections.Generic;
using System.Text;

namespace y2mate.Config
{
    internal class GlobalConfig
    {

        // App
        public static readonly string AppName = "y2mate";

        public static readonly string SettingsFileName = "Settings.json";
        public static readonly string downloadFolder = "/storage/emulated/0/Download";


        // Networking
        public static readonly string ApiDomain = "y2mate.com";
        public static readonly string ApiUrl = "https://" + "www.y2mate.com";
        public static readonly string ApiSearchUrl = ApiUrl + "/mates/analyzeV2/ajax";
        public static readonly string ApiRequestVideoUrl = ApiUrl + "/mates/convertV2/index";

        public static readonly int RequestTimeout = 10;
        public static readonly string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
        public static readonly bool IgnoreSSLCertificates = true;

        // Y2mate
        public static readonly string[] RequiredSearchResponseKeys = { "status", "mess", "title", "t", "a", "vid", "links" };

        // UI
        public static readonly int RefreshIconDurationMs = 2000;
    }
}
