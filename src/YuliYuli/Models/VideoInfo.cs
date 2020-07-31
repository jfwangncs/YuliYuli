using System;
using System.Collections.Generic;
using System.Text;

namespace YuliYuli.Models
{
    public class VideoInfo
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 视频分P
        /// </summary>
        public List<VideoPagedInfo> Sections { get; set; }
    }
}
