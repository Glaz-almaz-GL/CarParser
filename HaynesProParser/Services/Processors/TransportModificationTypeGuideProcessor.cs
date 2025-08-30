using HaynesProParser.Managers.WebManagers;
using HaynesProParser.Parser.TransportDatas;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HaynesProParser.Services.Processors
{
    public class TransportModificationTypeGuideProcessor
    {
        private readonly ChromeDriver _driver;
        private readonly WebDriverWait _wait;

        public TransportModificationTypeGuideProcessor(ChromeDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }

        public async Task<TransportModificationQuickGuideTypeData> ProcessAsync(
            KeyValuePair<string, string> guideType,
            string makerId,
            int transportId,
            string transportSiteId,
            string modificationId)
        {
            try
            {
                Debug.WriteLine($"Navigating to: {guideType.Value}");

                await WebDriverManager.NavigateAsync(_driver, $"https://www.workshopdata.com/touch/site/layout/modelDetail?typeId={modificationId}");
                await WebDriverManager.NavigateAsync(_driver, guideType.Value);

                IWebElement dropdownItems;
                try
                {
                    dropdownItems = _driver.FindElement(By.ClassName("dropdown-items"));
                }
                catch (NoSuchElementException)
                {
                    await WebDriverManager.NavigateAsync(_driver, $"https://www.workshopdata.com/touch/site/layout/modelDetail?typeId={modificationId}");
                    await WebDriverManager.NavigateAsync(_driver, guideType.Value);
                    dropdownItems = _driver.FindElement(By.ClassName("dropdown-items"));
                }

                var guideItems = _wait.Until(_ => dropdownItems.FindElements(By.XPath("./li")));
                Debug.WriteLine($"Found {guideItems.Count} guide items");

                var typeGuideData = new TransportModificationQuickGuideTypeData(
                    guideType.Key,
                    transportId,
                    transportSiteId,
                    GetLinkAttribute(guideType.Value, "levelId"),
                    guideType.Value,
                    null);

                await typeGuideData.SaveToDatabaseAsync();

                typeGuideData.MainQuickGuideDatas = await new TransportModificationMainGuideProcessor(_wait)
                    .GetMainGuidesAsync(guideItems, typeGuideData.dbId + 1, guideType, makerId, transportSiteId, modificationId);

                return typeGuideData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TypeGuide Error: {ex.Message}, Inner: {ex.InnerException?.Message}");
                return null;
            }
        }

        private static string GetLinkAttribute(string url, string paramName)
        {
            try
            {
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return query[paramName];
            }
            catch
            {
                return null;
            }
        }
    }
}
