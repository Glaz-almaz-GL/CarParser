using HaynesProParser.Parser.TransportDatas;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HaynesProParser.Services.Processors
{
    public class TransportModificationMainGuideProcessor
    {
        private readonly WebDriverWait _wait;

        public TransportModificationMainGuideProcessor(WebDriverWait wait)
        {
            _wait = wait;
        }

        public async Task<List<TransportModificationMainQuickGuideData>> GetMainGuidesAsync(
            IEnumerable<IWebElement> guideItems,
            int typeGuideId,
            KeyValuePair<string, string> guideType,
            string makerId,
            string transportId,
            string modificationId)
        {
            var mainGuideDatas = new List<TransportModificationMainQuickGuideData>();

            foreach (var guide in guideItems)
            {
                if (!guide.GetAttribute("class").Contains("active"))
                {
                    guide.Click();
                }

                var anchor = guide.FindElement(By.TagName("a"));
                var mainName = anchor.Text?.Trim();
                var mainId = anchor.GetAttribute("id");

                var modMainGuide = new TransportModificationMainQuickGuideData(
                    makerId,
                    transportId,
                    modificationId,
                    mainName,
                    mainId,
                    typeGuideId,
                    guideType.Key,
                    null);

                await modMainGuide.SaveToDatabaseAsync();

                modMainGuide.TransportModificationQuickGuidesDatas = await new TransportModificationQuickGuideProcessor(_wait)
                    .GetGuideJobsAsync(guide, modMainGuide.dbId + 1, mainId, makerId, transportId, modificationId);

                mainGuideDatas.Add(modMainGuide);
            }

            return mainGuideDatas;
        }
    }
}
