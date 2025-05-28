using EverythingParser.ParserActions;
using Selenium.WebDriver.EventCapture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EverythingParser.ParserManagers
{
    public class ControlsManager
    {
        public FlowLayoutPanel FlowLayoutPanel { get; private set; }
        public CheckedListBox CheckedListBox { get; private set; }
        public EventCapturingWebDriver CaptureDriver { get; private set; }

        public bool IsProcessingItemCheck { get; set; }

        public ControlsManager(WebDriverManager webDriverManager, CheckedListBox checkedListBox, FlowLayoutPanel flowLayoutPanel, EventCapturingWebDriver captureDriver)
        {
            CheckedListBox = checkedListBox;
            FlowLayoutPanel = flowLayoutPanel;
            CaptureDriver = captureDriver;
        }

        public void HandleItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (IsProcessingItemCheck)
            {
                return;
            }

            try
            {
                string XPath = "", AttributeName = "", itemText = "";

                // Получаем текст элемента, который был изменен
                if (CheckedListBox.InvokeRequired)
                {
                    CheckedListBox.Invoke(new MethodInvoker(() => itemText = CheckedListBox.Items[e.Index].ToString()));
                }
                else
                {
                    itemText = CheckedListBox.Items[e.Index].ToString();
                }

                // Получаем новое состояние флажка
                bool isChecked = (e.NewValue == CheckState.Checked);

                ControlItemCheck(itemText, isChecked, XPath, AttributeName);

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void RemoveControl(string XPath, string AttributeName)
        {
            Control controlToRemove = null;
            foreach (Control control in FlowLayoutPanel.Controls)
            {
                if (control is ucAction action && action.XPath == XPath && action.AttributeName == AttributeName)
                {
                    controlToRemove = control;
                    break;
                }
            }

            if (controlToRemove != null)
            {
                FlowLayoutPanel.Controls.Remove(controlToRemove);
                controlToRemove.Dispose();
            }
        }
        private void AddControl(ucAction action)
        {
            FlowLayoutPanel.Controls.Add(action);
            action.AutoSize = true; // Включаем автоматическое изменение размера
            action.AutoSizeMode = AutoSizeMode.GrowOnly;
            action.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        }
        private bool ControlAlreadyExist(string XPath, string AttributeName)
        {
            return FlowLayoutPanel.Controls.Cast<Control>()
                                    .Any(c => c is ucAction action && action.XPath == XPath && action.AttributeName == AttributeName);
        }
        private void ControlItemCheck(string itemText, bool isChecked, string XPath, string AttributeName)
        {
            int dashIndex = itemText.IndexOf('-');

            if (dashIndex != -1)
            {
                // Получить текст до '-'
                XPath = itemText.Substring(0, dashIndex).Trim();

                // Найти индекс первого ':'
                int colonIndex = itemText.IndexOf(':', dashIndex);
                if (colonIndex != -1)
                {
                    // Получить текст между '-' и ':'
                    AttributeName = itemText.Substring(dashIndex + 1, colonIndex - dashIndex - 1).Trim();

                    //Console.WriteLine("Текст до '-': " + XPath);
                    //Console.WriteLine("Текст между '-' и ':': " + AttributeName);

                    if (isChecked)
                    {
                        // Проверяем, существует ли уже такой элемент в flowLayoutPanel1
                        bool alreadyExists = false;

                        if (FlowLayoutPanel.InvokeRequired)
                        {
                            FlowLayoutPanel.Invoke(new Action(() => alreadyExists = ControlAlreadyExist(XPath, AttributeName)));
                        }
                        else
                        {
                            ControlAlreadyExist(XPath, AttributeName);
                        }

                        if (!alreadyExists)
                        {
                            ucAction action = new ucAction(WebDriverActions.GetBaseUrl(CaptureDriver.Url), XPath, AttributeName, ParserActionType.None);

                            if (FlowLayoutPanel.InvokeRequired)
                            {
                                FlowLayoutPanel.Invoke(new Action(() => AddControl(action)));
                            }
                            else
                            {
                                AddControl(action);
                            }
                        }
                    }
                    else
                    {
                        // Перебираем все контролы в flowLayoutPanel1 и удаляем соответствующий элемент
                        if (FlowLayoutPanel.InvokeRequired)
                        {
                            FlowLayoutPanel.Invoke(new Action(() => RemoveControl(XPath, AttributeName)));
                        }
                        else
                        {
                            RemoveControl(XPath, AttributeName);
                        }
                    }
                }
            }
        }
    }
}
