using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WebscraperYt
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.Write("Geef een zoekterm in: ");
            string zoekterm = Console.ReadLine();

            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.youtube.com/results?search_query=" + zoekterm + "&sp=CAI%253D");


            var cookiebutton = driver.FindElement(By.XPath("/html/body/ytd-app/ytd-consent-bump-v2-lightbox/tp-yt-paper-dialog/div[4]/div[2]/div[6]/div[1]/ytd-button-renderer[1]/yt-button-shape/button"));
            cookiebutton.Click();
            /*
            var searchbar = driver.FindElement(By.XPath("/html/body/ytd-app/div[1]/div/ytd-masthead/div[3]/div[2]/ytd-searchbox/form/div[1]/div[1]/div/div[2]/input"));
            searchbar.SendKeys(zoekterm);
            */

            //ytd-video-renderer:nth-child(2)
            //var videos = driver.FindElements(By.CssSelector("ytd-video-renderer"));
            Console.WriteLine();
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

                weergaven.Replace("weergaven", "");

                Console.WriteLine("Link: " + link);
                Console.WriteLine("Titel: " + titel);
                Console.WriteLine("Kanaal: " + uploader);
                Console.WriteLine("Aantal weergaven: " + weergaven.Replace("weergaven", ""));
                Console.WriteLine("_________________________________________________");
                Console.WriteLine();
            }
            Console.ReadKey();



        }
    }
}