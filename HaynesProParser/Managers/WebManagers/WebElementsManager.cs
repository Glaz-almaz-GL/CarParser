using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace HaynesProParser.Managers.WebManagers
{
    public static class WebElementsManager
    {
        public static string GetLinkAttribute(string hrefValue, string attribute)
        {
            Uri uri = new(hrefValue);

            string query = uri.Query;
            NameValueCollection queryParams = HttpUtility.ParseQueryString(query);

            return queryParams[attribute];
        }

        public class SectionGroup(IWebElement activeElement)
        {
            public IWebElement ActiveElement { get; set; } = activeElement;
            public List<IWebElement> ChildElements { get; set; } = [];
        }

        public static List<SectionGroup> GroupItemsByActiveSections(IEnumerable<IWebElement> allLiElements)
        {
            List<SectionGroup> sections = [];
            SectionGroup currentSection = null;

            foreach (IWebElement li in allLiElements)
            {
                string dataAwnr = li.GetAttribute("data-awnr");
                string className = li.GetAttribute("class");

                bool isActive = !string.IsNullOrEmpty(dataAwnr) && !className.Contains("include") && !className.Contains("followup");

                if (isActive)
                {
                    // Сохраняем предыдущую секцию, если она существует
                    if (currentSection != null)
                    {
                        sections.Add(currentSection);
                    }
                    // Создаем новую секцию с активным элементом
                    currentSection = new SectionGroup(li);
                }
                else
                {
                    // Добавляем элемент как дочерний в текущую секцию
                    currentSection?.ChildElements.Add(li);
                }

            }

            if (currentSection != null)
            {
                sections.Add(currentSection);
            }

            return sections;
        }
    }
}
