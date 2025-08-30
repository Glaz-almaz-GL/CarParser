using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HaynesProParser.Managers.WebManagers
{
    public static class WebDriverManager
    {
        public static int ParallelMakersCount { get; set; } = 0;
        public static int ParallelModelsCount { get; set; } = 0;
        public static int ParallelModificationCount { get; set; } = 0;
        public static int ParallelGuideTypesCount { get; set; } = 0;
        public static int NavigateExceptionTimeOutMinutes { get; set; } = 10;
        public static int TimeOutCount { get; private set; }
        private static readonly List<(ChromeDriver, WebDriverWait)> _allDriverPools = [];

        public static ChromeDriver CurrentDriver { get; set; }
        public static WebDriverWait CurrentWebDriverWait { get; set; }

        private static string LoginUrl = "https://www.workshopdata.com/touch/site/layout/wsdLogin";

        #region CreateDriver

        private static readonly List<string> UserAgents =
[
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/121.0",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Safari/605.1.15",
    "Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Mobile/15E148 Safari/604.1"
];

        private static readonly Random Random = new();

        private static ChromeOptions CreateDriverOptions()
        {
            ChromeOptions options = new();

            // Удалить дубли и оставить только нужные
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-plugins");
            options.AddArgument("--disable-web-security");
            options.AddArgument("--disable-features=VizDisplayCompositor");
            options.AddArgument("--disable-features=IsolateOrigins,site-per-process");
            options.AddArgument("--disable-ipc-flooding-protection");
            options.AddArgument("--disable-background-timer-throttling");
            options.AddArgument("--disable-renderer-backgrounding");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-automation");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--start-maximized");
            options.AddArgument("--force-device-scale-factor=1");
            options.AddArgument("--high-dpi-support=1");

            // Отключение отслеживания автоматизации
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            // Настройки профиля
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);

            // --- Генерация случайного User-Agent ---
            string randomUserAgent = UserAgents[Random.Next(UserAgents.Count)];
            options.AddArgument($"--user-agent={randomUserAgent}");

            return options;
        }

        public static void CreateCurrentDriver(bool hideDriver = false)
        {
            if (CurrentDriver == null)
            {
                ChromeOptions options = CreateDriverOptions();

                ChromeDriverService service = ChromeDriverService.CreateDefaultService();

                if (hideDriver)
                {
                    options.AddArgument("--headless");
                    service.HideCommandPromptWindow = true;
                }

                // Инициализация драйвера с указанием удаленного URL
                CurrentDriver = new ChromeDriver(service, options);
                CurrentWebDriverWait = new WebDriverWait(CurrentDriver, TimeSpan.FromSeconds(2));

                _allDriverPools.Add((CurrentDriver, CurrentWebDriverWait));
            }
        }

        public static (ChromeDriver, WebDriverWait) CreateAnotherDriver(bool headless = false)
        {
            ChromeOptions options = CreateDriverOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();

            if (headless)
            {
                options.AddArgument("--headless");
                service.HideCommandPromptWindow = true;
            }

            // Инициализация драйвера с указанием удаленного URL
            ChromeDriver driver = new ChromeDriver(service, options);
            WebDriverWait webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            _allDriverPools.Add((driver, webDriverWait));

            return (driver, webDriverWait);
        }

        #endregion

        #region Login

        public static void Login()
        {
            Navigate(CurrentDriver, LoginUrl);
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("username"))).SendKeys("stivari@officinestivari.it");
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("password"))).SendKeys("RUDY POZZETTO");
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("remember"))).Click();
            CurrentWebDriverWait.Until(d => d.FindElement(By.XPath("//input[@value='Вход']"))).Click();
        }

        public static void Login(ChromeDriver driver, WebDriverWait driverWait)
        {
            Navigate(driver, LoginUrl);

            try
            {
                driverWait.Until(d => d.FindElement(By.Id("username"))).SendKeys("stivari@officinestivari.it");
                driverWait.Until(d => d.FindElement(By.Id("password"))).SendKeys("RUDY POZZETTO");
                driverWait.Until(d => d.FindElement(By.Id("remember"))).Click();
                driverWait.Until(d => d.FindElement(By.XPath("//input[@value='Вход']"))).Click();
            }
            catch (NoSuchElementException) { Login(driver, driverWait); }
        }

        public static void Login(string userName, string userPassword)
        {
            Navigate(CurrentDriver, LoginUrl);
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("username"))).SendKeys(userName);
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("password"))).SendKeys(userPassword);
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("remember"))).Click();
            CurrentWebDriverWait.Until(d => d.FindElement(By.XPath("//input[@value='Вход']"))).Click();
        }

        public static async Task LoginAsync()
        {
            await NavigateAsync(CurrentDriver, LoginUrl);
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("username"))).SendKeys("stivari@officinestivari.it");
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("password"))).SendKeys("RUDY POZZETTO");
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("remember"))).Click();
            CurrentWebDriverWait.Until(d => d.FindElement(By.XPath("//input[@value='Вход']"))).Click();
        }

        public static async Task LoginAsync(ChromeDriver driver, WebDriverWait driverWait)
        {
            await NavigateAsync(driver, LoginUrl);
            try
            {
                driverWait.Until(d => d.FindElement(By.Id("username"))).SendKeys("stivari@officinestivari.it");
                driverWait.Until(d => d.FindElement(By.Id("password"))).SendKeys("RUDY POZZETTO");
                driverWait.Until(d => d.FindElement(By.Id("remember"))).Click();
                driverWait.Until(d => d.FindElement(By.XPath("//input[@value='Вход']"))).Click();
            }
            catch (NoSuchElementException) { await LoginAsync(driver, driverWait); }
        }

        public static async Task LoginAsync(string userName, string userPassword)
        {
            await NavigateAsync(CurrentDriver, LoginUrl);
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("username"))).SendKeys(userName);
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("password"))).SendKeys(userPassword);
            CurrentWebDriverWait.Until(d => d.FindElement(By.Id("remember"))).Click();
            CurrentWebDriverWait.Until(d => d.FindElement(By.XPath("//input[@value='Вход']"))).Click();
        }

        #endregion

        #region DemoLogin
        public static void DemoLogin()
        {
            Navigate(CurrentDriver, LoginUrl);
            CurrentWebDriverWait.Until(d => d.FindElement(By.XPath("//input[@value='Демо']"))).Click();
        }

        public static void DemoLogin(ChromeDriver driver, WebDriverWait driverWait)
        {
            Login(driver, driverWait);

            //Navigate(driver, LoginUrl);
            //driverWait.Until(d => d.FindElement(By.XPath("//input[@value='Демо']"))).Click();
        }

        public static async Task DemoLoginAsync()
        {
            await NavigateAsync(CurrentDriver, LoginUrl);
            CurrentWebDriverWait.Until(d => d.FindElement(By.XPath("//input[@value='Демо']"))).Click();
        }

        public static async Task DemoLoginAsync(ChromeDriver driver, WebDriverWait driverWait)
        {
            await LoginAsync(driver, driverWait);

            //await NavigateAsync(driver, LoginUrl);
            //driverWait.Until(d => d.FindElement(By.XPath("//input[@value='Демо']"))).Click();
        }
        #endregion

        public static void Navigate(ChromeDriver driver, string url)
        {
            try
            {
                driver.Navigate().GoToUrl(url);

                if (driver.Url != url)
                {
                    driver.Navigate().GoToUrl(url);
                }
            }
            catch (WebDriverException ex)
            {
                if (ex.Message.Contains("timed out after"))
                {
                    Debug.WriteLine($"Server is out, wait {NavigateExceptionTimeOutMinutes} min.");
                    TimeOutCount++;
                    Task.Delay(TimeSpan.FromMinutes(NavigateExceptionTimeOutMinutes)).Wait();
                    Navigate(driver, url);
                }
            }
        }

        public static async Task NavigateAsync(ChromeDriver driver, string url)
        {
            try
            {
                await driver.Navigate().GoToUrlAsync(url);

                if (driver.Url != url)
                {
                    await driver.Navigate().GoToUrlAsync(url);
                }
            }
            catch (WebDriverException ex)
            {
                if (ex.Message.Contains("timed out after"))
                {
                    Debug.WriteLine($"Server is out, wait {NavigateExceptionTimeOutMinutes} min.");
                    TimeOutCount++;
                    await Task.Delay(TimeSpan.FromMinutes(NavigateExceptionTimeOutMinutes));
                    await NavigateAsync(driver, url);
                }
            }
        }

        public static void InitializeDriverCleanup()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Debug.WriteLine("AppDomain.ProcessExit: Закрываем все драйверы...");
                Quit();
            };

            // Также ловим unhandled-исключения
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Debug.WriteLine($"Критическая ошибка: {e.ExceptionObject}");
                Quit();
            };
        }

        public static void Quit()
        {
            foreach (var driverPool in _allDriverPools)
            {
                try
                {
                    driverPool.Item1?.Quit();
                    driverPool.Item1?.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    Debug.WriteLine($"Драйвер уже был закрыт");
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine($"Драйвер уже не доступен");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при закрытии драйвера: {ex.Message}");
                }
            }

            _allDriverPools.Clear();
            Debug.WriteLine("Все драйверы закрыты.");
        }
    }
}