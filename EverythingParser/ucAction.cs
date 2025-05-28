using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EverythingParser
{
    public partial class ucAction : UserControl
    {
        public string PageUrl { get; private set; }
        public string XPath { get; private set; }
        public string AttributeName { get; private set; }
        public ParserActionType ActionType { get; private set; }

        public ucAction(string pageUrl, string xpath, string attributeName, ParserActionType actionType)
        {
            InitializeComponent();

            PageUrl = pageUrl;
            XPath = xpath;
            AttributeName = attributeName;
            ActionType = actionType;

            lbTagName.Text = XPath;
            lbAttribute.Text = AttributeName;

            switch (actionType)
            {
                case ParserActionType.None:
                    cbActions.SelectedItem = "Ничего";
                    break;

                case ParserActionType.DownloadFile:
                    cbActions.SelectedItem = "Скачать файл по ссылке из атрибута";
                    break;

                case ParserActionType.DownloadAttributes:
                    cbActions.SelectedItem = "Экспортировать текст атрибута";
                    break;

                case ParserActionType.DownloadIdFromURL:
                    cbActions.SelectedItem = "Экспортировать Id из URL";
                    break;

                case ParserActionType.ClickOnElement:
                    cbActions.SelectedItem = "Нажать по элементу";
                    break;
            }
        }

        private void cbActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            string actionName = cbActions.SelectedItem.ToString();

            switch (actionName)
            {
                case "Ничего":
                    ActionType = ParserActionType.None;
                    break;

                case "Скачать файл по ссылке из атрибута":
                    ActionType = ParserActionType.DownloadFile;
                    break;

                case "Экспортировать текст атрибута":
                    ActionType = ParserActionType.DownloadAttributes;
                    break;

                case "Экспортировать ID из URL":
                    ActionType = ParserActionType.DownloadIdFromURL;
                    break;

                case "Нажать по элементу":
                    ActionType = ParserActionType.ClickOnElement;
                    break;
            }
        }
    }
}
