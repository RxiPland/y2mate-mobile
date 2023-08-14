using System;
using System.Collections.Generic;
using System.Text;

namespace y2mate.Models
{
    public class FoundVideoModel
    {

        // {"360p": "oFGWtSzxsagasL176v...", }
        public Dictionary<string, string> AvailableMP3 = new();
        public Dictionary<string, string> AvailableMP4 = new();

        public string VideoTitle { get; set; }
        public int VideoDurationTimeSec { get; set; }
        public string VideoUrl { get; set; }
        public string VideoId { get; set; }
        public string YtChannel { get; set; }

    }
}
