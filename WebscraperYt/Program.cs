using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace WebscraperYt
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //geef zoekterm in voor in de url
            Console.Write("Geef een zoekterm in: ");
            string zoekterm = Console.ReadLine();
            //Verplaats spaties naar + zodat de url klopt
            zoekterm = zoekterm.Replace(" ", "+");
            Console.WriteLine();

            //start driver en browse naar pagina
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.youtube.com/results?search_query=" + zoekterm + "&sp=CAI%253D");

            List<data> _data = new List<data>();

            string json = "";
            var csv = new StringBuilder();

            var cookiebutton = driver.FindElement(By.XPath("/html/body/ytd-app/ytd-consent-bump-v2-lightbox/tp-yt-paper-dialog/div[4]/div[2]/div[6]/div[1]/ytd-button-renderer[1]/yt-button-shape/button"));
            cookiebutton.Click();


            try
            {
                //Zoek de eerste video
                driver.FindElement(By.CssSelector("ytd-video-renderer:nth-child(1)"));

                try
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        Console.WriteLine("_________________________________________________");
                        Console.WriteLine("Video " + i);

                        var videoclass = "ytd-video-renderer:nth-child(" + i + ")";

                        var video = driver.FindElement(By.CssSelector(videoclass));
                        var titel = video.FindElement(By.CssSelector("#video-title")).Text;
                        var weergaven = video.FindElement(By.CssSelector("#metadata-line > span:nth-child(3)")).Text;
                        var uploader = video.FindElement(By.Id("channel-info")).Text;
                        var link = video.FindElement(By.CssSelector("#video-title")).GetAttribute("href");

                        //Print video details
                        Console.WriteLine("Link: " + link);
                        Console.WriteLine("Titel: " + titel);
                        Console.WriteLine("Kanaal: " + uploader);
                        Console.WriteLine("Aantal weergaven: " + weergaven.Replace("weergaven", ""));
                        Console.WriteLine("_________________________________________________");
                        Console.WriteLine();

                        _data.Add(new data()
                        {
                            Titel = titel,
                            Uploader = uploader,
                            Weergaven = weergaven,
                            Link = link
                        }); ;
                        json = JsonSerializer.Serialize(_data);

                        var newLine = string.Format("{0},{1},{2},{3}", titel, uploader, weergaven, link);
                        csv.AppendLine(newLine);
                    }
                }
                catch
                {
                    string error = "Er zijn niet genoeg video's met deze zoekterm om aan 5 te komen.";
                    Console.WriteLine(error);
                    _data.Add(new data()
                    {
                        Error = error,
                    }); ;
                    json = JsonSerializer.Serialize(_data);

                    var newLine = string.Format("{0}", error);
                    csv.AppendLine(newLine);

                }
            }
            catch
            {
                string error = "Er zijn geen video's met deze zoekterm.";
                Console.WriteLine(error);
                _data.Add(new data()
                {
                    Error = error,
                }); ;
                json = JsonSerializer.Serialize(_data);

                var newLine = string.Format("{0}", error);
                csv.AppendLine(newLine);

            }

            Directory.CreateDirectory("D:\\DevOps");

            File.WriteAllText(@"D:\DevOps\Yt.csv", csv.ToString());
            File.WriteAllText(@"D:\DevOps\Yt.json", json);
            Console.ReadLine();

            Console.ReadKey();

        }
    }
}