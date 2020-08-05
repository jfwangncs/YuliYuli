using System;
using System.Collections.Generic;
using System.Text;

namespace YuliYuli.Models
{
    public class VideoListView
    {
        public string Title { get; set; }
        public List<VideoView> Sections { get; set; } = new List<VideoView>();
    }

    public class VideoView
    {
        public string cid { get; set; }

        public string part { get; set; }

        public string Size { get; set; } = "0MB";

        public string Process { get; set; } = "0%";

        public string Speed { get; set; } = "0KB/s";

        public bool IsDown { get; set; } = false;

    }
}
