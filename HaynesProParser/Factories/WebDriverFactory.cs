using HaynesProParser.Managers.WebManagers;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Concurrent;

namespace HaynesProParser.Factories
{
    public interface IDriverFactory
    {
        (ChromeDriver, WebDriverWait) CreateDriver(bool headless = false);
        void ShutdownAllDrivers();
    }

    public class DriverFactory : IDriverFactory
    {
        public readonly ConcurrentBag<(ChromeDriver, WebDriverWait)> _driverPools = [];

        public (ChromeDriver, WebDriverWait) CreateDriver(bool headless = false)
        {
            var driverInfo = WebDriverManager.CreateAnotherDriver(headless);
            _driverPools.Add(driverInfo);
            return driverInfo;
        }

        public void ShutdownAllDrivers()
        {
            foreach (var (driver, _) in _driverPools)
            {
                try
                {
                    driver?.Quit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error shutting down driver: {ex.Message}");
                }
            }

            _driverPools.Clear();
        }
    }
}
