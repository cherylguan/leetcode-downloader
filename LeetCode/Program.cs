using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeetCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            DownloadLeetCode();
        }

        static void DownloadLeetCode()
        {
            var downloaders = new List<LeetCodeDownloader>();
            var approximateMaxCount = 1400;
            var batchSize = 100;
            var tasks = new List<Task>();
            var processed = 0;
            while(processed < approximateMaxCount)
            {
                var downloader = new LeetCodeDownloader();
                downloaders.Add(downloader);
                var temp = processed;
                tasks.Add(Task.Run(() => downloader.DownloadProblems(temp, temp + batchSize)));
                processed += batchSize;
            }

            Task.WhenAll(tasks).Wait();
        }

        static void Web()
        {
            var html = Get("https://leetcode.com/problemset/all/");

            // From Web
            var url = "http://html-agility-pack.net/";
            var web = new HtmlWeb();
            var doc = web.Load(url);
        }

        static string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
