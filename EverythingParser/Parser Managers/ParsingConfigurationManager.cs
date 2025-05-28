using CarParser.Parser;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Vml;
using EverythingParser.ParserActions;
using EverythingParser.ParserConfigurationScripts;
using OpenQA.Selenium.Support.UI;
using Selenium.WebDriver.EventCapture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EverythingParser.ParserManagers
{
    public class ParsingConfigurationManager
    {
        private WebDriverManager WebDriverManager { get; }
        private EventCapturingWebDriver CaptureDriver { get; }
        private WebDriverWait DriverWait { get; }
        private FilesDownloader FilesDownloader { get; }
        private ControlsManager ControlsManager { get; }
        private CheckedListBox CheckedListBox { get; }
        private FlowLayoutPanel FlowLayoutPanel { get; }
        private AttributesManager AttributesManager { get; }

        public ParsingConfigurationManager(EventCapturingWebDriver captureDriver, WebDriverManager webDriverManager, WebDriverWait driverWait, FilesDownloader filesDownloader, ControlsManager controlsManager, AttributesManager attributesManager, CheckedListBox checkedListBox, FlowLayoutPanel flowLayoutPanel)
        {
            CaptureDriver = captureDriver;
            WebDriverManager = webDriverManager;
            DriverWait = driverWait;
            FilesDownloader = filesDownloader;
            ControlsManager = controlsManager;
            AttributesManager = attributesManager;
            CheckedListBox = checkedListBox;
            FlowLayoutPanel = flowLayoutPanel;
        }

        public void GetExistingSettings(ParsingElementConfiguration existingElementConfig)
        {
            ControlsManager.IsProcessingItemCheck = true;

            if (existingElementConfig != null)
            {
                // Временная коллекция для хранения индексов элементов, которые нужно отметить
                List<int> indicesToCheck = new List<int>();
                List<ucAction> actionsToAdd = new List<ucAction>();

                // Получаем все чекбоксы из chbListBox
                foreach (var item in CheckedListBox.Items)
                {
                    string itemText = item.ToString();

                    int dashIndex = itemText.IndexOf('-');
                    if (dashIndex != -1)
                    {
                        string XPath = itemText.Substring(0, dashIndex).Trim();
                        int colonIndex = itemText.IndexOf(':', dashIndex);

                        if (colonIndex != -1)
                        {
                            string AttributeName = itemText.Substring(dashIndex + 1, colonIndex - dashIndex - 1).Trim();

                            // Ищем действие с таким XPath и AttributeName
                            var attributeAction = existingElementConfig.AttributeActions.FirstOrDefault(a => a.ElementXPath == XPath && a.AttributeName == AttributeName);

                            if (attributeAction != null)
                            {
                                // Добавляем индекс в временную коллекцию
                                int index = CheckedListBox.Items.IndexOf(item);
                                if (index != -1)
                                {
                                    indicesToCheck.Add(index);
                                }

                                // Определяем ActionType

                                if (attributeAction != null)
                                {
                                    ParserActionType actionType = attributeAction.ActionType;

                                    // Создаем и добавляем соответствующий ucAction в временную коллекцию
                                    ucAction action = new ucAction(WebDriverActions.GetBaseUrl(CaptureDriver.Url), XPath, AttributeName, actionType);
                                    actionsToAdd.Add(action);
                                }
                            }
                        }
                    }
                }

                // Применяем изменения в основном потоке
                ApplyExistingSettings(indicesToCheck, actionsToAdd);
            }

            ControlsManager.IsProcessingItemCheck = false;
        }
        private void ApplyExistingSettings(List<int> indicesToCheck, List<ucAction> actionsToAdd)
        {
            if (CheckedListBox.InvokeRequired)
            {
                CheckedListBox.Invoke(new Action(() =>
                {
                    foreach (int index in indicesToCheck)
                    {
                        CheckedListBox.SetItemChecked(index, true);
                    }
                }));
            }
            else
            {
                foreach (int index in indicesToCheck)
                {
                    CheckedListBox.SetItemChecked(index, true);
                }
            }

            if (FlowLayoutPanel.InvokeRequired)
            {
                FlowLayoutPanel.Invoke(new Action(() =>
                {
                    foreach (var action in actionsToAdd)
                    {
                        FlowLayoutPanel.Controls.Add(action);
                        action.AutoSize = true;
                        action.AutoSizeMode = AutoSizeMode.GrowOnly;
                        action.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    }
                }));
            }
            else
            {
                foreach (var action in actionsToAdd)
                {
                    FlowLayoutPanel.Controls.Add(action);
                    action.AutoSize = true;
                    action.AutoSizeMode = AutoSizeMode.GrowOnly;
                    action.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }
            }
        }

        public void SaveElementActions(string ElementXPath, ref List<ParsingPageConfiguration> ParsingPageConfigurations, List<ucAction> ucsList)
        {
            string DownloadFilesFolder = WebDriverManager.DownloadFilesFolderPath;

            try
            {
                if (string.IsNullOrEmpty(ElementXPath))
                {
                    Console.WriteLine("ElementXPath Is Null");
                    return;
                }

                List<AttributeActions> AttributeActions = new List<AttributeActions>();

                ParsingPageConfiguration existingPageConfig = ParsingPageConfigurations.FirstOrDefault(config => config.PageUrl == WebDriverActions.GetBaseUrl(CaptureDriver.Url));
                ParsingElementConfiguration existingElementConfig = null;

                foreach (ucAction uc in ucsList)
                {
                    if (existingPageConfig != null)
                    {
                        existingElementConfig = existingPageConfig.ParsingElements.FirstOrDefault(config => config.ElementXPath == ElementXPath);
                    }
                    else
                    {
                        existingPageConfig = new ParsingPageConfiguration(CaptureDriver, uc.PageUrl);
                        ParsingPageConfigurations.Add(existingPageConfig);
                    }

                    Console.WriteLine($"Сохранение, Url: {uc.PageUrl} XPath: {ElementXPath}");

                    AttributeActions exportAttributeAction = new AttributeActions(CaptureDriver, DriverWait, WebDriverManager, FilesDownloader, AttributesManager, uc.PageUrl, DownloadFilesFolder, uc.XPath, uc.AttributeName, uc.ActionType);
                    AttributeActions.Add(exportAttributeAction);
                }

                ParsingElementConfiguration parsingElementConfiguration = new ParsingElementConfiguration(ElementXPath, AttributeActions);

                if (existingPageConfig != null)
                {
                    existingPageConfig.ParsingElements.Add(parsingElementConfiguration);
                }

                if (existingElementConfig != null)
                {
                    existingPageConfig.ParsingElements.Remove(existingElementConfig);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
