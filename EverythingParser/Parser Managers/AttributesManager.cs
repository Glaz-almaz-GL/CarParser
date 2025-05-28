using CarParser.Parser;
using DocumentFormat.OpenXml.Presentation;
using EverythingParser;
using EverythingParser.ParserActions;
using EverythingParser.ParserConfigurationScripts;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.WebDriver.EventCapture;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EverythingParser
{
    public class AttributesManager
    {
        public CheckedListBox CheckedListBox { get; private set; }
        public FlowLayoutPanel FlowLayoutPanel { get; private set; }
        public WebDriverWait DriverWait { get; private set; }
        public EventCapturingWebDriver CaptureDriver { get; private set; }

        private List<string> AttributesForParsing = new List<string>();

        private readonly string DownloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Parser (EverethingParser)", "Files");

        public AttributesManager(EventCapturingWebDriver captureDriver, CheckedListBox checkedListBox, FlowLayoutPanel flowLayoutPanel, WebDriverWait driverWait)
        {
            CaptureDriver = captureDriver;
            CheckedListBox = checkedListBox;
            FlowLayoutPanel = flowLayoutPanel;
            DriverWait = driverWait;
        }

        #region ChbListBox
        public void AddAttributesToChbListBox(IWebElement webElement)
        {
            // Очистим предыдущие атрибуты
            HashSet<string> attributesList = new HashSet<string>();

            if (FlowLayoutPanel.InvokeRequired)
            {
                FlowLayoutPanel.Invoke(new MethodInvoker(() => FlowLayoutPanel.Controls.Clear()));
            }
            else
            {
                FlowLayoutPanel.Controls.Clear();
            }

            // Проверяем, является ли элемент ячейкой таблицы
            if (webElement != null && (webElement.TagName == "td" || webElement.TagName == "th"))
            {
                // Если это ячейка таблицы, получаем родительскую строку
                webElement = webElement.FindElement(By.XPath("./ancestor::tr"));
            }

            // Получаем полный XPath элемента
            string fullXPath = WebDriverActions.GetFullXPath(webElement);
            string[] parts = fullXPath.Split('/');

            // Найдем первый с конца элемент с числом
            int lastNumberIndex = -1;
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                if (parts[i].Contains("["))
                {
                    lastNumberIndex = i;
                    break;
                }
            }

            if (lastNumberIndex > 0)
            {
                // Получаем XPath нужного элемента
                string targetElementXPath = string.Join("/", parts.Take(lastNumberIndex + 1));

                AddTargetAttributesToChb(ref attributesList, targetElementXPath);
            }

            AddAttributesToChb(attributesList);
        }


        private void AddAttributesToChb(HashSet<string> attributesList)
        {

            // Добавляем все атрибуты в CheckedListBox
            if (CheckedListBox.InvokeRequired)
            {
                CheckedListBox.Invoke(new MethodInvoker(() =>
                {
                    CheckedListBox.Items.Clear();
                    foreach (var attribute in attributesList)
                    {
                        CheckedListBox.Items.Add(attribute);
                    }
                }));
            }
            else
            {
                CheckedListBox.Items.Clear();
                foreach (var attribute in attributesList)
                {
                    CheckedListBox.Items.Add(attribute);
                }
            }
        }

        private void AddTargetAttributesToChb(ref HashSet<string> attributesList, string targetElementXPath)
        {
            // Находим целевой элемент
            IWebElement targetElement = CaptureDriver.FindElement(By.XPath(targetElementXPath));

            if (targetElement != null)
            {

                // Добавляем атрибуты дочерних элементов целевого элемента
                var childElements = targetElement.FindElements(By.XPath(".//*"));

                // Получаем текст целевого элемента
                string targetElementText = WebDriverActions.GetElementText(targetElement);
                attributesList.Add($"{targetElementXPath} - Text: {targetElementText}");

                // Добавляем атрибуты целевого элемента
                var targetAttributes = GetAllAttributes(targetElement);
                foreach (var attribute in targetAttributes)
                {
                    if (attribute.Key != "style")
                    {
                        attributesList.Add($"{targetElementXPath} - {attribute.Key}: {attribute.Value}");
                    }
                }

                AddChildAttributesToChb(childElements, attributesList);
            }
        }

        private void AddChildAttributesToChb(ReadOnlyCollection<IWebElement> childElements, HashSet<string> attributesList)
        {
            foreach (var childElement in childElements)
            {
                if (childElement != null)
                {
                    string childXPath = WebDriverActions.GetFullXPath(childElement);
                    string childElementText = WebDriverActions.GetElementText(childElement);

                    if (!string.IsNullOrEmpty(childElementText))
                    {
                        attributesList.Add($"{childXPath} - Text: {childElementText}");
                    }

                    var childAttributes = GetAllAttributes(childElement);
                    foreach (var attribute in childAttributes)
                    {
                        if (attribute.Key != "style")
                        {
                            attributesList.Add($"{childXPath} - {attribute.Key}: {attribute.Value}");
                        }
                    }
                }
            }
        }


        public Dictionary<string, string> GetAllAttributes(IWebElement element)
        {
            var attributeDictionary = new Dictionary<string, string>();

            var jsExecutor = (IJavaScriptExecutor)CaptureDriver;
            var attributes = jsExecutor.ExecuteScript("var items = {}; for (index = 0; index < arguments[0].attributes.length; ++index) { items[arguments[0].attributes[index].name] = arguments[0].attributes[index].value }; return items;", element);

            if (attributes != null)
            {
                var attributeDict = (Dictionary<string, object>)attributes;
                foreach (var attr in attributeDict)
                {
                    attributeDictionary[attr.Key] = attr.Value.ToString();
                }
            }

            return attributeDictionary;
        }
        #endregion

        #region PerformActions
        public void ProcessElements(string[] Parts, int LastNumberIndex, ParserActionType ActionType, string AttributeName, string ElementXPath)
        {
            AttributesForParsing = new List<string>();

            if (ActionType != ParserActionType.None && ActionType != ParserActionType.ClickOnElement)
            {
                string clickedElementNameTag = Parts[LastNumberIndex].Split('[')[0];

                string clickedParentElementXpath = string.Join("/", Parts.Take(LastNumberIndex));

                try
                {
                    var clickedParentElement = CaptureDriver.FindElement(By.XPath(clickedParentElementXpath));

                    DriverWait.Until(d => d.FindElements(By.TagName(clickedElementNameTag)).Distinct().Any());

                    var clickedElements = clickedParentElement.FindElements(By.TagName(clickedElementNameTag)).Distinct();

                    Console.WriteLine($"Обнаружено элементов: {clickedElements.Count()}");

                    int i = 0;
                    foreach (var clickedElement in clickedElements)
                    {
                        Console.WriteLine($"Element TagName: {clickedElement.TagName}, Element Text: {clickedElement.Text}");

                        if (clickedElement.TagName.Equals("td", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Елемент является ячейкой таблицы");

                            // Если элемент - это столбец таблицы, найти все строки таблицы
                            var tableRow = clickedParentElement.FindElement(By.XPath(".."));
                            Console.WriteLine(tableRow);
                            var table = tableRow.FindElement(By.XPath(".."));
                            Console.WriteLine(table);
                            var tableRows = table.FindElements(By.TagName("tr"));

                            foreach (var row in tableRows)
                            {
                                var cells = row.FindElements(By.TagName("td"));
                                foreach (var cell in cells)
                                {
                                    ExportAttributes(clickedElement, ActionType, AttributeName, i);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Елемент не является ячейкой таблицы");

                            ExportAttributes(clickedElement, ActionType, AttributeName, i);
                        }
                    }
                }
                catch (WebDriverException ex)
                {
                    MessageBox.Show($"WebDriverException при обработке элементов: {ex.Message}\nStackTrace: {ex.StackTrace}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (ActionType == ParserActionType.ClickOnElement)
            {
                try
                {
                    DriverWait.Until(d => d.FindElement(By.XPath(ElementXPath)));

                    string clickedElementNameTag = Parts[LastNumberIndex].Split('[')[0];

                    string clickedParentElementXpath = string.Join("/", Parts.Take(LastNumberIndex));

                    var clickedParentElement = CaptureDriver.FindElement(By.XPath(clickedParentElementXpath));

                    DriverWait.Until(d => d.FindElements(By.TagName(clickedElementNameTag)).Distinct().Any());

                    var clickedElements = clickedParentElement.FindElements(By.TagName(clickedElementNameTag)).Distinct();

                    DriverWait.Until(d => d.FindElement(By.XPath(ElementXPath)));

                    foreach (var element in clickedElements)
                    {
                        ClickOnElement(element);
                    }
                }
                catch (WebDriverException ex)
                {
                    MessageBox.Show($"Ошибка при клике на элемент: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportAttributes(IWebElement clickedElement, ParserActionType ActionType, string AttributeName, int i)
        {
            string elementText = WebDriverActions.GetElementText(clickedElement);
            List<string> attributesValues = WebDriverActions.GetAllAttributeValues(clickedElement, AttributeName);

            Console.WriteLine($"Обнаружено значений атрибута: {attributesValues.Count}");

            if (attributesValues.Count > 0)
            {
                foreach (string attributeValue in attributesValues)
                {
                    AttributesForParsing.Add(attributeValue);
                    Console.WriteLine($"Element TagName: {clickedElement.TagName}, AttributeName: {AttributeName}, AttributeValue: {attributeValue}");
                    Console.WriteLine($"Element Text: {elementText}");

                    PerformActions(ActionType, elementText, i);
                    i++;
                }
            }
        }

        private void PerformActions(ParserActionType ActionType, string elementText, int attributeNum)
        {
            try
            {
                elementText = FilesDownloader.GetSafeFileName(elementText);
                string attributeValue = AttributesForParsing[attributeNum];

                if (!string.IsNullOrEmpty(attributeValue) && attributeValue != "#")
                {
                    switch (ActionType)
                    {
                        case ParserActionType.None:
                            Console.WriteLine("ParserActionType: None");
                            break;

                        case ParserActionType.DownloadFile:
                            Console.WriteLine("ParserActionType: DownloadFile");
                            FilesDownloader.DownloadFile(attributeValue, elementText);
                            break;

                        case ParserActionType.DownloadAttributes:
                            Console.WriteLine("ParserActionType: DownloadAttribute");
                            ExtractAndSaveAttributesToFile(elementText, attributeValue);
                            break;

                        case ParserActionType.DownloadIdFromURL:
                            Console.WriteLine("ParserActionType: DownloadId");
                            ExtractAndSaveIdFromUrl(attributeValue, elementText);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении действий: {ex.Message}\nStackTrace: {ex.StackTrace}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClickOnElement(IWebElement clickedElement)
        {
            try
            {
                if (clickedElement != null && clickedElement.Displayed && clickedElement.Enabled)
                {
                    try
                    {
                        string oldUrl = WebDriverActions.GetBaseUrl(CaptureDriver.Url);

                        clickedElement.Click();

                        if (oldUrl != WebDriverActions.GetBaseUrl(CaptureDriver.Url))
                        {
                            ParsingPageConfiguration page = Form1.GetParsingPageConfigurations().FirstOrDefault(p => p.PageUrl == WebDriverActions.GetBaseUrl(CaptureDriver.Url));

                            page?.StartParsing(true);
                        }
                    }
                    catch (ElementNotInteractableException) { Console.WriteLine($"Element {clickedElement.TagName} not interactable"); }
                }
                else if (clickedElement == null)
                {
                    MessageBox.Show("Элемент не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private void ExtractAndSaveAttributesToFile(string attribute, string fileName)
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
        #endregion
    }
}