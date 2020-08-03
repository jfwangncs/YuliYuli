using jfYu.Core.Common.Utilities;
using jfYu.Core.jfYuRequest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using YuliYuli.Models;

namespace YuliYuli
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Aid.Text = "";
            FilePath.Text = Environment.CurrentDirectory;
            ProcessBar.Value = 0;
            ProcessValue.Text = "";
            this.ResizeMode = ResizeMode.CanMinimize;
            //选择保存地址
            FilePath.MouseDoubleClick += FilePath_MouseDoubleClick;
            //下载
            DownFile.Click += DownFile_Click;

        }

        private async void DownFile_Click(object sender, RoutedEventArgs e)
        {
            ProcessBar.Value = 0;
            ProcessValue.Text = "";
            Result.Text = "";
            string aid = Aid.Text.Trim();
            if (string.IsNullOrEmpty(aid))
            {
                System.Windows.Forms.MessageBox.Show("请输入视频号,例如：BV1fD4y1m7TD");
                return;
            }
            //获取视频信息
            var video = await GetVideoInfo(aid);
            if (string.IsNullOrEmpty(video.Title))
            {
                System.Windows.Forms.MessageBox.Show("视频不存在请检查视频号或者链接是否正确");
                return;
            }
            var run = new Run($"{video.Title}下载中...\r\n")
            {
                Foreground = Brushes.Green
            };
            Result.Inlines.Add(run);
            //下载视频
            await DownLoadVideo(aid, video);

            run = new Run($"{video.Title}全部下载完成.\r\n")
            {
                Foreground = Brushes.Green
            };
            Result.Inlines.Add(run);
        }

        private void FilePath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FilePath.Text = dialog.SelectedPath;
            }
        }

        private async Task<VideoInfo> GetVideoInfo(string aid)
        {
            var video = new VideoInfo();
            try
            {
                var jfYu = new jfYuRequest($"https://www.bilibili.com/video/{aid}/")
                {
                    Timeout = 30000
                };
                jfYu.RequestHeader.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                jfYu.RequestHeader.AcceptEncoding = "gzip, deflate, br";
                jfYu.RequestHeader.AcceptLanguage = "zh-CN,zh;q=0.9,en;q=0.8";
                jfYu.RequestHeader.CacheControl = "max-age=0";
                jfYu.RequestHeader.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36";
                var html = await jfYu.GetHtmlAsync();
                if (jfYu.StatusCode == System.Net.HttpStatusCode.OK && !html.Contains("视频不见了"))
                {
                    //获取视频标题
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);
                    var title = doc.DocumentNode.SelectSingleNode("//h1[@class='video-title']").Attributes["title"].Value;
                    string reg = @"\:" + @"|du\;" + @"|\/" + @"|\\" + @"|\|" + @"|\," + @"|\*" + @"|\?" + @"|\""" + @"|\<" + @"|\>";
                    Regex r = new Regex(reg);
                    video.Title = r.Replace(title, "");
                    //获取视频分页
                    var videoPageReq = new jfYuRequest($"https://api.bilibili.com/x/player/pagelist?bvid={aid}&jsonp=jsonp")
                    {
                        Timeout = 30000
                    };
                    var videoPageList = Serializer.Deserialize<VideoPagedList>(await videoPageReq.GetHtmlAsync());
                    if (videoPageList.code == 0)
                        video.Sections = videoPageList.data;
                }
            }
            catch (Exception)
            {

            }

            return video;
        }

        private async Task DownLoadVideo(string aid, VideoInfo video)
        {

            var SavePath = FilePath.Text + "\\" + video.Title + "\\";
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);
            foreach (var item in video.Sections)
            {
                var pTitle = item.part;
                //三次重试
                for (int j = 0; j < 4; j++)
                {
                    var dfreq = await DownLoadFile($"https://www.bilibili.com/video/{aid}?p={item.page}");
                    var fileName = $"P{item.page}{pTitle}";
                    var flag = await dfreq.GetFileAsync($"{SavePath}\\{fileName}.mp4", (q, e, t) =>
                    {
                        var pro = double.Parse(q.ToString("0.00"));
                        var left = pro / 100 * this.Width;
                        if (left < 20)
                            left = 20;
                        if (left > this.Width - 80)
                            left = this.Width - 80;
                        Thickness margin = new Thickness(left - 10, 63, 10, 0);
                        ProcessPanel.Margin = margin;
                        ProcessBar.Value = pro;
                        ProcessValue.Text = $"{pro}%";
                    });
                    if (flag)
                    {
                        Thickness margin = new Thickness(10, 63, 10, 0);
                        ProcessPanel.Margin = margin;
                        ProcessBar.Value = 0;
                        ProcessValue.Text = "";
                        var run = new Run($"{fileName}下载完成\r\n")
                        {
                            Foreground = Brushes.Green
                        };
                        Result.Inlines.Add(run);
                        break;
                    }
                    else
                    {
                        var run = new Run($"{fileName}下载失败，重试第{j + 1}次\r\n")
                        {
                            Foreground = Brushes.Red
                        };
                        Result.Inlines.Add(run);
                    }
                }
            }
        }

        private async Task<jfYuRequest> DownLoadFile(string burl)
        {

            var xbeibei = new jfYuRequest("https://www.xbeibeix.com/api/bilibili/")
            {
                Encoding = Encoding.UTF8,
                Timeout = 30000
            };
            xbeibei.RequestHeader.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            xbeibei.RequestHeader.AcceptEncoding = "gzip, deflate, br";
            xbeibei.RequestHeader.AcceptLanguage = "zh-CN,zh;q=0.9,en;q=0.8";
            xbeibei.RequestHeader.CacheControl = "max-age=0";
            xbeibei.RequestHeader.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36";
            xbeibei.Method = jfYuRequestMethod.Get;
            var xbeibeihtml = await xbeibei.GetHtmlAsync();
            var xbeibeidoc = new HtmlAgilityPack.HtmlDocument();
            xbeibeidoc.LoadHtml(xbeibeihtml);

            var burlname = xbeibeidoc.DocumentNode.SelectSingleNode("//input[contains(@placeholder, '输入BiliBili视频链接地址')]").Attributes["name"].Value;

            var jfYuRequest = new jfYuRequest("https://www.xbeibeix.com/api/bilibili/")
            {
                Encoding = Encoding.UTF8,
                //jfYuRequest.CustomHeader.Add(":authority", "www.xbeibeix.com");
                //jfYuRequest.CustomHeader.Add(":method", "POST");
                //jfYuRequest.CustomHeader.Add(":path", "/api/bilibili/");
                //jfYuRequest.CustomHeader.Add(":scheme", "https");
                Timeout = 30000
            };
            jfYuRequest.RequestHeader.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            jfYuRequest.RequestHeader.AcceptEncoding = "gzip, deflate, br";
            jfYuRequest.RequestHeader.AcceptLanguage = "zh-CN,zh;q=0.9,en;q=0.8";
            jfYuRequest.RequestHeader.CacheControl = "max-age=0";
            jfYuRequest.RequestHeader.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36";
            jfYuRequest.Method = jfYuRequestMethod.Post;
            jfYuRequest.Para.Add(burlname, burl);
            jfYuRequest.Para.Add("zengqiang", "true");
            var html = await jfYuRequest.GetHtmlAsync();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var title = doc.DocumentNode.SelectSingleNode("//span[contains(text(), '标题')]")?.ParentNode?.NextSibling?.NextSibling?.Attributes["value"]?.Value;
            if (string.IsNullOrEmpty(title))
            {
                return null;
            }
            var url = doc.DocumentNode.SelectSingleNode("//a[contains(text(), 'MP4地址')]").Attributes["href"].Value;
            return new jfYuRequest(url);

        }
    }

}
