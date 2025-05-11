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
        TransportParser carParser;
        TransportParser motorcycleParser;
        TransportParser truckParser;


        public Form1()
        {
            InitializeComponent();
            carParser = new TransportParser();
            motorcycleParser = new TransportParser();
            truckParser = new TransportParser();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            _ = carParser.StartParsing(this, "Cars", "Overview", "modelTypes");
            _ = motorcycleParser.StartParsing(this, "Motorcycles", "MotoOverview", "modelTypes");
            _ = truckParser.StartParsing(this, "Trucks", "OverviewTrucks", "modelTypesTrucks");
        }
    }
}
