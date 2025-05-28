using CarParser.Parser;
using DocumentFormat.OpenXml.Bibliography;
using EverythingParser.ParserActions;
using EverythingParser.ParserConfigurationScripts;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Support.UI;
using Selenium.WebDriver.EventCapture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EverythingParser
{
    public enum ParserActionType
    {
        None,
        DownloadFile,
        DownloadAttributes,
        DownloadIdFromURL,
        ClickOnElement
    }

    public class AttributeActions
    {
        public EventCapturingWebDriver CaptureDriver { get; private set; }
        public WebDriverWait DriverWait { get; private set; }
        public WebDriverManager WebDriverManager { get; private set; }
        public FilesDownloader FilesDownloader { get; private set; }
        public AttributesManager AttributesManager { get; private set; }
        public string PageUrl { get; private set; }
        public string ElementXPath { get; private set; }
        public string AttributeName { get; private set; }
        public ParserActionType ActionType { get; private set; }

        public AttributeActions(EventCapturingWebDriver captureDriver, WebDriverWait driverWait, WebDriverManager webDriverManager, FilesDownloader filesDownloader, AttributesManager attributesManager, string pageUrl, string downloadFolder, string elementXPath, string attributeName, ParserActionType actionType)
        {
            CaptureDriver = captureDriver;
            DriverWait = driverWait;
            WebDriverManager = webDriverManager;
            FilesDownloader = filesDownloader;
            AttributesManager = attributesManager;
            PageUrl = pageUrl;
            ElementXPath = elementXPath;
            AttributeName = attributeName;
            ActionType = actionType;
        }

        public void StartAction()
        {
            if (WebDriverActions.GetBaseUrl(CaptureDriver.Url) != PageUrl)
            {
                CaptureDriver.Navigate().GoToUrl(PageUrl);
            }

            try
            {
                string[] parts = ElementXPath.Split('/');
                int lastNumberIndex = WebDriverActions.FindLastNumberIndex(parts);

                if (lastNumberIndex > 0)
                {
                    IWebElement element = CaptureDriver.FindElement(By.XPath(ElementXPath));
                    if (element.TagName == "td")
                    {
                        // Если элемент - ячейка таблицы, ищем второй с конца элемент с цифрой
                        int secondLastNumberIndex = WebDriverActions.FindSecondLastNumberIndex(parts);
                        if (secondLastNumberIndex > 0)
                        {
                            AttributesManager.ProcessElements(parts, secondLastNumberIndex, ActionType, AttributeName, ElementXPath);
                        }
                        else
                        {
                            Console.WriteLine("Второй элемент с числом не найден.");
                        }
                    }
                    else
                    {
                        AttributesManager.ProcessElements(parts, lastNumberIndex, ActionType, AttributeName, ElementXPath);
                    }
                }
                else
                {
                    Console.WriteLine("Элемент с числом не найден.");
                }
            }
            catch (WebDriverException ex)
            {
                MessageBox.Show($"WebDriverException: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении действия: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}