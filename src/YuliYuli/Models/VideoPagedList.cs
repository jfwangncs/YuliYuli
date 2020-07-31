using System;
using System.Collections.Generic;
using System.Text;

namespace YuliYuli.Models
{
    /// <summary>
    /// 接口返回分P列表
    /// </summary>
    public class VideoPagedList
    {
        public int code { get; set; }
        public List<VideoPagedInfo> data { get; set; }
    }
}
