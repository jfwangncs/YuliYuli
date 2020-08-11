using AutoMapper;
using jfYu.Core.Common.Utilities;
using jfYu.Core.jfYuRequest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using YuliYuli.Models;

namespace YuliYuli
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        object lockObject = new object();
        public List<VideoInfo> DownList = new List<VideoInfo>();
        private ConcurrentQueue<string> Queue = new ConcurrentQueue<string>();
        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Aid.Text = "";
            FilePath.Text = Environment.CurrentDirectory;
            this.ResizeMode = ResizeMode.CanMinimize;
            VideoListView.ItemsSource = DownList;
            //选择保存地址
            ChangeSavePath.Click += ChangeSavePath_Click;
            //下载
            DownFile.Click += DownFile_Click;

            Task.Run(async () => await DownLoadVideo());
            Task.Run(async () => await DownLoadVideo());
            Task.Run(async () => await DownLoadVideo());
        }
        private void ChangeSavePath_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FilePath.Text = dialog.SelectedPath;
            }
        }
        private async void DownFile_Click(object sender, RoutedEventArgs e)
        {
            string aid = Aid.Text.Trim();
            if (string.IsNullOrEmpty(aid))
            {
                System.Windows.Forms.MessageBox.Show("请输入视频号或者视频链接,例如：BV1fD4y1m7TD");
                DownFile.IsEnabled = true;
                return;
            }

            string Url = @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
            if (Regex.IsMatch(aid, Url))
            {
                aid = aid.Replace("https://www.bilibili.com/video/", "").Split("?")[0];
            }
            //获取视频信息
            var video = await GetVideoInfo(aid);
            if (string.IsNullOrEmpty(video.Title) || video.Sections.Count <= 0)
            {
                System.Windows.Forms.MessageBox.Show("视频不存在请检查视频号或者链接是否正确。");
                DownFile.IsEnabled = true;
                return;
            }
            video.SavePath = FilePath.Text;
            if (!DownList.Any(q => q.Title.Equals(video.Title) && q.AID.Equals(video.AID) && q.BVID.Equals(video.BVID)))
            {
                Queue.Enqueue(video.AID);
                DownList.Add(video);
                VideoListView.Items.Refresh();
            }
        }
        private async Task<VideoInfo> GetVideoInfo(string bvid)
        {
            var video = new VideoInfo();
            try
            {
                var jfYu = new jfYuRequest($"https://api.bilibili.com/x/web-interface/view?bvid={bvid}")
                {
                    Timeout = 30000
                };
                jfYu.RequestHeader.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                jfYu.RequestHeader.AcceptEncoding = "gzip, deflate, br";
                jfYu.RequestHeader.AcceptLanguage = "zh-CN,zh;q=0.9,en;q=0.8";
                jfYu.RequestHeader.CacheControl = "max-age=0";
                jfYu.RequestHeader.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36";
                var html = await jfYu.GetHtmlAsync();
                var data = Serializer.Deserialize<dynamic>(html);
                if (jfYu.StatusCode == System.Net.HttpStatusCode.OK && data.code == "0")
                {
                    //获取视频信息
                    string title = data.data.title;
                    string reg = @"\:" + @"|du\;" + @"|\/" + @"|\\" + @"|\|" + @"|\," + @"|\*" + @"|\?" + @"|\""" + @"|\<" + @"|\>";
                    Regex r = new Regex(reg);
                    video.Title = r.Replace(title, "");
                    video.Sections = Serializer.Deserialize<List<VideoPagedInfo>>(data.data.pages.ToString());
                    video.AID = data.data.aid;
                    video.BVID = bvid;
                    video.Mid = data.data.owner.mid;
                    video.Name = data.data.owner.name;
                }
            }
            catch (Exception)
            {

            }
            return video;
        }
        private async Task DownLoadVideo()
        {
            do
            {
                string Aid = "";
                if ((Queue.TryDequeue(out Aid)))
                {
                    var video = DownList.FirstOrDefault(q => q.AID.Equals(Aid));
                    if (video != null)
                    {
                        var SavePath = video.SavePath + "\\" + video.Title + "\\";
                        if (!Directory.Exists(SavePath))
                            Directory.CreateDirectory(SavePath);

                        foreach (var item in video.Sections)
                        {
                            var fileName = $"P{item.page}{item.part}";
                            //三次重试
                            for (int j = 0; j < 4; j++)
                            {
                                var urlReq = new jfYuRequest($"https://api.bilibili.com/x/player/playurl?avid={video.AID}&cid={item.cid}&ptype=json&qn=112")
                                {
                                    Encoding = Encoding.UTF8,
                                    Timeout = 30000
                                };
                                var urlReqhtml = await urlReq.GetHtmlAsync();
                                var urlReqData = Serializer.Deserialize<dynamic>(urlReqhtml);
                                string url = urlReqData.data.durl[0].url;
                                item.size = urlReqData.data.durl[0].size;
                                item.Size = ((decimal)item.size / 1024 / 1024).ToString("0.00") + "MB";
                                var dfreq = new jfYuRequest(url);
                                dfreq.RequestHeader.Referer = $"https://www.bilibili.com/video/{video.BVID}";
                                try
                                {
                                    var flag = await dfreq.GetFileAsync($"{SavePath}\\{fileName}.flv", (q, e, t) =>
                                   {
                                       Dispatcher.Invoke(() =>
                                       {
                                           lock (lockObject)
                                           {
                                               var pro = double.Parse(q.ToString("0.00"));
                                               item.Process = pro + "%";
                                               var speed = e > 1024 ? $"{e / 1024:0.00}MB/s" : $"{e:0.00}KB/s";
                                               item.Speed = speed;
                                               //TimeSpan time = TimeSpan.FromSeconds((double)t);
                                               VideoListView.Items.Refresh();
                                           };
                                       });
                                   });
                                    if (flag)
                                    {
                                        item.IsDown = true;
                                        break;
                                    }
                                    else
                                        item.IsDown = false;
                                }
                                catch (Exception)
                                {
                                    if (j >= 4)
                                    {
                                        item.Process = "0%";
                                        item.Speed = "0KB/s";
                                        Dispatcher.Invoke(() =>
                                        {
                                            VideoListView.Items.Refresh();
                                        });
                                    }
                                }
                            }
                        }
                    }
                    if (video.Sections.All(q => q.IsDown))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            DownList.Remove(video);
                            VideoListView.Items.Refresh();
                        });
                    }

                }

                await Task.Delay(1000);
            } while (true);
        }
    }
}
