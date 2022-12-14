using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace WebscraperIctJob
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
            driver.Navigate().GoToUrl("https://www.ictjob.be/nl/it-vacatures-zoeken?keywords=" + zoekterm);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);

            var sortbutton = driver.FindElement(By.CssSelector("#sort-by-date"));
            sortbutton.Click();

            List<data> _data = new List<data>();

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
                            try
                            {
                                keywords = vacature.FindElement(By.CssSelector(".job-keywords")).Text;
                            }
                            catch
                            {
                                keywords = "Geen keywords";
                            }
                            var link = vacature.FindElement(By.CssSelector(".job-title")).GetAttribute("href");

                            //Print Job details
                            Console.WriteLine("_________________________________________________");
                            if (i<=3)
                            {
                                Console.WriteLine("Job " + i);
                            }
                            else
                            {
                                Console.WriteLine("Job " + (i-1));
                            }
                            Console.WriteLine("Titel: " + titel);
                            Console.WriteLine("Bedrijf: " + bedrijf);
                            Console.WriteLine("Locatie: " + locatie);
                            Console.WriteLine("Keywords: " + keywords);
                            Console.WriteLine("Link: " + link);

                            Console.WriteLine("_________________________________________________");
                            Console.WriteLine();

                            _data.Add(new data()
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
                    string error = "Er zijn geen jobs met deze zoekterm.";
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

            File.WriteAllText(@"D:\DevOps\IctJob.csv", csv.ToString());
            File.WriteAllText(@"D:\DevOps\IctJob.json", json);
            Console.ReadLine();

        }
    }
}
