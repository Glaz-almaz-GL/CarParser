using ClosedXML.Excel;
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
        private ParserData _parserData;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _parserData = new ParserData(TransportType.Car, "CarParser", "https://www.workshopdata.com/touch/site/layout/makesOverview");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_parserData != null)
            {
                foreach (SiteData site in _parserData.Sites)
                {
                    string siteName = "", siteLink = "";

                    site.GetSiteData(ref siteName, ref siteLink);

                    TreeNode siteNode = new TreeNode(siteName);

                    if (site.Transports != null && site.Transports.Count > 0)
                    {
                        foreach (TransportData transport in site.Transports)
                        {
                            TransportType transportType = TransportType.None;

                            transport.GetTransportData(ref transportType);

                            TreeNode transportTypeNode = new TreeNode(transportType.ToString());
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
                                                foreach (TransportModificationDescriptionData transportModification in transportModel.ModificationDescriptions)
                                                {
                                                    string modificationId = "", modificationType = "", modificationEngineCode = "", modificationFuelType = "", modificationVolume = "", modificationPower = "", modificationModelYear = "";

                                                    transportModification.GetModificationData(ref modificationId, ref modificationType, ref modificationEngineCode, ref modificationFuelType, ref modificationVolume, ref modificationPower, ref modificationModelYear);

                                                    TreeNode modificationNode = new TreeNode(modificationType);

                                                    TreeNode modificationIdNode = new TreeNode("Id: " + modificationId);
                                                    TreeNode modificationEngineCodeNode = new TreeNode("Engine Code: " + modificationEngineCode);
                                                    TreeNode modificationFuelTypeNode = new TreeNode("Fuel Type: " + modificationFuelType);
                                                    TreeNode modificationVolumeNode = new TreeNode("Volume: " + modificationVolume);
                                                    TreeNode modificationPowerNode = new TreeNode("Power: " + modificationPower);
                                                    TreeNode modificationModelYearNode = new TreeNode("Model Year: " + modificationModelYear);

                                                    modificationNode.Nodes.Add(modificationIdNode);
                                                    modificationNode.Nodes.Add(modificationEngineCodeNode);
                                                    modificationNode.Nodes.Add(modificationFuelTypeNode);
                                                    modificationNode.Nodes.Add(modificationVolumeNode);
                                                    modificationNode.Nodes.Add(modificationPowerNode);
                                                    modificationNode.Nodes.Add(modificationModelYearNode);

                                                    modelNode.Nodes.Add(modificationNode);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    treeView1.Nodes.Add(siteNode);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parserData.Driver.Quit();
        }
    }
}
