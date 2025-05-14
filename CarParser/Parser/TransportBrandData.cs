using CarParser.Parser;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CarParser.DebugLog;

namespace CarParser
{
    public class TransportBrandData
    {
        public delegate void Event();
        public event Event TransportModelDescriptionCreated;

        IWebDriver Driver { get; }
        WebDriverWait DriverWait { get; }
        FilesDownloader FilesParser { get; }
        TransportTypes TransportType { get; }

        string Domain { get; }

        string Id { get; }
        string Name { get; }
        string ImageLink { get; }
        string BrandPageLink { get; }

        public TransportBrandData(IWebDriver driver, WebDriverWait driverWait, FilesDownloader filesParser, TransportTypes transportType, string domain, string id, string name, string imageLink, string brandPageLink)
        {
            Domain = domain;
            Id = id;
            Driver = driver;
            DriverWait = driverWait;
            TransportType = transportType;
            Name = name;
            ImageLink = imageLink;
            BrandPageLink = brandPageLink;

            FilesParser = filesParser;

            GetAllTransportModels();
        }

        public void GetAllTransportModels()
        {
            List<TempTransportModelData> tempTransportModelDatas = new List<TempTransportModelData>();
            Dictionary<string, string> transportImageDatas = new Dictionary<string, string>();

            try
            {
                Driver.Navigate().GoToUrl(BrandPageLink);

                var TransportModels = Driver.FindElements(By.ClassName("tiles"));

                foreach (var transportModel in TransportModels)
                {
                    var buttons = transportModel.FindElements(By.TagName("a"));

                    foreach (var btn in buttons)
                    {
                        string transportModelName = btn.FindElement(By.TagName("p")).Text;

                        var divImgWrap = btn.FindElement(By.ClassName("imgWrap"));
                        var transportModelImageElement = divImgWrap.FindElement(By.ClassName("model-group-image"));

                        string transportModelImageLink = transportModelImageElement.GetAttribute("src");

                        if (!transportImageDatas.ContainsKey($"({Name}) " + transportModelName))
                        {
                            transportImageDatas.Add($"({Name}) " + transportModelName, transportModelImageLink);
                        }

                        if (!string.IsNullOrEmpty(btn.GetAttribute("href")) && btn.GetAttribute("href") != "#" && btn.GetAttribute("href").Contains("Id"))
                        {
                            string transportModelPageLink = btn.GetAttribute("href");

                            TempTransportModelData tempTransportModelData = new TempTransportModelData();

                            tempTransportModelData.ModelName = transportModelName;
                            tempTransportModelData.ModelImageLink = transportModelImageLink;
                            tempTransportModelData.ModelPageLink = transportModelPageLink;

                            tempTransportModelDatas.Add(tempTransportModelData);
                        }
                    }
                }
            }
            catch (WebDriverException ex)
            {
                UpdateUI($"WebDriverException (TransportBrandData.cs): {ex.Message}");
            }
            catch (Exception ex)
            {
                UpdateUI($"Exception (TransportBrandData.cs): {ex.Message}");
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
            TransportModelData transportModel = new TransportModelData(Driver, DriverWait, FilesParser, Domain, TransportType, Name, ModelName, ModelImageLink, ModelPageLink);

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

            UpdateUI($"Log (TransportBrandData.cs): Brand Id: {Id}, Brand Name: {Name}, Brand Page Link: {BrandPageLink}");
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
