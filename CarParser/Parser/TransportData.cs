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
using static CarParser.DebugLog;

namespace CarParser
{
    public class TransportData
    {
        IWebDriver Driver { get; }

        WebDriverWait DriverWait { get; }

        FilesDownloader FilesParser { get; }

        public string Domain { get; private set; }

        TransportTypes TransportType { get; }

        string SiteLink { get; }

        public TransportData(TransportTypes transportType, IWebDriver driver, string ParserName, string CacheFolderPath, WebDriverWait driverWait, string siteLink)
        {
            Driver = driver;
            DriverWait = driverWait;
            TransportType = transportType;
            SiteLink = siteLink;

            Uri uri = new Uri(SiteLink);
            Domain = uri.GetLeftPart(UriPartial.Authority);

            FilesParser = new FilesDownloader(Driver, ParserName, CacheFolderPath, $"{transportType.ToString()}ExportedImages");

            GetAllTransportBrands();
        }

        private void GetAllTransportBrands()
        {
            List<TempTransportBrandData> tempTransportBrandDatas = new List<TempTransportBrandData>();
            Dictionary<string, string> brandImageDatas = new Dictionary<string, string>();

            try
            {
                HashSet<string> brands = new HashSet<string>();

                Driver.Navigate().GoToUrl(SiteLink);

                if (Driver.FindElements(By.XPath("//input[@value='Демо']")).Count > 0)
                {
                    IWebElement demoButton = Driver.FindElement(By.XPath("//input[@value='Демо']"));
                    demoButton.Click();
                }

                Driver.Navigate().GoToUrl(SiteLink);

                UpdateUI("Log (TransportData.cs): Ожидание загрузки всех ссылок...");
                DriverWait.Until(d => d.FindElements(By.TagName("a")).Count > 0);

                var brandSections = Driver.FindElements(By.Id("makes"));

                if (TransportType == TransportTypes.Truck)
                {
                    brandSections = Driver.FindElements(By.Id("makeTrucks"));
                }
                else if (TransportType == TransportTypes.Trailer)
                {
                    var TrailerBtn = Driver.FindElement(By.ClassName("select-trailer"));
                    TrailerBtn.Click();

                    brandSections = Driver.FindElements(By.Id("makeTrailers"));
                }
                else if (TransportType == TransportTypes.Axel)
                {
                    var AxelBtn = Driver.FindElement(By.ClassName("select-axle"));
                    AxelBtn.Click();

                    brandSections = Driver.FindElements(By.Id("makeAxels"));
                }

                foreach (var section in brandSections)
                {
                    var brandElements = section.FindElements(By.ClassName("tile"));

                    UpdateUI($"Log (TransportData.cs): Всего брендов: {brandElements.Count}");

                    foreach (var brand in brandElements)
                    {
                        var brandHrefElement = brand.FindElement(By.TagName("a"));
                        var imgUrlElement = brand.FindElement(By.ClassName("make-logo-big"));

                        string brandName = "";

                        if (TransportType == TransportTypes.Trailer || TransportType == TransportTypes.Axel)
                        {
                            brandName = brandHrefElement.Text.Trim();
                        }
                        else
                        {
                            brandName = brand.Text.Trim();
                        }

                        string brandImgUrl = imgUrlElement.GetAttribute("src");

                        if (!brandImageDatas.ContainsKey(brandName))
                        {
                            brandImageDatas.Add(brandName, brandImgUrl);
                        }

                        string brandPageLink = brandHrefElement.GetAttribute("href");

                        UpdateUI($"Log (TransportData.cs): Name: {brandName}, ImgURL: {brandImgUrl}, BrandPageLink: {brandPageLink}");

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
            }
            catch (NoSuchElementException ex)
            {
                UpdateUI($"(TransportData.cs): Ошибка: Элемент не найден - {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                UpdateUI($"(TransportData.cs): Ошибка: Таймаут ожидания - {ex.Message}");
            }
            catch (WebDriverException ex)
            {
                UpdateUI($"(TransportData.cs): Ошибка WebDriver: {ex.Message}");
            }
            catch (Exception ex)
            {
                UpdateUI($"(TransportData.cs): Неизвестная ошибка: {ex.Message}");
            }

            UpdateUI("(TransportData.cs): Начинается экспорт изображений брендов");
            foreach (var imageData in brandImageDatas)
            {
                FilesParser.ExportImageFile(imageData.Key, imageData.Value);
            }
            UpdateUI("(TransportData.cs): Экспорт изображений марок завершён");

            UpdateUI("(TransportData.cs): Начинается создание модели брендов");
            foreach (var transportBrandData in tempTransportBrandDatas)
            {
                CreateTransportBrandData(transportBrandData.Id, transportBrandData.Name, transportBrandData.ImageLink, transportBrandData.brandPageLink);
            }
            UpdateUI("(TransportData.cs): Создание модели брендов завершено");
        }

        private void CreateTransportBrandData(string Id, string Name, string ImageLink, string brandPageLink)
        {
            TransportBrandData brand = new TransportBrandData(Driver, DriverWait, FilesParser, TransportType, Domain, Id, Name, ImageLink, brandPageLink);

            if (Brands == null)
            {
                Brands = new List<TransportBrandData>();
            }

            if (!Brands.Contains(brand))
            {
                Brands.Add(brand);
            }
        }

        public static string GetMakeId(string url)
        {
            UpdateUI(url);

            Uri uri = new Uri(url);
            var query = QueryHelpers.ParseQuery(uri.Query);

            string makeId = query["makeId"].ToString();

            return makeId;
        }

        public void GetTransportData(ref TransportTypes transportType)
        {
            transportType = TransportType;

            UpdateUI($"Log (TransportData.cs): Transport Type: {TransportType}, Site Link: {SiteLink}");
        }

        public List<TransportBrandData> Brands { get; private set; }

        public class TempTransportBrandData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageLink { get; set; }
            public string brandPageLink { get; set; }
        }
    }
}