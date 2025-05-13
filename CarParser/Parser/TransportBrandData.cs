using CarParser.Parser;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarParser
{
    public class TransportBrandData
    {
        public delegate void Event();
        public event Event TransportModelDescriptionCreated;

        IWebDriver Driver { get; }
        WebDriverWait DriverWait { get; }

        FilesParser FilesParser { get; }

        string Domain { get; }

        string Id { get; }
        string Name { get; }
        string ImageLink { get; }
        string BrandPageLink { get; }

        public TransportBrandData(IWebDriver driver, WebDriverWait driverWait, FilesParser filesParser, string domain, string id, string name, string imageLink, string brandPageLink)
        {
            Domain = domain;
            Id = id;
            Driver = driver;
            DriverWait = driverWait;
            Name = name;
            ImageLink = imageLink;
            BrandPageLink = brandPageLink;

            FilesParser = filesParser;

            //_ = GetAllModels();
        }

        public void GetAllModels()
        {
            List<TempTransportModelData> tempTransportModelDatas = new List<TempTransportModelData>();
            Dictionary<string, string> transportImageDatas = new Dictionary<string, string>();

            try
            {
                Driver.Navigate().GoToUrl(BrandPageLink);

                Console.WriteLine($"BrandPageLink: {BrandPageLink}");

                //UpdateUI("Ожидание загрузки всех ссылок...");
                DriverWait.Until(d => d.FindElements(By.ClassName("tile")).Count > 0);

                var CarModels = Driver.FindElements(By.ClassName("tile"));

                foreach (var carModel in CarModels)
                {
                    var btn = carModel.FindElement(By.TagName("a"));

                    string carModelName = btn.FindElement(By.TagName("p")).Text;

                    var divImgWrap = btn.FindElement(By.ClassName("imgWrap"));
                    var carModelImageElement = divImgWrap.FindElement(By.ClassName("model-group-image"));

                    string carModelImageLink = carModelImageElement.GetAttribute("src");

                    if (!transportImageDatas.ContainsKey($"({Name})" + carModelName))
                    {
                        transportImageDatas.Add($"({Name})" + carModelName, carModelImageLink);
                    }

                    if (!string.IsNullOrEmpty(btn.GetAttribute("href")) && btn.GetAttribute("href") != "#" && btn.GetAttribute("href").Contains("modelGroupId"))
                    {
                        string carModelPageLink = btn.GetAttribute("href");

                        TempTransportModelData tempTransportModelData = new TempTransportModelData();

                        tempTransportModelData.ModelName = carModelName;
                        tempTransportModelData.ModelImageLink = carModelImageLink;
                        tempTransportModelData.ModelPageLink = carModelPageLink;

                        tempTransportModelDatas.Add(tempTransportModelData);
                    }
                }
            }
            catch (WebDriverException ex)
            {
                Console.WriteLine($"WebDriverException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            foreach (var transportImageData in transportImageDatas)
            {
                FilesParser.ExportImageFile(transportImageData.Key, transportImageData.Value);
            }
            foreach (var tempTransportModelData in tempTransportModelDatas)
            {
                CreateTransportData(tempTransportModelData.ModelName, tempTransportModelData.ModelImageLink, tempTransportModelData.ModelPageLink);
            }
        }

        private void CreateTransportData(string ModelName, string ModelImageLink, string ModelPageLink)
        {
            TransportModelData transportModel = new TransportModelData(Driver, DriverWait, FilesParser, Domain, Name, ModelName, ModelImageLink, ModelPageLink);
            transportModel.GetFullModelDescription();

            if (TransportModels == null)
            {
                TransportModels = new List<TransportModelData>();
            }
            TransportModels.Add(transportModel);
        }

        public void GetBrandData(ref string brandId, ref string brandName, ref string brandPageLink)
        {
            brandId = Id;
            brandName = Name;
            brandPageLink = BrandPageLink;

            Console.WriteLine($"Brand Id: {Id}, Brand Name: {Name}, Brand Page Link: {BrandPageLink}");
        }

        public List<TransportModelData> TransportModels { get; private set; }

        class TempTransportModelData
        {
            public string ModelName { get; set; }
            public string ModelImageLink { get; set; }
            public string ModelPageLink { get; set; }
        }
    }
}
