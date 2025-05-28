using CarParser.Parser;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Presentation;
using EverythingParser.ParserActions;
using EverythingParser.ParserConfigurationScripts;
using EverythingParser.ParserManagers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V134.Accessibility;
using OpenQA.Selenium.DevTools.V134.Network;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;
using Selenium.WebDriver.EventCapture;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using Control = System.Windows.Forms.Control;

namespace EverythingParser
{
    public partial class Form1 : Form
    {
        private AttributesManager AttributesManager;
        private ParsingConfigurationManager ParsingConfigurationManager;
        private ControlsManager ControlsManager;

        private WebDriverManager WebDriverManager;
        private FilesDownloader FilesDownloader;
        private EventCapturingWebDriver CaptureDriver;
        private WebDriverWait DriverWait;

        private string SiteUrl = "https://www.workshopdata.com/";
        private string CurrentXPath = "";
        private string LastUrl { get; set; }

        private IWebDriver Driver;
        public static List<ParsingPageConfiguration> StaticParsingPageConfigurations { get; set; }
        private List<ParsingPageConfiguration> ParsingPageConfigurations = new List<ParsingPageConfiguration>();

        private string overlayId = "anti-clickable-layer"; // Уникальный идентификатор для слоя

        public Form1()
        {
            InitializeComponent();
            Initialize();

            LastUrl = Driver.Url;

            AppDomain.CurrentDomain.ProcessExit += QuitDrivers;
        }

        private void Initialize()
        {
            WebDriverManager = new WebDriverManager();
            FilesDownloader = new FilesDownloader("EverethingParser", WebDriverManager.DownloadFilesFolderPath, "Files");

            Driver = WebDriverManager.Driver;
            CaptureDriver = WebDriverManager.CaptureDriver;
            DriverWait = WebDriverManager.DriverWait;

            ControlsManager = new ControlsManager(WebDriverManager, chbListBox, flowLayoutPanel1, CaptureDriver);
            AttributesManager = new AttributesManager(CaptureDriver, chbListBox, flowLayoutPanel1, DriverWait);

            chbListBox.ItemCheck += ControlsManager.HandleItemCheck;

            ParsingConfigurationManager = new ParsingConfigurationManager(CaptureDriver, WebDriverManager, DriverWait, FilesDownloader, ControlsManager, AttributesManager, chbListBox, flowLayoutPanel1);

            CaptureDriver.Navigate().GoToUrl(SiteUrl);
            WebDriverManager.ClickDemoButton();

            if (lbPageLink.InvokeRequired)
            {
                lbPageLink.Invoke(new MethodInvoker(() => lbPageLink.Text = $"{CaptureDriver.Title}"));
            }
            else
            {
                lbPageLink.Text = $"{CaptureDriver.Title}";
            }

            WebDriverManager.SetInitialLocalStorageValue();
            WebDriverActions.InjectJavaScriptForNavigationHandling(CaptureDriver);

            InitializeEventHandling();
        }

        private void InitializeEventHandling()
        {
            CaptureDriver.ElementClickCaptured += Driver_ElementClickCaptured;
        }



        private void Driver_ElementClickCaptured(object sender, WebElementCapturedMouseEventArgs e)
        {
            try
            {
                //// Добавляем прозрачный слой поверх всей страницы
                //string addOverlayScript = $@"
                //var overlay = document.createElement('div');
                //overlay.id = '{overlayId}';
                //overlay.style.position = 'fixed';
                //overlay.style.top = '0';
                //overlay.style.left = '0';
                //overlay.style.width = '100%';
                //overlay.style.height = '100%';
                //overlay.style.backgroundColor = 'rgba(0, 0, 0, 0.5)';
                //overlay.style.zIndex = '9999';
                //document.body.appendChild(overlay);
                //";
                //((IJavaScriptExecutor)CaptureDriver).ExecuteScript(addOverlayScript);

                SaveElementActions();

                UpdateControlText(lbPageLink, CaptureDriver.Title);
                UpdateControlEnabled(chbListBox, false);
                UpdateControlVisible(lbLoading, true);

                CurrentXPath = WebDriverActions.GetFullXPath(e.Element);

                // Relocate the element to avoid StaleElementReferenceException
                IWebElement element = Driver.FindElement(By.XPath(CurrentXPath));

                AttributesManager.AddAttributesToChbListBox(element);

                ParsingPageConfiguration existingPageConfig = ParsingPageConfigurations.FirstOrDefault(pageConfig => pageConfig.PageUrl == WebDriverActions.GetBaseUrl(CaptureDriver.Url));

                if (existingPageConfig != null)
                {
                    ParsingElementConfiguration existingElementConfig = existingPageConfig.ParsingElements.FirstOrDefault(elementConfig => elementConfig.ElementXPath == CurrentXPath);

                    if (existingElementConfig != null)
                    {
                        ParsingConfigurationManager.GetExistingSettings(existingElementConfig);
                    }
                }
            }
            catch (StaleElementReferenceException ex)
            {
                Console.WriteLine($"StaleElementReferenceException: {ex.Message}");
                MessageBox.Show("Элемент устарел, попробуйте снова", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"ObjectDisposedException: {ex.Message}");
                MessageBox.Show("Объект уже был удален, попробуйте снова", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show("Произошла ошибка, элементы могли быть не загружены до конца", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Console.WriteLine("Парсинг успешен!");

                //string removeOverlayScript = $@"
                //var overlay = document.getElementById('{overlayId}');
                //if (overlay) {{
                //    overlay.parentNode.removeChild(overlay);
                //}}
                //";
                //((IJavaScriptExecutor)CaptureDriver).ExecuteScript(removeOverlayScript);

                Console.WriteLine("Удаление защитного слоя успешно!");

                UpdateControlEnabled(chbListBox, true);
                UpdateControlVisible(lbLoading, false);

                Console.WriteLine("Обновление UI успешно!");
            }
        }

        private void QuitDrivers(object sender, EventArgs e)
        {
            WebDriverManager.QuitDrivers();
        }

        #region UpdateUI
        private static void UpdateControlText(Control control, string text)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new MethodInvoker(() => control.Text = text));
            }
            else
            {
                control.Text = text;
            }
        }

        private static void UpdateControlEnabled(Control control, bool enabled)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new MethodInvoker(() => control.Enabled = enabled));
            }
            else
            {
                control.Enabled = enabled;
            }
        }

        private static void UpdateControlVisible(Control control, bool visible)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new MethodInvoker(() => control.Visible = visible));
            }
            else
            {
                control.Visible = visible;
            }
        }
        #endregion

        #region FormElements
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            QuitDrivers(sender, e);
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Запуск парсинга");

            WebDriverManager.EditBlockNavigation(false);

            StaticParsingPageConfigurations = ParsingPageConfigurations;

            btStart.Enabled = false;

            SaveElementActions();

            var page = ParsingPageConfigurations.FirstOrDefault(p => p.PageUrl == WebDriverActions.GetBaseUrl(txtStartPage.Text));

            if (page != null)
            {
                Console.WriteLine("Запуск парсинга, cтатус: Успех");
                page.StartParsing(false, txtStartPage.Text);
            }
            else
            {
                // Обработка случая, когда страница не найдена
                Console.WriteLine("Запуск парсинга, cтатус: Неудача");
                MessageBox.Show("Конфигурация для страницы не найдена", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            WebDriverManager.EditBlockNavigation(!chbGoToUrl.Checked);

            btStart.Enabled = true;
        }

        private void chbGoToUrl_CheckedChanged(object sender, EventArgs e)
        {
            WebDriverManager.EditBlockNavigation(!chbGoToUrl.Checked);
        }
        #endregion

        private void SaveElementActions()
        {
            List<ucAction> ucsList = flowLayoutPanel1.Controls.OfType<ucAction>().ToList();

            ParsingConfigurationManager.SaveElementActions(CurrentXPath, ref ParsingPageConfigurations, ucsList);
        }

        public static List<ParsingPageConfiguration> GetParsingPageConfigurations()
        {
            return StaticParsingPageConfigurations;
        }
    }
}