using HaynesProParser.Factories;
using HaynesProParser.Managers.WebManagers;
using HaynesProParser.Parser.TransportDatas;
using HaynesProParser.Services.Processors;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HaynesProParser.Services
{
    public class TransportModificationQuickGuideTypeService
    {
        private readonly DriverFactory _driverFactory;

        public TransportModificationQuickGuideTypeService(DriverFactory driverFactory)
        {
            _driverFactory = driverFactory;
        }

        public async Task<List<TransportModificationQuickGuideTypeData>> GetTypeGuidesAsync(
            string makerId,
            int transportId,
            string transportSiteId,
            string modificationId,
            (ChromeDriver, WebDriverWait) initialDriverPool)
        {
            var guideTypeElements = initialDriverPool.Item1.FindElements(
                By.CssSelector("nav.side-nav a.arrow:not(.active)"));

            Debug.WriteLine($"Found {guideTypeElements.Count} guide type elements");

            var guideTypes = guideTypeElements
                .ToDictionary(bt => bt.Text, bt => bt.GetAttribute("href"));

            if (guideTypes.Count == 0)
            {
                return [];
            }

            var guideTypesList = guideTypes.ToList();
            var subsets = SplitIntoSubsets(guideTypesList, WebDriverManager.ParallelGuideTypesCount);

            var driverPool = await CreateDriverPoolAsync(subsets.Count, modificationId);

            var tasks = subsets
                .Select((subset, i) => ProcessGuideTypesSubsetAsync(
                    subset, makerId, transportId, transportSiteId, modificationId, driverPool[i].driver, driverPool[i].wait))
                .ToList();

            var results = await Task.WhenAll(tasks);

            _driverFactory.ShutdownAllDrivers();

            return [.. results.SelectMany(r => r)];
        }

        private static List<List<KeyValuePair<string, string>>> SplitIntoSubsets(
            List<KeyValuePair<string, string>> guideTypes,
            int parallelCount)
        {
            var totalItems = guideTypes.Count;
            var itemsPerDriver = (int)Math.Ceiling((double)totalItems / parallelCount);
            var subsets = new List<List<KeyValuePair<string, string>>>();

            for (int i = 0; i < parallelCount; i++)
            {
                int startIndex = i * itemsPerDriver;
                int endIndex = Math.Min(startIndex + itemsPerDriver, totalItems);
                if (startIndex < endIndex)
                {
                    subsets.Add(guideTypes.Skip(startIndex).Take(endIndex - startIndex).ToList());
                }
            }

            return subsets;
        }

        private async Task<List<(ChromeDriver driver, WebDriverWait wait)>> CreateDriverPoolAsync(int count, string modificationId)
        {
            var pool = new List<(ChromeDriver, WebDriverWait)>();

            for (int i = 0; i < count; i++)
            {
                var (driver, wait) = _driverFactory.CreateDriver(true);
                await WebDriverManager.DemoLoginAsync(driver, wait);
                await WebDriverManager.NavigateAsync(driver, $"https://www.workshopdata.com/touch/site/layout/modelDetail?typeId={modificationId}");
                await Task.Delay(TimeSpan.FromSeconds(15));
                pool.Add((driver, wait));
            }

            return pool;
        }

        private static async Task<List<TransportModificationQuickGuideTypeData>> ProcessGuideTypesSubsetAsync(
            List<KeyValuePair<string, string>> subset,
            string makerId,
            int transportId,
            string transportSiteId,
            string modificationId,
            ChromeDriver driver,
            WebDriverWait wait)
        {
            var results = new List<TransportModificationQuickGuideTypeData>();

            foreach (var guideType in subset)
            {
                Debug.WriteLine($"Processing guide type: \"{guideType.Key}\" on driver {driver.GetHashCode()}");
                var result = await new TransportModificationTypeGuideProcessor(driver, wait).ProcessAsync(
                    guideType, makerId, transportId, transportSiteId, modificationId);

                if (result != null)
                {
                    results.Add(result);
                }
            }

            return results;
        }
    }
}
