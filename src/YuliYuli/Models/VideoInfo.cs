using System;
using System.Collections.Generic;
using System.Text;

namespace YuliYuli.Models
{
    public class VideoInfo
    {
        /// <summary>
        /// BVID
        /// </summary>
        public string BVID { get; set; }
        /// <summary>
        /// AID
        /// </summary>
        public string AID { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 作者编号
        /// </summary>
        public string Mid { get; set; }
        
        /// <summary>
        /// 作者名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 视频分P
        /// </summary>
        public List<VideoPagedInfo> Sections { get; set; } = new List<VideoPagedInfo>();
    }
}
