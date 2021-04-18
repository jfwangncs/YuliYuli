using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace YuliYuli.Models
{
    /// <summary>
    /// 接口返回分P数据
    /// </summary>
    public class VideoPagedInfo : INotifyPropertyChanged
    {

        /// <summary>
        /// 存储位置
        /// </summary>
        public string SavePath { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 视频ID
        /// </summary>
        public string AID { get; set; }

        /// <summary>
        /// 视频ID
        /// </summary>
        public string BVID { get; set; }
        /// <summary>
        /// 视频ID
        /// </summary>
        public int cid { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        public long size { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int page { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string part { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public string Size { get; set; } = "0MB";

        /// <summary>
        /// 下载进度
        /// </summary>
        public string Process { get; set; } = "0%";

        /// <summary>
        /// 下载速度
        /// </summary>
        public string Speed { get; set; } = "0KB/s";

        public Color color { get; set; } = Color.WhiteSmoke;

        /// <summary>
        /// 是否完成
        /// </summary>
        public string state { get; set; } = "正常";

        /// <summary>
        /// 作者编号
        /// </summary>
        public string Mid { get; set; }


        /// <summary>
        /// 作者名
        /// </summary>
        public string Name { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
