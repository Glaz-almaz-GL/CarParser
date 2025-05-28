using EverythingParser.ParserActions;
using Selenium.WebDriver.EventCapture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EverythingParser.ParserConfigurationScripts
{
    public class ParsingPageConfiguration
    {
        public List<ParsingElementConfiguration> ParsingElements { get; set; }
        public string PageUrl { get; private set; }
        private EventCapturingWebDriver CaptureDriver { get; }

        public ParsingPageConfiguration(EventCapturingWebDriver captureDriver, string pageUrl)
        {
            PageUrl = pageUrl;
            ParsingElements = new List<ParsingElementConfiguration>();
            CaptureDriver = captureDriver;
        }

        public void StartParsing(bool _isElementClick = false, string pageUrl = "")
        {
            if (!_isElementClick)
            {
                Console.WriteLine($"Переход на страницу ({PageUrl})");

                if (!string.IsNullOrEmpty(pageUrl))
                {
                    CaptureDriver.Navigate().GoToUrl(pageUrl);
                }
                else
                {
                    CaptureDriver.Navigate().GoToUrl(PageUrl);
                }

            }

            Console.WriteLine("Запуск парсинга");

            foreach (var element in ParsingElements)
            {
                if (WebDriverActions.GetBaseUrl(CaptureDriver.Url) != PageUrl)
                {
                    CaptureDriver.Navigate().GoToUrl(PageUrl);
                }
                element.StartParsing();
            }
        }
    }
}
