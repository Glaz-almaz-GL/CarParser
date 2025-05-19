using CarParser.Parser;
using DocumentFormat.OpenXml.Office2010.Excel;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.WebDriver.EventCapture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EverythingParser
{
    public enum ParserActionType
    {
        None,
        DownloadFile,
        DownloadAttributes,
        DownloadIdFromURL
    }

    public class AttributeActions
    {
        public EventCapturingWebDriver CaptureDriver { get; private set; }
        public WebDriverWait DriverWait { get; private set; }
        public WebDriverManager WebDriverManager { get; private set; }
        public FilesDownloader FilesDownloader { get; private set; }
        public string DownloadFolder { get; private set; }
        public string PageURL { get; private set; }
        public string ElementXPath { get; private set; }
        public string AttributeName { get; private set; }
        public List<string> AttributesForParsing { get; private set; } = new List<string>();
        public ParserActionType ActionType { get; private set; }
        public List<string> AttributesForOpenUrl { get; private set; } = new List<string>();

        public AttributeActions(EventCapturingWebDriver captureDriver, WebDriverWait driverWait, WebDriverManager webDriverManager, FilesDownloader filesDownloader, string downloadFolder, string pageUrl, string elementXPath, string attributeName, ParserActionType actionType)
        {
            CaptureDriver = captureDriver;
            DriverWait = driverWait;
            WebDriverManager = webDriverManager;
            FilesDownloader = filesDownloader;
            DownloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"Parser (EverethingParser)", "Files");
            PageURL = pageUrl;
            ElementXPath = elementXPath;
            AttributeName = attributeName;
            ActionType = actionType;
        }

        public void StartAction()
        {
            CaptureDriver.Navigate().GoToUrl(PageURL);

            string[] parts = ElementXPath.Split('/');
            int lastNumberIndex = WebDriverManager.FindLastNumberIndex(parts);
            if (lastNumberIndex > 0)
            {
                ProcessElements(parts, lastNumberIndex);
            }
            else
            {
                Console.WriteLine("Элемент с числом не найден.");
            }
        }

        private void ProcessElements(string[] parts, int lastNumberIndex)
        {
            string clickedElementNameTag = parts[lastNumberIndex].Split('[')[0];
            string clickedParentElementXpath = string.Join("/", parts.Take(lastNumberIndex));
            var clickedParentElement = CaptureDriver.FindElement(By.XPath(clickedParentElementXpath));
            var clickedElements = clickedParentElement.FindElements(By.TagName(clickedElementNameTag)).Distinct();
            int i = 0;

            MessageBox.Show(clickedElements.Count().ToString());

            foreach (var clickedElement in clickedElements)
            {
                string elementText = WebDriverManager.GetElementText(clickedElement);
                List<string> attributesValues = WebDriverManager.GetAllAttributeValues(clickedElement, AttributeName);

                if (attributesValues.Count >= 2)
                {
                    MessageBox.Show(attributesValues.Count.ToString());
                }

                foreach (string attributeValue in attributesValues)
                {
                    AttributesForParsing.Add(attributeValue);

                    Console.WriteLine($"Element TagName: {clickedElement.TagName}, AttributeName: {AttributeName}, AttributeValue: {attributeValue}");
                    Console.WriteLine($"Element Text: {elementText}");

                    PerformActions(elementText, i);
                    i++;
                }
            }
        }

        private void PerformActions(string fileName, int attributeNum)
        {
            try
            {
                fileName = FilesDownloader.GetSafeFileName(fileName);
                string attributeValue = AttributesForParsing[attributeNum];

                if (!string.IsNullOrEmpty(attributeValue) && attributeValue != "#")
                {
                    switch (ActionType)
                    {
                        case ParserActionType.None:
                            Console.WriteLine("ParserActionType: None");
                            break;

                        case ParserActionType.DownloadFile:
                            FilesDownloader.DownloadFile(attributeValue, fileName);
                            break;

                        case ParserActionType.DownloadAttributes:
                            SaveAttributesToFile(attributeValue, fileName);
                            break;

                        case ParserActionType.DownloadIdFromURL:
                            ExtractAndSaveIdFromUrl(attributeValue, fileName);
                            break;
                    }
                }
            }

            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void SaveAttributesToFile(string attribute, string fileName)
        {
            string attributesFolderPath = Path.Combine(DownloadFolder, "Attributes");

            Directory.CreateDirectory(attributesFolderPath);
            File.AppendAllText(Path.Combine(attributesFolderPath, fileName + " (Attribute).txt"), attribute + "\n");
        }

        private void ExtractAndSaveIdFromUrl(string url, string fileName)
        {
            string idsFolderPath = Path.Combine(DownloadFolder, "Attributes", "ID");

            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Url не может быть пустым", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Находим индекс начала "Id="
            int startIndex = url.IndexOf("Id=", StringComparison.OrdinalIgnoreCase);
            if (startIndex == -1)
            {
                startIndex = url.IndexOf("Id?=", StringComparison.OrdinalIgnoreCase);
                if (startIndex == -1)
                {
                    MessageBox.Show("Url не содержит \"Id=\" или \"Id?=\"", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Двигаемся к началу реального значения ID, пропуская "Id="
            startIndex += 3;

            // Находим индекс конца значения ID, предполагая, что значение ID оканчивается на '&' или в конце строки
            int endIndex = url.IndexOf('&', startIndex);
            if (endIndex == -1)
            {
                endIndex = url.Length;
            }

            // Извлекаем подстроку, содержащую ID
            string id = url.Substring(startIndex, endIndex - startIndex);

            Directory.CreateDirectory(idsFolderPath);
            File.AppendAllText(Path.Combine(idsFolderPath, fileName + " (ID).txt"), id + "\n");
        }
    }
}
