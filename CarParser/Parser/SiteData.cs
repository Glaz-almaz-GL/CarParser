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
        TransportType TransportType { get; }
        IWebDriver Driver { get; }

        string CacheFolderPath { get; }

        WebDriverWait DriverWait { get; }

        string Name { get; }
        string Link { get; }


        public SiteData(TransportType transportType, IWebDriver driver, string cacheFolderPath, WebDriverWait driverWait, string name, string link)
        {
            TransportType = transportType;
            Driver = driver;
            CacheFolderPath = cacheFolderPath;
            DriverWait = driverWait;
            Name = name;
            Link = link;

            CreateTransportData();
        }

        private void CreateTransportData()
        {
            TransportData transport = new TransportData(TransportType, Driver, CacheFolderPath, DriverWait, Link);
            if (Transports == null)
            {
                Transports = new List<TransportData>();
            }
            else
            {
                Transports.Clear();
            }

            Transports.Add(transport);
        }

        public void GetSiteData(ref string name, ref string siteLink)
        {
            name = Name;
            siteLink = Link;

            Console.WriteLine($"Site Name: {Name}, Site Link: {Link}");
        }

        public List<TransportData> Transports { get; private set; }
    }
}