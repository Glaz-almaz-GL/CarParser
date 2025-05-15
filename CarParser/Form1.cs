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

                                            switch (transportType)
                                            {
                                                case TransportTypes.Car:
                                                    if (transportModel.CarModificationDescriptions != null && transportModel.CarModificationDescriptions.Count > 0)
                                                    {
                                                        foreach (var carModification in transportModel.CarModificationDescriptions)
                                                        {
                                                            string ModificationId = "", ModificationType = "", ModificationEngineCode = "", ModificationFuelType = "", ModificationVolume = "", ModificationPower = "", ModificationModelYear = "";

                                                            carModification.GetCarModificationData(ref ModificationId, ref ModificationType, ref ModificationEngineCode, ref ModificationFuelType, ref ModificationVolume, ref ModificationPower, ref ModificationModelYear);

                                                            TreeNode IdNode = new TreeNode("Id: " + ModificationId);
                                                            TreeNode EngineCodeNode = new TreeNode("Код двигателя: " + ModificationEngineCode);
                                                            TreeNode FuelTypeNode = new TreeNode("Тип топлива " + ModificationFuelType);
                                                            TreeNode VolumeNode = new TreeNode("Объём: " + ModificationVolume);
                                                            TreeNode PowerNode = new TreeNode("Мощность: " + ModificationPower);
                                                            TreeNode ModelYearNode = new TreeNode("Модельный год: " + ModificationModelYear);

                                                            TreeNode CarModificationNode = new TreeNode(ModificationType);

                                                            CarModificationNode.Nodes.Add(IdNode);
                                                            CarModificationNode.Nodes.Add(EngineCodeNode);
                                                            CarModificationNode.Nodes.Add(FuelTypeNode);
                                                            CarModificationNode.Nodes.Add(VolumeNode);
                                                            CarModificationNode.Nodes.Add(PowerNode);
                                                            CarModificationNode.Nodes.Add(ModelYearNode);

                                                            modelNode.Nodes.Add(CarModificationNode);
                                                        }
                                                    }
                                                    break;

                                                case TransportTypes.Motorcycle:
                                                    if (transportModel.MotorcycleModificationDescriptions != null && transportModel.MotorcycleModificationDescriptions.Count > 0)
                                                    {
                                                        foreach (var motorcycleModification in transportModel.MotorcycleModificationDescriptions)
                                                        {
                                                            string ModificationId = "", ModificationType = "", ModificationEngineCode = "", ModificationFuelType = "", ModificationVolume = "", ModificationPower = "", ModificationModelYear = "";

                                                            motorcycleModification.GetMotorcycleModificationData(ref ModificationId, ref ModificationType, ref ModificationEngineCode, ref ModificationFuelType, ref ModificationVolume, ref ModificationPower, ref ModificationModelYear);

                                                            TreeNode IdNode = new TreeNode("Id: " + ModificationId);
                                                            TreeNode EngineCodeNode = new TreeNode("Код двигателя: " + ModificationEngineCode);
                                                            TreeNode FuelTypeNode = new TreeNode("Тип топлива " + ModificationFuelType);
                                                            TreeNode VolumeNode = new TreeNode("Объём: " + ModificationVolume);
                                                            TreeNode PowerNode = new TreeNode("Мощность: " + ModificationPower);
                                                            TreeNode ModelYearNode = new TreeNode("Модельный год: " + ModificationModelYear);

                                                            TreeNode MotorcycleModificationNode = new TreeNode(ModificationType);

                                                            MotorcycleModificationNode.Nodes.Add(IdNode);
                                                            MotorcycleModificationNode.Nodes.Add(EngineCodeNode);
                                                            MotorcycleModificationNode.Nodes.Add(FuelTypeNode);
                                                            MotorcycleModificationNode.Nodes.Add(VolumeNode);
                                                            MotorcycleModificationNode.Nodes.Add(PowerNode);
                                                            MotorcycleModificationNode.Nodes.Add(ModelYearNode);

                                                            modelNode.Nodes.Add(MotorcycleModificationNode);
                                                        }
                                                    }
                                                    break;

                                                case TransportTypes.Truck:
                                                    if (transportModel.TruckModificationDescriptions != null && transportModel.TruckModificationDescriptions.Count > 0)
                                                    {
                                                        foreach (var truckModification in transportModel.TruckModificationDescriptions)
                                                        {
                                                            string ModificationId = "", ModificationType = "", ModificationEngineCode = "", ModificationVolume = "", ModificationBridge = "", ModificationPower = "", ModificationModelYear = "", ModificationManufactureType = "", ModificationWeight = "";

                                                            truckModification.GetTruckModificationData(ref ModificationId, ref ModificationType, ref ModificationEngineCode, ref ModificationVolume, ref ModificationBridge, ref ModificationPower, ref ModificationModelYear, ref ModificationManufactureType, ref ModificationWeight);

                                                            TreeNode IdNode = new TreeNode("Id: " + ModificationId);
                                                            TreeNode EngineCodeNode = new TreeNode("Код двигателя: " + ModificationEngineCode);
                                                            TreeNode VolumeNode = new TreeNode("Объём: " + ModificationVolume);
                                                            TreeNode BridgeNode = new TreeNode("Мост: " + ModificationBridge);
                                                            TreeNode PowerNode = new TreeNode("Объём: " + ModificationVolume);
                                                            TreeNode ModelYearNode = new TreeNode("Модельный год: " + ModificationModelYear);
                                                            TreeNode ManufactureTypeNode = new TreeNode("Вес (Тонны): " + ModificationWeight);

                                                            TreeNode TruckModificationNode = new TreeNode(ModificationType);

                                                            TruckModificationNode.Nodes.Add(IdNode);
                                                            TruckModificationNode.Nodes.Add(EngineCodeNode);
                                                            TruckModificationNode.Nodes.Add(VolumeNode);
                                                            TruckModificationNode.Nodes.Add(BridgeNode);
                                                            TruckModificationNode.Nodes.Add(PowerNode);
                                                            TruckModificationNode.Nodes.Add(ModelYearNode);
                                                            TruckModificationNode.Nodes.Add(ManufactureTypeNode);

                                                            modelNode.Nodes.Add(TruckModificationNode);
                                                        }
                                                    }
                                                    break;

                                                case TransportTypes.Trailer:
                                                    if (transportModel.TrailerModificationDescriptions != null && transportModel.TrailerModificationDescriptions.Count > 0)
                                                    {
                                                        foreach (var trailerModification in transportModel.TrailerModificationDescriptions)
                                                        {
                                                            string ModificationId = "", ModificationGroup = "", ModificationType = "", ModificationMakeName = "", ModificationCode = "", ModificationDescription = "";

                                                            trailerModification.GetTrailerModificationData(ref ModificationId, ref ModificationGroup, ref ModificationType, ref ModificationMakeName, ref ModificationCode, ref ModificationDescription);

                                                            TreeNode IdNode = new TreeNode("Id: " + ModificationId);
                                                            TreeNode GroupNode = new TreeNode("Группа: " + ModificationGroup);
                                                            TreeNode MakeNameNode = new TreeNode("Изготовитель: " + ModificationCode);
                                                            TreeNode CodeNode = new TreeNode("Код: " + ModificationCode);
                                                            TreeNode DescriptionNode = new TreeNode("Описание: " + ModificationDescription);

                                                            TreeNode TruckModificationNode = new TreeNode(ModificationType);

                                                            TruckModificationNode.Nodes.Add(IdNode);
                                                            TruckModificationNode.Nodes.Add(GroupNode);
                                                            TruckModificationNode.Nodes.Add(MakeNameNode);
                                                            TruckModificationNode.Nodes.Add(CodeNode);
                                                            TruckModificationNode.Nodes.Add(DescriptionNode);

                                                            modelNode.Nodes.Add(TruckModificationNode);
                                                        }
                                                    }
                                                    break;
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
