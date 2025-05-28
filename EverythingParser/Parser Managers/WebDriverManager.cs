using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Math;
using EverythingParser.ParserActions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;
using Selenium.WebDriver.EventCapture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EverythingParser
{
    public class WebDriverManager
    {
        public delegate void DriverUrlHandler(string url);

        public string DownloadFilesFolderPath { get; private set; }
        public IWebDriver Driver { get; private set; }
        public WebDriverWait DriverWait { get; private set; }
        public EventCapturingWebDriver CaptureDriver { get; private set; }
        public bool ShouldCancelNavigation { get; private set; } = true;

        private string LastUrl = "";

        public WebDriverManager()
        {
            InitializeWebDriver();
            WebDriverActions.InjectJavaScriptForNavigationHandling(CaptureDriver);
            _ = StartUrlMonitoring();
        }

        private void InitializeWebDriver()
        {
            DownloadFilesFolderPath = Path.Combine(Path.GetTempPath(), $"EverethingParser", "Download");

            ChromeOptions options = new ChromeOptions();

            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddUserProfilePreference("download.default_directory", DownloadFilesFolderPath);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            Driver = new ChromeDriver(service, options);
            DriverWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
            CaptureDriver = new EventCapturingWebDriver(Driver);

            // Добавляем обработчик события Navigated
            CaptureDriver.Navigated += CaptureDriver_Navigated;
        }

        private void CaptureDriver_Navigated(object sender, WebDriverNavigationEventArgs e)
        {
            // Внедряем скрипт после каждой навигации
            WebDriverActions.InjectJavaScriptForNavigationHandling(CaptureDriver);
        }

        private async Task StartUrlMonitoring()
        {
            while (true)
            {
                await Task.Delay(500); // Проверяем каждые 500 мс

                WebDriver_UrlChanged();
            }
        }

        private void WebDriver_UrlChanged()
        {
            if (!string.IsNullOrEmpty(LastUrl) && Driver.Url != LastUrl)
            {
                Console.WriteLine($"LastUrl: {LastUrl}, DriverUrl: {CaptureDriver.Url}");

                WebDriverActions.InjectJavaScriptForNavigationHandling(CaptureDriver);
            }

            LastUrl = CaptureDriver.Url;
        }

        public void SetInitialLocalStorageValue()
        {
            if (CaptureDriver.Url.StartsWith("data:"))
            {
                Console.WriteLine("URL начинается с 'data:', скрипт не внедряется.");
                return;
            }

            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("localStorage.setItem('shouldCancelNavigation', arguments[0]);", ShouldCancelNavigation.ToString().ToLower());
        }

        public void ClickDemoButton()
        {
            if (CaptureDriver.FindElements(By.XPath("//*[contains(@class, 'demo')]")).Count > 0)
            {
                IWebElement divSubmit = CaptureDriver.FindElement(By.XPath("//*[contains(@class, 'demo')]"));
                IWebElement btSubmit = null;

                if (divSubmit.FindElements(By.XPath(".//*[contains(@name, 'demo')]")).Count == 0)
                {
                    btSubmit = divSubmit.FindElement(By.XPath(".//*[contains(@name, 'submit')]"));

                    if (divSubmit.FindElements(By.XPath(".//*[contains(@name, 'submit')]")).Count == 0)
                    {
                        btSubmit = divSubmit.FindElement(By.XPath(".//*[contains(@name, 'test')]"));
                    }
                }
                else
                {
                    divSubmit.FindElement(By.XPath(".//*[contains(@name, 'demo')]"));
                }

                if (btSubmit != null)
                {
                    btSubmit.Click();
                }
            }
        }

        public void EditBlockNavigation(bool _shouldCancelNavigation)
        {
            ShouldCancelNavigation = _shouldCancelNavigation;

            if (CaptureDriver.Url.StartsWith("data:"))
            {
                Console.WriteLine("URL начинается с 'data:', скрипт не внедряется.");
                return;
            }

            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;

            js.ExecuteScript("localStorage.setItem('shouldCancelNavigation', arguments[0]);", ShouldCancelNavigation.ToString().ToLower());
        }

        public void QuitDrivers()
        {
            CaptureDriver?.Quit();
            Driver?.Quit();
        }
    }
}