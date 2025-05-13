using CarParser.Parser;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.WebUtilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static OpenQA.Selenium.VirtualAuth.VirtualAuthenticatorOptions;

namespace CarParser
{
    public class TransportData
    {
        IWebDriver Driver { get; }

        WebDriverWait DriverWait { get; }

        FilesParser FilesParser { get; }

        public string Domain { get; private set; }

        TransportType TransportType { get; }

        string SiteLink { get; }

        public TransportData(TransportType transportType, IWebDriver driver, string CacheFolderPath, WebDriverWait driverWait, string siteLink)
        {
            Driver = driver;
            DriverWait = driverWait;
            TransportType = transportType;
            SiteLink = siteLink;

            Uri uri = new Uri(SiteLink);
            Domain = uri.GetLeftPart(UriPartial.Authority);

            FilesParser = new FilesParser(Driver, CacheFolderPath, "CarExportedImages");

            GetAllBrands();
        }

        private void GetAllBrands()
        {
            List<TempTransportBrandData> tempTransportBrandDatas = new List<TempTransportBrandData>();
            Dictionary<string, string> brandImageDatas = new Dictionary<string, string>();

            try
            {
                HashSet<string> brands = new HashSet<string>();

                Driver.Navigate().GoToUrl(SiteLink);
                IWebElement demoButton = Driver.FindElement(By.XPath("//input[@value='Демо']"));
                demoButton.Click();

                Driver.Navigate().GoToUrl(SiteLink);

                UpdateUI("Ожидание загрузки всех ссылок...");
                DriverWait.Until(d => d.FindElements(By.TagName("a")).Count > 0);

                var brandElements = Driver.FindElements(By.ClassName("tile"));

                UpdateUI($"Всего брендов: {brandElements.Count}");

                for (int i = 0; i < brandElements.Count; i++)
                {
                    var brandHrefElement = brandElements[i].FindElement(By.TagName("a"));
                    var imgUrlElement = brandElements[i].FindElement(By.ClassName("make-logo-big"));

                    string brandName = brandElements[i].Text;
                    string brandImgUrl = imgUrlElement.GetAttribute("src");

                    if (!brandImageDatas.ContainsKey(brandName))
                    {
                        brandImageDatas.Add(brandName, brandImgUrl);
                    }

                    UpdateUI($"Name: {brandName}, ImgURL: {brandImgUrl}");

                    string brandPageLink = brandHrefElement.GetAttribute("href");

                    if (!string.IsNullOrEmpty(brandPageLink) && brandPageLink != "#" && brandPageLink.Contains("makeId") && !brands.Contains(brandName))
                    {
                        string Id = GetMakeId(brandPageLink);
                        brands.Add(brandName);

                        TempTransportBrandData tempTransportBrandData = new TempTransportBrandData();

                        tempTransportBrandData.Id = Id;
                        tempTransportBrandData.Name = brandName;
                        tempTransportBrandData.ImageLink = brandImgUrl;
                        tempTransportBrandData.brandPageLink = brandPageLink;

                        tempTransportBrandDatas.Add(tempTransportBrandData);
                    }
                }
            }
            catch (NoSuchElementException ex)
            {
                UpdateUI($"Ошибка: Элемент не найден - {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                UpdateUI($"Ошибка: Таймаут ожидания - {ex.Message}");
            }
            catch (WebDriverException ex)
            {
                UpdateUI($"Ошибка WebDriver: {ex.Message}");
            }
            catch (Exception ex)
            {
                UpdateUI($"Неизвестная ошибка: {ex.Message}");
            }

            UpdateUI("Начинается экспорт изображений брендов");
            foreach (var imageData in brandImageDatas)
            {
                FilesParser.ExportImageFile(imageData.Key, imageData.Value);
            }
            UpdateUI("Экспорт изображений марок завершён");

            UpdateUI("Начинается создание модели брендов");
            foreach (var transportBrandData in tempTransportBrandDatas)
            {
                CreateTransportBrandData(transportBrandData.Id, transportBrandData.Name, transportBrandData.ImageLink, transportBrandData.brandPageLink);
            }
            UpdateUI("Создание модели брендов завершено");
        }

        private void CreateTransportBrandData(string Id, string Name, string ImageLink, string brandPageLink)
        {
            TransportBrandData brand = new TransportBrandData(Driver, DriverWait, FilesParser, Domain, Id, Name, ImageLink, brandPageLink);
            brand.GetAllModels();

            if (Brands == null)
            {
                Brands = new HashSet<TransportBrandData>();
            }

            Brands.Add(brand);
        }

        public static string GetMakeId(string url)
        {
            UpdateUI(url);

            Uri uri = new Uri(url);
            var query = QueryHelpers.ParseQuery(uri.Query);

            string makeId = query["makeId"].ToString();

            return makeId;
        }

        static void UpdateUI(string message)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] " + message);
        }

        public void GetTransportData(ref TransportType transportType)
        {
            transportType = TransportType;

            Console.WriteLine($"Transport Type: {TransportType}, Site Link: {SiteLink}");
        }

        public HashSet<TransportBrandData> Brands { get; private set; }

        public class TempTransportBrandData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageLink { get; set; }
            public string brandPageLink { get; set; }
        }
    }
}