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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace CarParser
{
    public class TransportModelData
    {
        public delegate void Event();
        public event Event DescriptionCreated;

        IWebDriver Driver { get; }
        WebDriverWait DriverWait { get; }

        FilesParser FilesParser { get; }

        string Domain { get; }
        string BrandName { get; }
        string Id;
        string Name { get; }
        string ImageLink { get; }
        string ModelPageLink { get; }

        public List<TransportModificationDescriptionData> ModificationDescriptions { get; private set; }

        public TransportModelData(IWebDriver driver, WebDriverWait driverWait, FilesParser filesParser, string domain, string brandName, string name, string imageLink, string modelPageLink)
        {
            Domain = domain;
            Driver = driver;
            FilesParser = filesParser;
            DriverWait = driverWait;
            Name = name;
            BrandName = brandName;
            ImageLink = imageLink;
            ModelPageLink = modelPageLink;

            UpdateUI(ModelPageLink);
        }

        public void GetFullModelDescription()
        {
            Dictionary<string, string> PDFFiles = new Dictionary<string, string>();
            Dictionary<string, string> ImageFiles = new Dictionary<string, string>();

            Id = GetTransportModelId(ModelPageLink);

            Console.WriteLine(ModelPageLink);

            Driver.Navigate().GoToUrl(ModelPageLink);

            DriverWait.Until(d => d.FindElement(By.TagName("table")));

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
                    string DataUrl = Row.GetAttribute("data-url");

                    string ModificationId = GetTransportModificationId(DataUrl);
                    string Type = "";
                    string EngineCode = "";
                    string FuelType = "";
                    string Volume = "", Power = "";
                    string ModelYear = "";

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

                    CreateTransportModificationDescriptionData(ModificationId, Type, EngineCode, FuelType, Volume, Power, ModelYear);
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

            //Console.WriteLine($"[------------------------------]\nОписания модели созданы: \nНазвание: {Name}\nСсылка на изображение: {ImageLink}\nId модели: {Id}\nPDF файлы (названия): {PDFFiles.Keys.ToString()}\nPDF файлы (ссылки): {PDFFiles.Values.ToString()}\nИзображения (названия): {ImageFiles.Keys}\nИзображения (ссылки): {ImageFiles.Values}\n[------------------------------]");
        }

        public void GetModelData(ref string modelId, ref string modelName, ref string modelPageLink)
        {
            modelId = Id;
            modelName = Name;
            modelPageLink = ModelPageLink;

            Console.WriteLine($"Model Id: {Id}, Model Name: {Name}, Model Page Link: {ModelPageLink}");
        }

        private void CreateTransportModificationDescriptionData(string ModificationId, string Type, string EngineCode, string FuelType, string Volume, string Power, string ModelYear)
        {
            TransportModificationDescriptionData ModificationDescription = new TransportModificationDescriptionData(ModificationId, Type, EngineCode, FuelType, Volume, Power, ModelYear);

            if (ModificationDescriptions == null)
            {
                ModificationDescriptions = new List<TransportModificationDescriptionData>();
            }

            ModificationDescriptions.Add(ModificationDescription);
        }

        private static string GetTransportModelId(string url)
        {
            Uri uri = new Uri(url);
            var query = QueryHelpers.ParseQuery(uri.Query);

            string modelGroupId = query["modelGroupId"].ToString();

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

        private static void UpdateUI(string message)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] " + message);
        }
    }
}