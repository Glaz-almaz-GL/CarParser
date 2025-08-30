using HaynesProParser.Parser.TransportDatas;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HaynesProParser.Services.Processors
{
    public class TransportModificationQuickGuideProcessor
    {
        private readonly WebDriverWait _wait;

        public TransportModificationQuickGuideProcessor(WebDriverWait wait)
        {
            _wait = wait;
        }

        public async Task<List<TransportModificationQuickGuideData>> GetGuideJobsAsync(
            IWebElement guide,
            int mainGuideId,
            string mainGuideSiteId,
            string makerId,
            string transportId,
            string modificationId)
        {
            var guideDatas = new List<TransportModificationQuickGuideData>();
            var guideJobItems = guide.FindElements(By.XPath(".//li[contains(@class, 'followup') or @node-hierarchy]"));
            var guideJobs = TransportModificationHelper.GroupItemsByActiveSections(guideJobItems);

            for (int i = 0; i < guideJobs.Count; i++)
            {
                var guideJob = guideJobs[i];
                var guideJobItem = guideJob.ActiveElement;
                var titleSpan = guideJobItem.FindElement(By.CssSelector("span.title"));
                var timeSpan = guideJobItem.FindElement(By.CssSelector("span.time"));

                var checkbox = _wait.Until(_ =>
                    guideJobItem.FindElement(By.CssSelector("span.custom-checkbox-wrapper:not(.small)")));
                checkbox.Click();

                try
                {
                    _wait.Until(_ => guide.FindElements(By.XPath(".//li[contains(@class, 'included')]")).Count > 0);
                }
                catch (WebDriverTimeoutException)
                {
                    Debug.WriteLine("No included sub-jobs found.");
                }

                var updatedChildItems = guide.FindElements(By.XPath(".//li[contains(@class, 'followup') or @node-hierarchy]"));
                var updatedGuideJobs = TransportModificationHelper.GroupItemsByActiveSections(updatedChildItems);

                var guideName = titleSpan.Text?.Trim();
                var guideId = guideJobItem.GetAttribute("data-id") ?? guideJobItem.GetAttribute("id");
                var guideOeCode = TransportModificationHelper.GetOeCode(titleSpan);
                var guideTime = timeSpan.Text?.Trim();

                var guideData = new TransportModificationQuickGuideData(
                    makerId,
                    transportId,
                    modificationId,
                    mainGuideId,
                    mainGuideSiteId,
                    guideName,
                    guideId,
                    guideOeCode,
                    guideTime,
                    null);

                await guideData.SaveToDatabaseAsync();
                guideData.SubJobDatas = TransportModificationHelper.GetSubJobs(updatedGuideJobs, guideData.dbId + 1, i);

                // Снимаем галочку
                await Task.Delay(100);
                var resetCheckbox = _wait.Until(_ =>
                    guideJobItem.FindElement(By.CssSelector("span.custom-checkbox-wrapper:not(.small)")));
                resetCheckbox.Click();
                await Task.Delay(100);

                guideDatas.Add(guideData);
            }

            return guideDatas;
        }
    }
}
