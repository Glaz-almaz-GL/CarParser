using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarParser
{
    public class ParserData
    {
        string Name { get; }
        string SiteLink { get; }
        string CacheFolderPath { get; }

        Dictionary<TransportTypes, string> SiteInfo { get; }

        public IWebDriver Driver { get; }

        WebDriverWait DriverWait { get; }

        public ParserData(string name, string siteLink, Dictionary<TransportTypes, string> siteInfo)
        {
            //TransportType = transportType;
            Name = name;
            SiteLink = siteLink;
            SiteInfo = siteInfo;

            CacheFolderPath = Path.Combine(Path.GetTempPath(), $"{Name} Parser", "Download");

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless"); // Запуск в безголовом режиме
            options.AddArgument("--disable-gpu"); // Отключает GPU для улучшения производительности в headless режиме
            options.AddUserProfilePreference("download.default_directory", CacheFolderPath);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            // Инициализация драйвера с указанием удаленного URL
            Driver = new ChromeDriver(service, options);
            DriverWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));

            CreateSiteData();
        }

        public void GetParserData(ref string name)
        {
            name = Name;
        }

        private void CreateSiteData()
        {
            SiteData siteData = new SiteData(Driver, CacheFolderPath, DriverWait, Name, SiteLink, SiteInfo);

            if (Sites == null)
            {
                Sites = new List<SiteData>();
            }
            else
            {
                Sites.Clear();
            }

            Sites.Add(siteData);
        }

        public List<SiteData> Sites { get; private set; }
    }

    public enum TransportTypes
    {
        None,
        Car,
        Motorcycle,
        Truck,
        Trailer,
        Axel
    }
}
