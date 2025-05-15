using CarParser.Parser;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.WebUtilities;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V134.Schema;
using OpenQA.Selenium.Support.UI;
using SharpVectors.Dom.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static CarParser.DebugLog;

namespace CarParser
{
    public class TransportModelData
    {
        public delegate void Event();
        public event Event DescriptionCreated;

        IWebDriver Driver { get; }
        WebDriverWait DriverWait { get; }
        FilesDownloader FilesParser { get; }
        TransportTypes TransportType { get; }

        string Domain { get; }
        string BrandName { get; }
        string Id;
        string Name { get; }
        string ImageLink { get; }
        string ModelPageLink { get; }

        public List<CarModificationDescriptionData> CarModificationDescriptions { get; private set; }
        public List<MotorcycleModificationDescriptionData> MotorcycleModificationDescriptions { get; private set; }
        public List<TruckModificationDescriptionData> TruckModificationDescriptions { get; private set; }
        public List<TrailerModificationDescriptionData> TrailerModificationDescriptions { get; private set; }

        public TransportModelData(IWebDriver driver, WebDriverWait driverWait, FilesDownloader filesParser, string domain, TransportTypes transportType, string brandName, string name, string imageLink, string modelPageLink)
        {
            Domain = domain;
            Driver = driver;
            FilesParser = filesParser;
            DriverWait = driverWait;
            TransportType = transportType;
            Name = name;
            BrandName = brandName;
            ImageLink = imageLink;
            ModelPageLink = modelPageLink;

            GetFullModelDescription();
        }

        public void GetFullModelDescription()
        {
            Dictionary<string, string> PDFFiles = new Dictionary<string, string>();
            Dictionary<string, string> ImageFiles = new Dictionary<string, string>();

            string DataUrl = "", ModificationId = "";
            string Type = "Не указан";
            string Bridge = "Отсутствует";
            string EngineCode = "Отсутствует";
            string FuelType = "Отсутствует";
            string Volume = "Отсутствует", Power = "Отсутствует";
            string ModelYear = "Не указан";
            string ManufactureType = "Отсутствует";
            string Weight = "Не указан";

            string Group = "Не указан";
            string MakeName = "Не указан";
            string Code = "Не указан";
            string Description = "Отсутствует";

            Id = GetTransportModelId(ModelPageLink);

            Driver.Navigate().GoToUrl(ModelPageLink);

            DriverWait.Until(d => d.FindElement(By.TagName("table")));

            if (TransportType == TransportTypes.Trailer)
            {
                var btnShowMore = Driver.FindElements(By.Id("showMore"));
                foreach (var btn in btnShowMore)
                {
                    while (Driver.FindElements(By.ClassName("hidden")).Count > 0)
                    {
                        btn.Click();
                    }
                }

                var modificationLists = Driver.FindElements(By.ClassName("customComponentList"));
                foreach (var modificationList in modificationLists)
                {
                    var modificationRows = modificationList.FindElements(By.TagName("li"));
                    foreach (var modificationRow in modificationRows)
                    {
                        ModificationId = modificationRow.GetAttribute("Id");

                        var cells = modificationRow.FindElements(By.TagName("span"));

                        foreach (var cell in cells)
                        {
                            switch (cell.GetAttribute("class"))
                            {
                                case "group":
                                    Group = cell.Text.Trim();
                                    break;

                                case "make":
                                    MakeName = cell.Text.Trim();
                                    break;

                                case "componentType":
                                    Type = cell.Text.Trim();
                                    break;

                                case "code":
                                    Code = cell.Text.Trim();
                                    break;

                                case "description":
                                    Description = cell.Text.Trim();
                                    break;
                            }
                        }

                        CreateTransportModificationDescriptionData(ModificationId, Group, Type, MakeName, EngineCode, Code, FuelType, Volume, Bridge, Power, ModelYear, ManufactureType, Description, Weight);
                    }
                }
            }
            else
            {
                var IdLocationButtons = Driver.FindElements(By.ClassName("id-location-btn"));

                Regex repairManualRegex = new Regex(@"https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/repairManuals\?modelId=\w+&groupId=\w+&storyId=\w+(&altView=true&extraInfo=true)?");
                Regex jackLocationRegex = new Regex(@"^\/touch\/site\/layout\/jackingPoints\?modelId=d_\d+");
                Regex diagnosticConnectorRegex = new Regex(@"^\/touch\/site\/layout\/eobdConnectorLocations\?modelId=d_\d+");

                foreach (var IdLocationButton in IdLocationButtons)
                {
                    string href = IdLocationButton.GetAttribute("href");
                    string dataUrl = IdLocationButton.GetAttribute("data-url");

                    if (!string.IsNullOrEmpty(dataUrl) && (jackLocationRegex.IsMatch(dataUrl) || diagnosticConnectorRegex.IsMatch(dataUrl)) && !ImageFiles.ContainsKey($"({BrandName})" + IdLocationButton.Text))
                    {
                        ImageFiles.Add($"({BrandName})" + IdLocationButton.Text, Domain + dataUrl);
                    }
                    if (!string.IsNullOrEmpty(href) && repairManualRegex.IsMatch(href) && !PDFFiles.ContainsKey($"({BrandName})" + IdLocationButton.Text))
                    {
                        PDFFiles.Add($"({BrandName})" + IdLocationButton.Text, href);
                    }
                }

                // Получение таблицы
                var Table = Driver.FindElement(By.TagName("table"));

                // Получение всех строк таблицы
                var TableRows = Table.FindElements(By.TagName("tr"));

                foreach (var Row in TableRows)
                {
                    if (Row.GetAttribute("class") != "heading")
                    {
                        DataUrl = Row.GetAttribute("data-url");

                        ModificationId = GetTransportModificationId(DataUrl);

                        string classAttribute = Row.GetAttribute("class");

                        // Разбиваю строку на отдельные классы
                        string[] classes = classAttribute.Split(' ');

                        // Получаю первый класс
                        string fuelClassType = classes[0];

                        switch (fuelClassType)
                        {
                            case "petrol":
                                FuelType = "Бензин";
                                break;

                            case "diesel":
                                FuelType = "Дизель";
                                break;

                            case "cng":
                                FuelType = "Природный газ";
                                break;

                            case "hydrogen":
                                FuelType = "Водород";
                                break;

                            case "electric":
                                FuelType = "Электроэнергия";
                                break;

                            case "hybrid":
                                FuelType = "Гибрид";
                                break;

                            case "":
                                FuelType = "Не указано";
                                break;
                        }

                        // Получение ячеек (<td>) в текущей строке
                        var Cells = Row.FindElements(By.TagName("td"));

                        if (TransportType != TransportTypes.Truck)
                        {
                            for (int i = 0; i < Cells.Count; i++)
                            {
                                IWebElement cell = Cells[i];

                                switch (i)
                                {
                                    case 0:
                                        Type = cell.Text.Trim();
                                        break;

                                    case 1:
                                        EngineCode = cell.Text.Trim();
                                        break;

                                    case 2:
                                        Volume = cell.Text.Trim();
                                        break;

                                    case 3:
                                        Power = cell.Text.Trim();
                                        break;

                                    case 4:
                                        ModelYear = cell.Text.Trim();
                                        break;
                                }
                            }
                        }
                        else if (TransportType == TransportTypes.Truck)
                        {
                            for (int i = 0; i < Cells.Count; i++)
                            {
                                IWebElement cell = Cells[i];

                                switch (i)
                                {
                                    case 0:
                                        Type = cell.Text.Trim();
                                        break;

                                    case 1:
                                        Bridge = cell.Text.Trim();
                                        break;

                                    case 2:
                                        EngineCode = cell.Text.Trim();
                                        break;

                                    case 3:
                                        Volume = cell.Text.Trim();
                                        break;

                                    case 4:
                                        Power = cell.Text.Trim();
                                        break;

                                    case 5:
                                        ModelYear = cell.Text.Trim();
                                        break;

                                    case 6:
                                        ManufactureType = cell.Text.Trim();
                                        if (string.IsNullOrEmpty(ManufactureType))
                                        {
                                            ManufactureType = "Не указано";
                                        }
                                        break;

                                    case 7:
                                        Weight = cell.Text.Trim();
                                        break;
                                }
                            }
                        }

                        CreateTransportModificationDescriptionData(ModificationId, Group, Type, MakeName, EngineCode, Code, FuelType, Volume, Bridge, Power, ModelYear, ManufactureType, Description, Weight);
                    }
                }
            }

            foreach (var image in ImageFiles)
            {
                FilesParser.ExportImageFile(image.Key, image.Value);
            }
            foreach (var pdf in PDFFiles)
            {
                FilesParser.ExportImageFile(pdf.Key, pdf.Value);
            }
        }

        public void GetModelData(ref string modelId, ref string modelName, ref string modelPageLink)
        {
            modelId = Id;
            modelName = Name;
            modelPageLink = ModelPageLink;

            UpdateUI($"GetModelData (TransportModelData.cs): Model Id: {Id}, Model Name: {Name}, Model Page Link: {ModelPageLink}");
        }

        private void CreateTransportModificationDescriptionData(string ModificationId, string Group, string Type, string MakeName, string EngineCode, string Code, string FuelType, string Volume, string Bridge, string Power, string ModelYear, string ManufactureType, string Description, string Weight)
        {
            switch (TransportType)
            {
                case TransportTypes.Car:
                    if (CarModificationDescriptions == null)
                    {
                        CarModificationDescriptions = new List<CarModificationDescriptionData>();
                    }

                    CarModificationDescriptionData CarModificationDescription = new CarModificationDescriptionData(ModificationId, Type, EngineCode, FuelType, Volume, Power, ModelYear);
                    CarModificationDescriptions.Add(CarModificationDescription);
                    break;

                case TransportTypes.Motorcycle:
                    if (MotorcycleModificationDescriptions == null)
                    {
                        MotorcycleModificationDescriptions = new List<MotorcycleModificationDescriptionData>();
                    }

                    MotorcycleModificationDescriptionData MotorcycleModificationDescription = new MotorcycleModificationDescriptionData(ModificationId, Type, EngineCode, FuelType, Volume, Power, ModelYear);
                    MotorcycleModificationDescriptions.Add(MotorcycleModificationDescription);
                    break;

                case TransportTypes.Truck:
                    if (TruckModificationDescriptions == null)
                    {
                        TruckModificationDescriptions = new List<TruckModificationDescriptionData>();
                    }

                    TruckModificationDescriptionData TruckModificationDescription = new TruckModificationDescriptionData(ModificationId, Group, Type, EngineCode, Volume, Bridge, Power, ModelYear, ManufactureType, Weight);
                    TruckModificationDescriptions.Add(TruckModificationDescription);
                    break;

                case TransportTypes.Trailer:
                    if (TrailerModificationDescriptions == null)
                    {
                        TrailerModificationDescriptions = new List<TrailerModificationDescriptionData>();
                    }

                    TrailerModificationDescriptionData TrailerModificationDescription = new TrailerModificationDescriptionData(ModificationId, Group, Type, MakeName, Code, Description);
                    TrailerModificationDescriptions.Add(TrailerModificationDescription);
                    break;
            }
        }

        private static string GetTransportModelId(string url)
        {
            Uri uri = new Uri(url);
            var query = QueryHelpers.ParseQuery(uri.Query);

            string modelGroupId = "Unknown";

            if (url.Contains("modelGroupId"))
            {
                modelGroupId = query["modelGroupId"].ToString();
            }
            else if (url.Contains("typeId"))
            {
                modelGroupId = query["typeId"].ToString();
            }

            return modelGroupId;
        }

        private string GetTransportModificationId(string url)
        {
            // Создаем UriBuilder из строки URL
            UriBuilder uriBuilder = new UriBuilder(Domain + url);
            Uri uri = uriBuilder.Uri;

            // Получаем коллекцию параметров запроса
            var query = HttpUtility.ParseQueryString(uri.Query);

            // Извлекаем значение параметра typeId
            return query["typeId"];
        }
    }
}