using CarParser.Parser;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Selenium.WebDriver.EventCapture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EverythingParser
{
    public partial class Form1 : Form
    {
        public HashSet<string> attributesList = new HashSet<string>();

        private WebDriverManager WebDriverManager;
        private FilesDownloader FilesDownloader;
        private EventCapturingWebDriver CaptureDriver;
        private WebDriverWait DriverWait;
        private string DownloadFilesFolder;
        private string SiteUrl = "https://www.workshopdata.com/";

        private List<AttributeActions> DownloadAttributeActions = new List<AttributeActions>();
        private List<AttributeActions> OpenUrlAttributeActions = new List<AttributeActions>();
        private List<AttributeActions> ExportIdAttributeActions = new List<AttributeActions>();
        private List<AttributeActions> ExportAttributeActions = new List<AttributeActions>();

        public Form1()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            WebDriverManager = new WebDriverManager(this);
            FilesDownloader = new FilesDownloader("EverethingParser", WebDriverManager.DownloadFilesFolderPath, "Files");
            CaptureDriver = WebDriverManager.CaptureDriver;
            DriverWait = WebDriverManager.DriverWait;
            DownloadFilesFolder = WebDriverManager.DownloadFilesFolderPath;

            CaptureDriver.Navigate().GoToUrl(SiteUrl);
            HandleDemoButton();

            WebDriverManager.SetInitialLocalStorageValue();
            WebDriverManager.InjectJavaScriptForNavigationHandling();

            InitializeEventHandling();
        }

        private void HandleDemoButton()
        {
            if (CaptureDriver.FindElements(By.XPath("//input[@value='Демо']")).Count > 0)
            {
                IWebElement demoButton = CaptureDriver.FindElement(By.XPath("//input[@value='Демо']"));
                demoButton.Click();
            }
        }

        private void InitializeEventHandling()
        {
            CaptureDriver.ElementClickCaptured += Driver_ElementClickCaptured;
        }

        private void Driver_ElementClickCaptured(object sender, WebElementCapturedMouseEventArgs e)
        {
            AddAttributesToFlowLayoutPanel(txtAttributes.Text);
            UpdateXPathTextBox(e.Element);
        }

        private void AddAttributesToFlowLayoutPanel(string attributesToParsing)
        {
            if (!string.IsNullOrEmpty(attributesToParsing))
            {
                var attributesArray = attributesToParsing.Split(',');
                foreach (var attribute in attributesArray)
                {
                    AddControlToFlowLayoutPanel(attribute);
                }
            }
        }

        private void AddControlToFlowLayoutPanel(string attribute)
        {
            if (flowLayoutPanel1.InvokeRequired)
            {
                flowLayoutPanel1.Invoke(new MethodInvoker(() => flowLayoutPanel1.Controls.Add(new ucAction(attribute))));
            }
            else
            {
                flowLayoutPanel1.Controls.Add(new ucAction(attribute));
            }
        }

        private void UpdateXPathTextBox(IWebElement element)
        {
            if (txtXPath.InvokeRequired)
            {
                txtXPath.Invoke(new MethodInvoker(() => txtXPath.Text = WebDriverManager.GetFullXPath(element)));
            }
            else
            {
                txtXPath.Text = WebDriverManager.GetFullXPath(element);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            WebDriverManager.QuitDrivers();
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            string XPath = txtXPath.Text;
            string[] parts = XPath.Split('/');
            int lastNumberIndex = WebDriverManager.FindLastNumberIndex(parts);
            if (lastNumberIndex > 0)
            {
                ProcessElements(parts, lastNumberIndex, XPath);
            }
            else
            {
                Console.WriteLine("Элемент с числом не найден.");
            }
        }

        private void ProcessElements(string[] parts, int lastNumberIndex, string elementXPath)
        {
            string clickedElementNameTag = parts[lastNumberIndex].Split('[')[0];
            string clickedParentElementXpath = string.Join("/", parts.Take(lastNumberIndex));
            var clickedParentElement = CaptureDriver.FindElement(By.XPath(clickedParentElementXpath));
            var clickedElements = clickedParentElement.FindElements(By.TagName(clickedElementNameTag)).Distinct();

            HashSet<string> elementsForParsing = new HashSet<string>();

            foreach (var clickedElement in clickedElements)
            {
                string attributes = "";
                string elementText = WebDriverManager.GetElementText(clickedElement);

                foreach (var attributeList in WebDriverManager.GetAllAttributes(clickedElement).Where(attributeList => !elementsForParsing.Contains(attributeList.Key)))
                {
                    elementsForParsing.Add(attributeList.Key);
                }

                Console.WriteLine($"{clickedElement.TagName} Attributes: {attributes}");
                Console.WriteLine($"Element Text: {elementText}");
            }

            PerformActions(elementXPath);

            foreach (var downloadAttribute in DownloadAttributeActions)
            {
                downloadAttribute.StartAction();
            }
            foreach (var exportIdAttribute in ExportIdAttributeActions)
            {
                exportIdAttribute.StartAction();
            }
            foreach (var exportAttribute in ExportAttributeActions)
            {
                exportAttribute.StartAction();
            }
            foreach (var openUrlAttribute in OpenUrlAttributeActions)
            {
                openUrlAttribute.StartAction();
            }
        }

        private void PerformActions(string elementXPath)
        {
            try
            {
                foreach (ucAction actionControl in flowLayoutPanel1.Controls)
                {
                    string attributeName = actionControl.lbAttribute.Text;
                    string selectedAction = actionControl.cbActions.SelectedItem.ToString();

                    switch (selectedAction)
                    {
                        case "Скачать файл по ссылке из атрибута":
                            AttributeActions downloadAttributeAction = new AttributeActions(CaptureDriver, DriverWait, WebDriverManager, FilesDownloader, DownloadFilesFolder, CaptureDriver.Url, elementXPath, attributeName, ParserActionType.DownloadFile);
                            DownloadAttributeActions.Add(downloadAttributeAction);
                            break;

                        case "Запомнить и потом перейти по ссылке из атрибута":
                            AttributeActions openUrlAttributeAction = new AttributeActions(CaptureDriver, DriverWait, WebDriverManager, FilesDownloader, DownloadFilesFolder, CaptureDriver.Url, elementXPath, attributeName, ParserActionType.None);
                            OpenUrlAttributeActions.Add(openUrlAttributeAction);
                            break;

                        case "Экспортировать Id из ссылки":
                            AttributeActions exportIdAttributeAction = new AttributeActions(CaptureDriver, DriverWait, WebDriverManager, FilesDownloader, DownloadFilesFolder, CaptureDriver.Url, elementXPath, attributeName, ParserActionType.DownloadIdFromURL);
                            ExportIdAttributeActions.Add(exportIdAttributeAction);
                            break;

                        case "Экспортировать текст атрибута":
                            AttributeActions exportAttributeAction = new AttributeActions(CaptureDriver, DriverWait, WebDriverManager, FilesDownloader, DownloadFilesFolder, CaptureDriver.Url, elementXPath, attributeName, ParserActionType.DownloadAttributes);
                            ExportAttributeActions.Add(exportAttributeAction);
                            break;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void chbGoToUrl_CheckedChanged(object sender, EventArgs e)
        {
            WebDriverManager.EditBlockNavigation(!chbGoToUrl.Checked);
        }
    }
}