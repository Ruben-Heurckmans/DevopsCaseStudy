using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace WebScraper
{
    class Scraper
    {
        public static void Main(string[] args)
        {
            int keuzeScraper;
            string zoekterm;
            try{
                Console.Write("Welke scraper wilt u gebruiken (1 = Youtube, 2 = IctJob, 3 = Twitch): ");
                keuzeScraper = Convert.ToInt32(Console.ReadLine());
            }
            catch { keuzeScraper = 4;}
            while (keuzeScraper != 0)
            {
                if (0 > keuzeScraper || keuzeScraper > 3){
                    try{
                        Console.Write("Welke scraper wilt u gebruiken (1 = Youtube, 2 = IctJob, 3 = Twitch): ");
                        keuzeScraper = Convert.ToInt32(Console.ReadLine());
                    }
                    catch { keuzeScraper = 4;}
                }
                else{
                    Console.Write("Geef een zoekterm in: ");
                    zoekterm = Console.ReadLine();

                    GetUrl(keuzeScraper, zoekterm);
                    keuzeScraper = 4;
                }
            }
        }

        static string GetUrl(int keuzeScraper, string zoekterm)
        {
            string url = "";
            switch (keuzeScraper)
            {
                case 1:
                    zoekterm = zoekterm.Replace(" ", "+");
                    url = "https://www.youtube.com/results?search_query=" + zoekterm + "&sp=CAI%253D";
                    YtScraper(StartScraper(url));
                    break;
                case 2:
                    zoekterm = zoekterm.Replace(" ", "+");
                    url = "https://www.ictjob.be/nl/it-vacatures-zoeken?keywords=" + zoekterm;
                    IctJobScraper(StartScraper(url));
                    break;
                case 3:
                    zoekterm = zoekterm.Replace(" ", "%20");
                    url = "https://www.twitch.tv/directory/game/" + zoekterm + "?sort=VIEWER_COUNT";
                    TwitchScraper(StartScraper(url));
                    break;
            }
            return url; 
        }

        static IWebDriver StartScraper(string url)
        {
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);
            return driver;
        }

        static void WriteFiles(StringBuilder csv, string json, string scraper)
        {
            Directory.CreateDirectory("D:\\DevOps");
            File.WriteAllText(@"D:\DevOps\"+scraper+".csv", csv.ToString());
            File.WriteAllText(@"D:\DevOps\" + scraper + ".json", json);
            Console.ReadLine();
        }

        static void YtScraper(IWebDriver driver)
        {
            List<YoutubeData> _data = new List<YoutubeData>();

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
                        var video = driver.FindElement(By.CssSelector("ytd-video-renderer:nth-child(" + i + ")"));
                        string titel = video.FindElement(By.CssSelector("#video-title")).Text;
                        string weergaven = video.FindElement(By.CssSelector("#metadata-line > span:nth-child(3)")).Text;
                        string uploader = video.FindElement(By.Id("channel-info")).Text;
                        string link = video.FindElement(By.CssSelector("#video-title")).GetAttribute("href");

                        //Print video details
                        Console.WriteLine("_________________________________________________");
                        Console.WriteLine("Video " + i);
                        Console.WriteLine("Link: " + link);
                        Console.WriteLine("Titel: " + titel);
                        Console.WriteLine("Kanaal: " + uploader);
                        Console.WriteLine("Aantal weergaven: " + weergaven.Replace("weergaven", ""));
                        Console.WriteLine("_________________________________________________");
                        Console.WriteLine();

                        _data.Add(new YoutubeData()
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

                    _data.Add(new YoutubeData()
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

                _data.Add(new YoutubeData()
                {
                    Error = error,
                }); ;
                json = JsonSerializer.Serialize(_data);

                var newLine = string.Format("{0}", error);
                csv.AppendLine(newLine);
            }

            WriteFiles(csv, json, "Youtube");
        }
        static void IctJobScraper(IWebDriver driver)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);

            var sortbutton = driver.FindElement(By.CssSelector("#sort-by-date"));
            sortbutton.Click();

            List<IctjobData> _data = new List<IctjobData>();

            string json = "";
            var csv = new StringBuilder();
            try
            {
                //Vraag div op waar alle jobs instaan
                var vacatures = driver.FindElement(By.CssSelector("#search-result-body"));
                try
                {
                    for (int i = 1; i <= 6; i++)
                    {
                        if (i != 4)
                        {
                            var vacature = vacatures.FindElement(By.CssSelector("li:nth-child(" + i + ")"));

                            var titel = vacature.FindElement(By.CssSelector(".job-title")).Text;
                            var bedrijf = vacature.FindElement(By.CssSelector(".job-company")).Text;
                            var locatie = vacature.FindElement(By.CssSelector(".job-location")).Text;
                            var keywords = "";
                            try {
                                keywords = vacature.FindElement(By.CssSelector(".job-keywords")).Text; }
                            catch { keywords = "Geen keywords"; }
                            var link = vacature.FindElement(By.CssSelector(".job-title")).GetAttribute("href");

                            //Print Job details
                            Console.WriteLine("_________________________________________________");
                            if (i <= 3){ 
                                Console.WriteLine("Job " + i); }
                            else { 
                                Console.WriteLine("Job " + (i - 1)); }
                            Console.WriteLine("Titel: " + titel);
                            Console.WriteLine("Bedrijf: " + bedrijf);
                            Console.WriteLine("Locatie: " + locatie);
                            Console.WriteLine("Keywords: " + keywords);
                            Console.WriteLine("Link: " + link);
                            Console.WriteLine("_________________________________________________");
                            Console.WriteLine();

                            _data.Add(new IctjobData()
                            {
                                Titel = titel,
                                Bedrijf = bedrijf,
                                Locatie = locatie,
                                Keywords = keywords,
                                Link = link
                            }); ;
                            json = JsonSerializer.Serialize(_data);

                            var newLine = string.Format("{0},{1},{2},{3},{4}", titel, bedrijf, locatie, keywords, link);
                            csv.AppendLine(newLine);
                        }
                    }
                }
                catch
                {
                    string error = "Er zijn niet genoeg jobs met deze keyword(s) om aan 5 te komen.";
                    Console.WriteLine(error);

                    _data.Add(new IctjobData()
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
                string error = "Er zijn geen jobs met deze zoekterm.";
                Console.WriteLine(error);

                _data.Add(new IctjobData()
                {
                    Error = error,
                }); ;
                json = JsonSerializer.Serialize(_data);

                var newLine = string.Format("{0}", error);
                csv.AppendLine(newLine);
            }
            WriteFiles(csv, json, "IctJob");
        }
        static void TwitchScraper(IWebDriver driver)
        {
            List<TwitchData> _data = new List<TwitchData>();

            string json = "";
            var csv = new StringBuilder();

            try
            {
                //Vraag div op waar alle streams instaan
                var streams = driver.FindElement(By.CssSelector("#directory-game-main-content > div:nth-child(4) > div > div.Layout-sc-1xcs6mc-0.bYReYr > div.ScTower-sc-1sjzzes-0.czzjEE.tw-tower"));
                try
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        //Haal alle data op van de stream                       
                        var stream = streams.FindElement(By.CssSelector("#directory-game-main-content > div:nth-child(4) > div > div.Layout-sc-1xcs6mc-0.bYReYr > div.ScTower-sc-1sjzzes-0.czzjEE.tw-tower > div:nth-child(" + (i + 1) + ")"));
                        //Zoek in alle data de details
                        string titel = stream.FindElement(By.CssSelector("h3")).Text;
                        string kanaal = stream.FindElement(By.CssSelector("p")).Text;
                        string kijkers = stream.FindElement(By.CssSelector("div.tw-media-card-stat")).Text;
                        string taal = "";
                        try
                        {
                            taal = stream.FindElement(By.CssSelector("div.bPzjwR")).Text;

                        }
                        catch
                        {
                            taal = "Geen taal";
                        }

                        //Fix encoding voor speciale tekens (bv. aziatische letters)
                        Console.OutputEncoding = Encoding.UTF8;
                        //Print stream details
                        Console.WriteLine("_________________________________________________");
                        Console.WriteLine("Stream " + i);
                        Console.WriteLine("Titel: " + titel);
                        Console.WriteLine("Taal: " + taal);
                        Console.WriteLine("Kanaal: " + kanaal);
                        Console.WriteLine("Live kijkers: " + kijkers);
                        Console.WriteLine("_________________________________________________");
                        Console.WriteLine();

                        _data.Add(new TwitchData()
                        {
                            Titel = titel,
                            Taal = taal,
                            Kanaal = kanaal,
                            Kijkers = kijkers,
                        }); ;
                        json = JsonSerializer.Serialize(_data);

                        var newLine = string.Format("{0},{1},{2},{3}", titel, taal, kanaal, kijkers);
                        csv.AppendLine(newLine);
                    }
                }
                catch
                {
                    string error = "Er zijn niet genoeg streams in deze categorie om aan 5 te komen.";
                    Console.WriteLine(error);

                    _data.Add(new TwitchData()
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
                string error = "Categorie bestaat niet.";
                Console.WriteLine(error);

                _data.Add(new TwitchData()
                {
                    Error = error,
                }); ;
                json = JsonSerializer.Serialize(_data);

                var newLine = string.Format("{0}", error);
                csv.AppendLine(newLine);
            }
            WriteFiles(csv, json, "Twitch");
        }
    }
}