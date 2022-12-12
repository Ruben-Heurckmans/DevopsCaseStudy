using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WebscraperTwitch
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //geef zoekterm in voor in de url
            Console.Write("Geef een zoekterm in: ");
            string zoekterm = Console.ReadLine();
            //Verplaats spaties naar %20 zodat de url klopt
            zoekterm = zoekterm.Replace(" ", "%20");
            Console.WriteLine();

            //start driver en browse naar pagina
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.twitch.tv/directory/game/" + zoekterm + "?sort=VIEWER_COUNT");

            List<data> _data = new List<data>();

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
                        var titel = stream.FindElement(By.CssSelector("h3")).Text;
                        var kanaal = stream.FindElement(By.CssSelector("p")).Text;
                        var kijkers = stream.FindElement(By.CssSelector("div.tw-media-card-stat")).Text;
                        var taal = stream.FindElement(By.CssSelector("div.bPzjwR")).Text;

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

                        _data.Add(new data()
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
                string error = "Categorie bestaat niet.";
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

            File.WriteAllText(@"D:\DevOps\Twitch.csv", csv.ToString());
            File.WriteAllText(@"D:\DevOps\Twitch.json", json);
            Console.ReadLine();

        }
    }
}