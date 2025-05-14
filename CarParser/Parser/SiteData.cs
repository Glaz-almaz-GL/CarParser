using DocumentFormat.OpenXml.Office2010.ExcelAc;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Net;

namespace CarParser
{
    public class SiteData
    {
        IWebDriver Driver { get; }

        Dictionary<TransportTypes, string> TransportInfo { get; }

        string CacheFolderPath { get; }

        WebDriverWait DriverWait { get; }

        string Name { get; }
        string Link { get; }


        public SiteData(IWebDriver driver, string cacheFolderPath, WebDriverWait driverWait, string name, string siteLink, Dictionary<TransportTypes, string> transportInfo)
        {
            Driver = driver;
            CacheFolderPath = cacheFolderPath;
            DriverWait = driverWait;
            Name = name;
            Link = siteLink;
            TransportInfo = transportInfo;

            CreateTransportData();
        }

        private void CreateTransportData()
        {
            foreach (var transportData in TransportInfo)
            {
                TransportData transport = new TransportData(transportData.Key, Driver, Name, CacheFolderPath, DriverWait, transportData.Value);
                if (Transports == null)
                {
                    Transports = new List<TransportData>();
                }

                Transports.Add(transport);
            }
        }

        public void GetSiteData(ref string name, ref string siteLink)
        {
            name = Name;
            siteLink = Link;

            Console.WriteLine($"Log (SiteData.cs): Site Name: {Name}, Site Link: {Link}");
        }

        public List<TransportData> Transports { get; private set; }
    }
}