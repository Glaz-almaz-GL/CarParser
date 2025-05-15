using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Selenium.WebDriver.EventCapture;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace EverythingParser
{
    public partial class Form1 : Form
    {
        bool shouldCancelNavigation = true;
        string CacheFolderPath;
        IWebDriver Driver;
        WebDriverWait DriverWait;

        EventCapturingWebDriver CaptureDriver;

        string SiteUrl = "https://www.workshopdata.com/  ";

        public Form1()
        {
            InitializeComponent();
            CreateDriver();

            CaptureDriver = new EventCapturingWebDriver(Driver);

            CaptureDriver.Navigate().GoToUrl(SiteUrl);

            if (CaptureDriver.FindElements(By.XPath("//input[@value='Демо']")).Count > 0)
            {
                IWebElement demoButton = CaptureDriver.FindElement(By.XPath("//input[@value='Демо']"));
                demoButton.Click();
            }

            // Добавление обработчика событий на все ссылки
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            string script = @"
            var shouldCancelNavigation = arguments[0];
            var links = document.getElementsByTagName('a');
            for (var i = 0; i < links.length; i++) {
                links[i].addEventListener('click', function(event) {
                    if (shouldCancelNavigation) {
                        event.preventDefault();
                        console.log('Переход по ссылке отменен.');
                    } else {
                        console.log('Переход по ссылке разрешен.');
                    }
                });
            }
        ";
            js.ExecuteScript(script, shouldCancelNavigation);

            CaptureDriver.ElementClickCaptured += Driver_ElementClickCaptured;

            //DriverWait.Until(d => d.FindElements(By.XPath("/html/body/section[1]/section[2]/section/nav/ul/li[9]")).Count > 0);

            //MessageBox.Show(Driver.FindElements(By.ClassName("tile")).Count.ToString());

            //var btns = driver.FindElements(By.ClassName("tile"));
            //List<IWebElement> childElements = new List<IWebElement>();

            //foreach (var btn in btns)
            //{
            //    Console.WriteLine("Name: " + btn.TagName + " " + GetFullXPath(btn));

            //    //childElements = btn.FindElements(By.XPath(".//*")).ToList();
            //    PopulateTreeView(btn);
            //}


            //Driver.Quit();

            //CreateDriver();   
        }

        private void Driver_ElementClickCaptured(object sender, WebElementCapturedMouseEventArgs e)
        {
            Console.WriteLine("Click at ({0}, {1}): <{2} id=\"{3}\">", e.ClientX, e.ClientY, e.Element.TagName, e.Element.GetAttribute("id"));

            PopulateTreeView(e.Element);
        }

        public string GetFullXPath(IWebElement element)
        {
            if (element == null)
                return string.Empty;

            string tagName = element.TagName;
            string path = tagName;

            if (tagName.Equals("html"))
                return path;

            // Получаем все соседние элементы того же типа
            IWebElement parent = element.FindElement(By.XPath(".."));
            List<IWebElement> siblings = parent.FindElements(By.XPath(tagName)).ToList();

            if (siblings.Count > 1)
            {
                int index = siblings.IndexOf(element) + 1;
                path += $"[{index}]";
            }

            return $"{GetFullXPath(parent)}/{path}";
        }

        // Метод для получения всех атрибутов элемента, исключая 'style'
        private void PopulateTreeView(IWebElement parentElement)
        {
            try
            {
                // Получить все дочерние элементы внутри родительского элемента
                List<IWebElement> childElements = parentElement.FindElements(By.XPath("./*")).ToList();

                // Создаем корневой узел для родительского элемента
                TreeNode parentNode = new TreeNode($"Parent: {parentElement.TagName} [id={parentElement.GetAttribute("id")} , class={parentElement.GetAttribute("class")} , Text={GetDirectText(parentElement).Trim()}]");

                // Обновляем TreeView в UI потоке
                if (treeView1.InvokeRequired)
                {
                    treeView1.Invoke(new Action<TreeNode>(AddParentNodeToTreeView), parentNode);
                }
                else
                {
                    AddParentNodeToTreeView(parentNode);
                }

                // Рекурсивно добавляем дочерние элементы
                AddChildNodes(parentNode, childElements);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating TreeView: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddParentNodeToTreeView(TreeNode parentNode)
        {
            treeView1.Nodes.Add(parentNode);
        }

        private void AddChildNodes(TreeNode parentNode, List<IWebElement> childElements)
        {
            foreach (IWebElement child in childElements)
            {
                try
                {
                    string tagName = child.TagName;
                    var attributes = GetAttributes(child);

                    // Формируем строку атрибутов
                    string attributesString = string.Join(" , ", attributes.Select(kv => $"{kv.Key}={kv.Value}"));

                    // Получаем текст элемента, игнорируя текст дочерних элементов
                    string elementText = GetDirectText(child).Trim();
                    string displayText = !string.IsNullOrEmpty(elementText) ? $" - {elementText}" : "";

                    // Создаем узел для текущего элемента
                    TreeNode childNode = new TreeNode($"{tagName} [{attributesString}]{displayText}");

                    if (treeView1.InvokeRequired)
                    {
                        treeView1.Invoke(new Action<TreeNode, TreeNode>((p, c) => p.Nodes.Add(c)), parentNode, childNode);
                    }
                    else
                    {
                        parentNode.Nodes.Add(childNode);
                    }

                    // Рекурсивно добавляем дочерние элементы текущего элемента
                    List<IWebElement> grandChildElements = child.FindElements(By.XPath("./*")).ToList();
                    if (grandChildElements.Count > 0)
                    {
                        AddChildNodes(childNode, grandChildElements);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding child node: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Метод для получения текста непосредственно внутри элемента, игнорируя текст дочерних элементов
        private string GetDirectText(IWebElement element)
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

        // Метод для получения всех атрибутов элемента
        private Dictionary<string, string> GetAttributes(IWebElement element)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();

            // Получаем строку всех атрибутов элемента
            string outerHtml = element.GetAttribute("outerHTML");
            string innerHtml = element.GetAttribute("innerHTML");

            // Удаляем внутреннее HTML, чтобы остались только атрибуты
            string tagWithAttributes = outerHtml;

            // Если innerHtml не пустая, заменяем его на пустую строку
            if (!string.IsNullOrEmpty(innerHtml))
            {
                tagWithAttributes = outerHtml.Replace(innerHtml, "").Trim();
            }

            // Находим начало и конец тега
            int startTagIndex = tagWithAttributes.IndexOf('<') + 1;
            int endTagIndex = tagWithAttributes.IndexOf('>');

            if (startTagIndex >= 0 && endTagIndex > startTagIndex)
            {
                // Извлекаем часть строки с атрибутами
                string attributesString = tagWithAttributes.Substring(startTagIndex, endTagIndex - startTagIndex).Trim();

                // Разделяем атрибуты по пробелам
                string[] attributePairs = attributesString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string pair in attributePairs)
                {
                    // Разделяем имя атрибута и его значение
                    int equalIndex = pair.IndexOf('=');
                    if (equalIndex > 0)
                    {
                        string attributeName = pair.Substring(0, equalIndex).Trim();
                        string attributeValue = pair.Substring(equalIndex + 1).Trim('\'', '"').Trim();

                        // Добавляем атрибут в словарь, если это не атрибут style
                        if (attributeName != "style")
                        {
                            attributes[attributeName] = attributeValue;
                        }
                    }
                }
            }

            return attributes;
        }

        private void CreateDriver()
        {
            CacheFolderPath = Path.Combine(Path.GetTempPath(), $"{Name} Parser", "Download");

            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless"); // Запуск в безголовом режиме
            //options.AddArgument("--disable-gpu"); // Отключает GPU для улучшения производительности в headless режиме
            options.AddUserProfilePreference("download.default_directory", CacheFolderPath);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            // Инициализация драйвера с указанием удаленного URL
            Driver = new ChromeDriver(service, options);
            DriverWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CaptureDriver.Quit();
            Driver.Quit();
        }
    }
}