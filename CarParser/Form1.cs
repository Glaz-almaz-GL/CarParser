using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using Microsoft.AspNetCore.WebUtilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Svg;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace CarParser
{
    public partial class Form1 : Form
    {
        private string SiteLink = "https://www.workshopdata.com/";

        private List<ParserData> ParserDatas = new List<ParserData>();
        private bool isProcessing = false;

        private List<Dictionary<TransportTypes, string>> sitesInfo = new List<Dictionary<TransportTypes, string>>();

        public Form1()
        {
            InitializeComponent();
            treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;

            Dictionary<TransportTypes, string> HaynesProSite = new Dictionary<TransportTypes, string>
            {
                { TransportTypes.Car, "https://www.workshopdata.com/touch/site/layout/makesOverview" },
                { TransportTypes.Motorcycle, "https://www.workshopdata.com/touch/site/layout/makesMotoOverview" },
                { TransportTypes.Truck, "https://www.workshopdata.com/touch/site/layout/makesOverviewTrucks" },
                { TransportTypes.Trailer, "https://www.workshopdata.com/touch/site/layout/makesOverviewTrucks" },
                { TransportTypes.Axel, "https://www.workshopdata.com/touch/site/layout/makesOverviewTrucks" }
            };

            sitesInfo.Add(HaynesProSite);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (isProcessing) return;
            isProcessing = true;

            button1.Enabled = false;
            ParserDatas.Clear(); // Очищаем предыдущие данные

            // Создаем список задач для хранения всех запущенных задач
            List<Task<ParserData>> tasks = new List<Task<ParserData>>();

            foreach (var site in sitesInfo)
            {
                // Добавляем каждую задачу в список
                tasks.Add(Task.Run(() => new ParserData("HaynesPro", SiteLink, site)));
            }

            // Ожидаем завершения всех задач
            var results = await Task.WhenAll(tasks);

            // Добавляем результаты в список parserDatas
            ParserDatas.AddRange(results);

            // Обработка результатов из всех задач
            foreach (var parserData in ParserDatas)
            {
                // Выполняем необходимые действия с каждым parserData
                CreateTreeView(parserData); // Предполагается, что CreateTreeView принимает ParserData
            }

            // Включаем кнопку после завершения всех операций
            button1.Enabled = true;
            isProcessing = false;
        }

        private void CreateTreeView(ParserData _parserData)
        {
            if (_parserData != null)
            {
                string parserName = "";
                _parserData.GetParserData(ref parserName);

                TreeNode parserNode = new TreeNode($"Parser ({parserName})");

                foreach (SiteData site in _parserData.Sites)
                {
                    string siteName = "", siteLink = "";

                    site.GetSiteData(ref siteName, ref siteLink);

                    TreeNode siteNode = new TreeNode(siteName);
                    parserNode.Nodes.Add(siteNode);

                    if (site.Transports != null && site.Transports.Count > 0)
                    {
                        foreach (TransportData transport in site.Transports)
                        {
                            TransportTypes transportType = TransportTypes.None;

                            transport.GetTransportData(ref transportType);

                            TreeNode transportTypeNode = new TreeNode(transportType.ToString() + "s");
                            siteNode.Nodes.Add(transportTypeNode);

                            if (transport.Brands != null && transport.Brands.Count > 0)
                            {
                                foreach (TransportBrandData transportBrand in transport.Brands)
                                {
                                    string brandId = "", brandName = "", brandPageLink = "";

                                    transportBrand.GetBrandData(ref brandId, ref brandName, ref brandPageLink);

                                    TreeNode brandNode = new TreeNode(brandName);
                                    transportTypeNode.Nodes.Add(brandNode);

                                    if (transportBrand.TransportModels != null && transportBrand.TransportModels.Count > 0)
                                    {
                                        foreach (TransportModelData transportModel in transportBrand.TransportModels)
                                        {
                                            string modelId = "", modelName = "", modelPageLink = "";

                                            transportModel.GetModelData(ref modelId, ref modelName, ref modelPageLink);

                                            TreeNode modelNode = new TreeNode(modelName);
                                            brandNode.Nodes.Add(modelNode);

                                            if (transportModel.ModificationDescriptions != null && transportModel.ModificationDescriptions.Count > 0)
                                            {
                                                // Словарь для хранения существующих узлов и счетчиков по их названию
                                                Dictionary<string, (TreeNode node, int count)> existingNodes = new Dictionary<string, (TreeNode, int)>();

                                                foreach (TransportModificationDescriptionData transportModification in transportModel.ModificationDescriptions)
                                                {
                                                    string modificationId = "", modificationType = "", modificationEngineCode = "", modificationFuelType = "", modificationVolume = "", modificationBridge = "", modificationPower = "", modificationModelYear = "", modificationManufactureType = "";
                                                    string modificationWeight = "";
                                                    string modificationGroup = "", modificationCode = "", modificationMakeName = "", modificationDescription = "";

                                                    transportModification.GetModificationData(ref modificationId, ref modificationGroup, ref modificationType, ref modificationMakeName, ref modificationEngineCode, ref modificationCode, ref modificationFuelType, ref modificationVolume, ref modificationBridge, ref modificationPower, ref modificationModelYear, ref modificationManufactureType, ref modificationDescription, ref modificationWeight);

                                                    TreeNode modificationIdNode = new TreeNode("Id: " + modificationId);
                                                    TreeNode modificationEngineCodeNode = new TreeNode("Код двигателя: " + modificationEngineCode);
                                                    TreeNode modificationCodeNode = new TreeNode("Код: " + modificationCode);
                                                    TreeNode modificationGroupNode = new TreeNode("Группа: " + modificationGroup);
                                                    TreeNode modificationMakeNameNode = new TreeNode("Изготовитель: " + modificationMakeName);
                                                    TreeNode modificationFuelTypeNode = new TreeNode("Тип топлива: " + modificationFuelType);
                                                    TreeNode modificationVolumeNode = new TreeNode("Объём: " + modificationVolume);
                                                    TreeNode modificationBridgeNode = new TreeNode("Мост: " + modificationBridge);
                                                    TreeNode modificationPowerNode = new TreeNode("Мощность: " + modificationPower);
                                                    TreeNode modificationModelYearNode = new TreeNode("Модельный год: " + modificationModelYear);
                                                    TreeNode modificationManufactureTypeNode = new TreeNode("Тип изготовления: " + modificationManufactureType);
                                                    TreeNode modificationDescriptionNode = new TreeNode("Описание: " + modificationDescription);
                                                    TreeNode modificationWeightNode = new TreeNode("Вес: " + modificationWeight);

                                                    // Проверяем, существует ли уже узел с таким названием
                                                    if (existingNodes.ContainsKey(modificationType))
                                                    {
                                                        // Если узел существует, увеличиваем счетчик и создаем новый подузел с цифрой
                                                        var (existingModificationNode, count) = existingNodes[modificationType];
                                                        count++;
                                                        TreeNode modificationNode = new TreeNode($"{modificationType} ({count})");

                                                        modificationNode.Nodes.Add(modificationIdNode);
                                                        modificationNode.Nodes.Add(modificationEngineCodeNode);
                                                        modificationNode.Nodes.Add(modificationCodeNode);
                                                        modificationNode.Nodes.Add(modificationGroupNode);
                                                        modificationNode.Nodes.Add(modificationMakeNameNode);
                                                        modificationNode.Nodes.Add(modificationFuelTypeNode);
                                                        modificationNode.Nodes.Add(modificationVolumeNode);
                                                        modificationNode.Nodes.Add(modificationBridgeNode);
                                                        modificationNode.Nodes.Add(modificationPowerNode);
                                                        modificationNode.Nodes.Add(modificationModelYearNode);
                                                        modificationNode.Nodes.Add(modificationManufactureTypeNode);
                                                        modificationNode.Nodes.Add(modificationDescriptionNode);
                                                        modificationNode.Nodes.Add(modificationWeightNode);

                                                        existingModificationNode.Nodes.Add(modificationNode);

                                                        // Обновляем счетчик в словаре
                                                        existingNodes[modificationType] = (existingModificationNode, count);
                                                    }
                                                    else
                                                    {
                                                        // Если узла нет, создаем новый узел с цифрой и добавляем его в дерево
                                                        int count = 0;
                                                        TreeNode modificationNode = new TreeNode($"{modificationType}");

                                                        modificationNode.Nodes.Add(modificationIdNode);
                                                        modificationNode.Nodes.Add(modificationEngineCodeNode);
                                                        modificationNode.Nodes.Add(modificationCodeNode);
                                                        modificationNode.Nodes.Add(modificationGroupNode);
                                                        modificationNode.Nodes.Add(modificationMakeNameNode);
                                                        modificationNode.Nodes.Add(modificationFuelTypeNode);
                                                        modificationNode.Nodes.Add(modificationVolumeNode);
                                                        modificationNode.Nodes.Add(modificationBridgeNode);
                                                        modificationNode.Nodes.Add(modificationPowerNode);
                                                        modificationNode.Nodes.Add(modificationModelYearNode);
                                                        modificationNode.Nodes.Add(modificationManufactureTypeNode);
                                                        modificationNode.Nodes.Add(modificationDescriptionNode);
                                                        modificationNode.Nodes.Add(modificationWeightNode);

                                                        modelNode.Nodes.Add(modificationNode);

                                                        // Сохраняем узел и счетчик в словарь
                                                        existingNodes[modificationType] = (modificationNode, count);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    treeView1.Nodes.Add(parserNode);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Закрываем драйверы для всех ParserData
            foreach (var parserData in ParserDatas)
            {
                if (parserData?.Driver != null)
                {
                    parserData.Driver.Quit();
                }
            }

            // Очищаем список parserDatas
            ParserDatas.Clear();
        }

        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // Определяем цвет для текущего узла
            Color backColor = (e.Node.Index % 2 == 0 && e.Node.Level % 2 == 0) ? Color.FromArgb(235, 235, 235) : Color.White;

            e.Node.BackColor = backColor;

            // Рисуем фон узла
            using (SolidBrush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            // Рисуем текст узла
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                TextRenderer.DrawText(e.Graphics, e.Node.Text, treeView1.Font, e.Bounds, Color.Black, TextFormatFlags.GlyphOverhangPadding);
            }

            e.DrawDefault = false;
        }
    }
}
