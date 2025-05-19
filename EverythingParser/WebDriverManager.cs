using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
    public class WebDriverManager
    {
        public Form1 MainForm { get; private set; }
        public string DownloadFilesFolderPath { get; private set; }
        public IWebDriver Driver { get; private set; }
        public WebDriverWait DriverWait { get; private set; }
        public EventCapturingWebDriver CaptureDriver { get; private set; }
        private bool ShouldCancelNavigation = true;

        public WebDriverManager(Form1 mainForm)
        {
            MainForm = mainForm;
            InitializeWebDriver();
        }

        private void InitializeWebDriver()
        {
            DownloadFilesFolderPath = Path.Combine(Path.GetTempPath(), $"EverethingParser", "Download");

            ChromeOptions options = new ChromeOptions();
            string extensionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChromeExtension.crx");

            options.AddExtension(extensionPath);
            options.AddUserProfilePreference("download.default_directory", DownloadFilesFolderPath);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            Driver = new ChromeDriver(service, options);
            DriverWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
            CaptureDriver = new EventCapturingWebDriver(Driver);
        }

        public void SetInitialLocalStorageValue()
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("localStorage.setItem('shouldCancelNavigation', arguments[0]);", ShouldCancelNavigation.ToString().ToLower());
        }

        public void InjectJavaScriptForNavigationHandling()
        {
            string script = @"
            function checkNavigation() {
                var shouldCancelNavigation = localStorage.getItem('shouldCancelNavigation') === 'true';
                var links = document.getElementsByTagName('a');
                for (var i = 0; i < links.length; i++) {
                    links[i].removeEventListener('click', clickHandler);
                    links[i].addEventListener('click', clickHandler);
                }
            }
            function clickHandler(event) {
                var shouldCancelNavigation = localStorage.getItem('shouldCancelNavigation') === 'true';
                if (shouldCancelNavigation) {
                    event.preventDefault();
                    console.log('Переход по ссылке отменен.');
                } else {
                    console.log('Переход по ссылке разрешен.');
                }
            }
            setInterval(checkNavigation, 1000);"; // Проверка каждую секунду
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript(script);
        }

        public void EditBlockNavigation(bool _shouldCancelNavigation)
        {
            ShouldCancelNavigation = _shouldCancelNavigation;
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("localStorage.setItem('shouldCancelNavigation', arguments[0]);", ShouldCancelNavigation.ToString().ToLower());
        }

        public void QuitDrivers()
        {
            CaptureDriver.Quit();
            Driver.Quit();
        }

        public int FindLastNumberIndex(string[] parts)
        {
            int lastNumberIndex = -1;
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Contains("[") && parts[i].Contains("]"))
                {
                    lastNumberIndex = i;
                }
            }
            return lastNumberIndex;
        }

        public string GetFullXPath(IWebElement element)
        {
            if (element == null) return string.Empty;
            string tagName = element.TagName;
            string path = tagName;
            if (tagName.Equals("html")) return path;
            IWebElement parent = element.FindElement(By.XPath(".."));
            List<IWebElement> siblings = parent.FindElements(By.XPath(tagName)).ToList();
            if (siblings.Count > 1)
            {
                int index = siblings.IndexOf(element) + 1;
                path += $"[{index}]";
            }
            return $"{GetFullXPath(parent)}/{path}";
        }

        public string GetElementText(IWebElement element)
        {
            string elementText = GetDirectText(element);
            IWebElement clickedElementParent = element;
            while (string.IsNullOrEmpty(elementText))
            {
                var childElements = clickedElementParent.FindElements(By.XPath("./*"));
                if (childElements.Count == 0)
                {
                    break;
                }
                foreach (var childElement in childElements)
                {
                    elementText = GetDirectText(childElement);
                    if (!string.IsNullOrEmpty(elementText))
                    {
                        break;
                    }
                }
                clickedElementParent = clickedElementParent.FindElement(By.XPath(".."));
            }
            return elementText;
        }

        private string GetDirectText(IWebElement element)
        {
            try
            {


                var directText = ((IJavaScriptExecutor)Driver).ExecuteScript(
                    @"var text = '';
          for (var i = 0; i < arguments[0].childNodes.length; i++) {
              if (arguments[0].childNodes[i].nodeType === Node.TEXT_NODE) {
                  text += arguments[0].childNodes[i].nodeValue.trim();
              }
          }
          return text;",
                    element);
                return directText.ToString().Trim();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return string.Empty; }
        }

        #region GetAllAttributes
        public Dictionary<string, List<string>> GetAllAttributes(IWebElement element)
        {
            Dictionary<string, List<string>> collectedAttributes = new Dictionary<string, List<string>>();
            CollectAttributes(element, collectedAttributes);
            return collectedAttributes;
        }

        private void CollectAttributes(IWebElement elem, Dictionary<string, List<string>> collectedAttributes)
        {
            var elemAttributes = GetElementAttributes(elem, MainForm.attributesList);
            foreach (var kvp in elemAttributes)
            {
                if (!collectedAttributes.ContainsKey(kvp.Key))
                {
                    collectedAttributes[kvp.Key] = new List<string>();
                }
                collectedAttributes[kvp.Key].Add(kvp.Value);
            }
            foreach (IWebElement child in elem.FindElements(By.XPath("./*")))
            {
                CollectAttributes(child, collectedAttributes);
            }
        }

        private Dictionary<string, string> GetElementAttributes(IWebElement element, HashSet<string> attributesToCollect)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            string outerHtml = element.GetAttribute("outerHTML");
            string innerHtml = element.GetAttribute("innerHTML");
            string tagWithAttributes = outerHtml;
            if (!string.IsNullOrEmpty(innerHtml))
            {
                tagWithAttributes = outerHtml.Replace(innerHtml, "").Trim();
            }
            int startTagIndex = tagWithAttributes.IndexOf('<') + 1;
            int endTagIndex = tagWithAttributes.IndexOf('>');
            if (startTagIndex >= 0 && endTagIndex > startTagIndex)
            {
                string attributesString = tagWithAttributes.Substring(startTagIndex, endTagIndex - startTagIndex).Trim();
                string[] attributePairs = attributesString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string pair in attributePairs)
                {
                    int equalIndex = pair.IndexOf('=');
                    if (equalIndex > 0)
                    {
                        string attributeName = pair.Substring(0, equalIndex).Trim();
                        string attributeValue = pair.Substring(equalIndex + 1).Trim('\'', '"').Trim();
                        if (attributesToCollect.Contains(attributeName) && attributeName != "style")
                        {
                            attributes[attributeName] = attributeValue;
                        }
                    }
                }
            }
            return attributes;
        }
        #endregion

        #region GetAllValuesOfAttribute
        public List<string> GetAllAttributeValues(IWebElement element, string attributeName)
        {
            List<string> attributeValues = new List<string>();
            CollectAttributeValues(element, attributeName, attributeValues);
            return attributeValues;
        }

        private void CollectAttributeValues(IWebElement elem, string attributeName, List<string> attributeValues)
        {
            string attributeValue = elem.GetAttribute(attributeName);
            if (!string.IsNullOrEmpty(attributeValue))
            {
                attributeValues.Add(attributeValue);
            }

            foreach (IWebElement child in elem.FindElements(By.XPath("./*")))
            {
                CollectAttributeValues(child, attributeName, attributeValues);
            }
        }
        #endregion
    }
}
