

using jfYu.Core.jfYuRequest;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

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
            Result.Text = "";


            FilePath.MouseDoubleClick += FilePath_MouseDoubleClick;
            //下载
            DownFile.Click += DownFile_Click;

        }

        private async void DownFile_Click(object sender, RoutedEventArgs e)
        {
            string aid = Aid.Text.Trim();
            if (string.IsNullOrEmpty(aid))
            {
                System.Windows.Forms.MessageBox.Show("请输入视频号或者视频链接");
                return;
            }

            var video = await GetVideoInfo(aid);

            if (string.IsNullOrEmpty(video.Title))
            {
                System.Windows.Forms.MessageBox.Show("视频不存在请检查视频号或者链接是否正确");
                return;
            }
        }

        private void FilePath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath.Text = dialog.SelectedPath;
                }
            }
        }

        private async Task<VideoInfo> GetVideoInfo(string aid)
        {
            var jfYu1 = new jfYuHttpClient("http://upos-sz-mirrorhw.bilivideo.com/upgcxcode/77/89/204618977/204618977-1-208.mp4?e=ig8euxZM2rNcNbhVhbdVhwdlhzdghwdVhoNvNC8BqJIzNbfq9rVEuxTEnE8L5F6VnEsSTx0vkX8fqJeYTj_lta53NCM=&uipk=5&nbs=1&deadline=1596125090&gen=playurl&os=hwbv&oi=837317101&trid=d444453003794334805e88f14b24c9cbT&platform=html5&upsig=599fab10853000f0a2c6e314276ef9c7&uparams=e,uipk,nbs,deadline,gen,os,oi,trid,platform&mid=580104086&orderid=0,1&logo=80000000");


            await jfYu1.GetFileAsync("d:/1.mp4", (q, e, t) =>
            {
                var pro = double.Parse(q.ToString("0.00"));
                var left = pro / 100 * this.Width;
                if (left < 10)
                    left = 10;
                if (left > this.Width - 120)
                    left = this.Width - 120;
                Thickness margin = new Thickness(left, 63, 10, 0);
                ProcessPanel.Margin = margin;
                ProcessBar.Value = pro;
                ProcessValue.Text = pro.ToString() + "%," + e.ToString("0.0") + "KB/s";
            });
           

            var video = new VideoInfo();
            return video;

            try
            {
                var jfYu = new jfYuRequest(aid.Contains("bilibili.com") ? aid : $"https://www.bilibili.com/video/{aid}/")
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
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);
                    var title = doc.DocumentNode.SelectSingleNode("//h1[@class='video-title']").Attributes["title"].Value;
                    var sections = doc.DocumentNode.SelectSingleNode("//span[@class='cur-page']")?.InnerText.Split("/")[1];
                    video.Title = title;
                    video.Sections = int.Parse(string.IsNullOrEmpty(sections) ? "1" : sections);
                }
            }
            catch (Exception ex)
            {

            }

            return video;
        }


    }
    public class VideoInfo
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 视频选集
        /// </summary>
        public int Sections { get; set; }
    }
}
