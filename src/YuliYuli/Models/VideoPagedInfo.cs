using System;
using System.Collections.Generic;
using System.Text;

namespace YuliYuli.Models
{
    /// <summary>
    /// 接口返回分P数据
    /// </summary>
    public class VideoPagedInfo
    {
        /// <summary>
        /// 视频ID
        /// </summary>
        public int cid { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int page { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string part { get; set; }
    }
}
