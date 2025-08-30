using HaynesProParser.Parser.TransportDatas;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace HaynesProParser.Services
{
    public static class TransportModificationHelper
    {
        public static string GetOeCode(IWebElement titleElement)
        {
            try
            {
                var oeCodeElement = titleElement.FindElement(By.CssSelector("span.oeCode"));
                return oeCodeElement.Text?.Trim().Replace("OE: ", "") ?? string.Empty;
            }
            catch (NoSuchElementException)
            {
                Debug.WriteLine("OE Code not found");
                return string.Empty;
            }
        }

        public static List<SectionGroup> GroupItemsByActiveSections(ReadOnlyCollection<IWebElement> items)
        {
            var groups = new List<SectionGroup>();
            SectionGroup currentGroup = null;

            foreach (var item in items)
            {
                if (item.GetAttribute("class").Contains("active"))
                {
                    if (currentGroup != null)
                    {
                        groups.Add(currentGroup);
                    }

                    currentGroup = new SectionGroup { ActiveElement = item, ChildElements = [] };
                }
                else if (currentGroup != null && !item.GetAttribute("class").Contains("active"))
                {
                    currentGroup.ChildElements.Add(item);
                }
            }

            if (currentGroup != null)
            {
                groups.Add(currentGroup);
            }

            return groups;
        }

        public static List<TransportJobData> GetSubJobs(List<SectionGroup> updatedGuideJobs, int guideId, int index)
        {
            var subJobs = new List<TransportJobData>();

            if (index >= updatedGuideJobs.Count)
            {
                return subJobs;
            }

            foreach (var subJob in updatedGuideJobs[index].ChildElements)
            {
                bool isIncluded = subJob.GetAttribute("class").Contains("included");
                var titleElement = subJob.FindElement(By.ClassName("title"));
                var subJobName = titleElement.Text?.Trim();
                var subJobId = subJob.GetAttribute("data-id") ?? subJob.GetAttribute("id");
                var subJobTime = isIncluded ? "+0,00" : subJob.FindElement(By.CssSelector("span.time")).Text?.Trim();

                if (!string.IsNullOrEmpty(subJobId) && subJobName?.Contains(subJobId) == true)
                {
                    subJobName = subJobName.Replace(subJobId, "").Trim();
                }

                subJobs.Add(new TransportJobData(
                    isIncluded ? "Включая" : "Следующее",
                    guideId,
                    subJobId,
                    subJobName,
                    subJobTime));
            }

            return subJobs;
        }
    }

    // Вспомогательная модель
    public class SectionGroup
    {
        public IWebElement ActiveElement { get; set; }
        public List<IWebElement> ChildElements { get; set; }
    }
}
