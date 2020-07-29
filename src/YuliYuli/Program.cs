using HtmlAgilityPack;
using System;

namespace YuliYuli
{
    class Program
    {
        static void Main(string[] args)
        {
            
                DownloadMp4("BV1B5411a7Ev", 129);
             
            Console.ReadLine();
        }

        static void DownloadMp4(string bilibili, int p)
        {
            jfYu.Core.jfYuRequest.jfYuRequest jfYuRequest = new jfYu.Core.jfYuRequest.jfYuRequest("https://www.xbeibeix.com/api/bilibili/");
            //jfYuRequest.CustomHeader.Add(":authority", "www.xbeibeix.com");
            //jfYuRequest.CustomHeader.Add(":method", "POST");
            //jfYuRequest.CustomHeader.Add(":path", "/api/bilibili/");
            //jfYuRequest.CustomHeader.Add(":scheme", "https");
            jfYuRequest.Timeout = 30000;
            jfYuRequest.RequestHeader.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            jfYuRequest.RequestHeader.AcceptEncoding = "gzip, deflate, br";
            jfYuRequest.RequestHeader.AcceptLanguage = "zh-CN,zh;q=0.9,en;q=0.8";
            jfYuRequest.RequestHeader.CacheControl = "max-age=0";
            jfYuRequest.RequestHeader.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36";
            jfYuRequest.Method = jfYu.Core.jfYuRequest.jfYuRequestMethod.Post;
            jfYuRequest.Para.Add("bilibiliurl07255", $"https://www.bilibili.com/video/{bilibili}?p={p}");
            jfYuRequest.Para.Add("zengqiang", "true");
            var x = jfYuRequest.GetHtml();
            var doc = new HtmlDocument();
            doc.LoadHtml(x);
            var title = doc.DocumentNode.SelectSingleNode("//span[contains(text(), '标题')]")?.ParentNode?.NextSibling?.NextSibling?.Attributes["value"]?.Value;
            if (string.IsNullOrEmpty(title))
            {
                Console.WriteLine($"P{p}下载失败");
                return;
            }
            var url = doc.DocumentNode.SelectSingleNode("//a[contains(text(), 'MP4地址')]").Attributes["href"].Value;
            jfYu.Core.jfYuRequest.jfYuRequest download = new jfYu.Core.jfYuRequest.jfYuRequest(url);
            download.GetFile(@$"F:\考试\软件设计师中级考试辅导视频\P{p}{title}.mp4");
            Console.WriteLine($"P{p}下载完毕");
        }

    }
}
