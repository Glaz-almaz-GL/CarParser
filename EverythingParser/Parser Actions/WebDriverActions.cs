using OpenQA.Selenium;
using Selenium.WebDriver.EventCapture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EverythingParser.ParserActions
{
    public static class WebDriverActions
    {
        public static string GetBaseUrl(string url)
        {
            Uri uri = null;

            if (!string.IsNullOrEmpty(url))
            {
                uri = new Uri(url);
            }
            else
            {
                return string.Empty;
            }

            string baseUrl = uri.GetLeftPart(UriPartial.Path);

            return baseUrl;
        }

        public static void InjectJavaScriptForNavigationHandling(EventCapturingWebDriver CaptureDriver)
        {
            // Внедрение JavaScript для управления переходами по ссылкам

            if (CaptureDriver.Url.StartsWith("data:"))
            {
                Console.WriteLine("URL начинается с 'data:', скрипт не внедряется.");
                return;
            }

            string script = @"
    (function() {
        if (!window.jsMessages) {
            window.jsMessages = [];
        }

        function checkNavigation() {
            var shouldCancelNavigation = localStorage.getItem('shouldCancelNavigation') === 'true';
            var elements = document.querySelectorAll('[href], [data-url], [onclick], [action]');
            for (var i = 0; i < elements.length; i++) {
                elements[i].removeEventListener('click', clickHandler);
                elements[i].addEventListener('click', clickHandler);
            }

            // Обработка кликов на строках таблицы
            $('table').off('click', 'tr:not(.disabled)');
            $('table').on('click', 'tr:not(.disabled)', function (event) {
                if (!$(this).hasClass('heading')) {
                    var url = $(this).attr('data-url');
                    if (url !== '' && url !== '#') {
                        if (shouldCancelNavigation) {
                            event.preventDefault();
                            console.log('Переход по ссылке отменен.');
                        } else {
                            console.log('Переход по ссылке разрешен.');
                        }
                    }
                }
            });
        }

        function clickHandler(event) {
            var shouldCancelNavigation = localStorage.getItem('shouldCancelNavigation') === 'true';
            if (shouldCancelNavigation) {
                // Проверяем текущий элемент и его предков на наличие атрибута data-url
                var targetElement = event.target;
                var closestWithDataURL = targetElement.closest('[href], [data-url], [onclick], [action]');
                if (closestWithDataURL) {
                    event.preventDefault();
                    console.log('Переход по ссылке отменен.');
                    return;
                }
            }
            console.log('Переход по ссылке разрешен.');
        }

        // Используем MutationObserver для отслеживания изменений в DOM
        var observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                if (mutation.type === 'childList') {
                    checkNavigation();
                }
            });
        });

        observer.observe(document.body, { childList: true, subtree: true });

        // Отслеживание события beforeunload для отправки сообщения в C#
        window.addEventListener('beforeunload', function() {
            window.jsMessages.push('navigationOccurred');
        });

        // Вызов checkNavigation сразу после внедрения скрипта
        checkNavigation();
    })();
";
            IJavaScriptExecutor js = (IJavaScriptExecutor)CaptureDriver;
            js.ExecuteScript(script);

            Console.WriteLine("Скрипт внедрён!");
        }

        public static List<string> GetAllAttributeValues(IWebElement element, string attributeName)
        {
            List<string> attributeValues = new List<string>();
            CollectAttributeValues(element, attributeName, attributeValues);
            return attributeValues;
        }

        private static void CollectAttributeValues(IWebElement element, string attributeName, List<string> attributeValues)
        {
            string attributeValue = element.GetAttribute(attributeName);

            if (attributeName == "Text")
            {
                attributeValue = WebDriverActions.GetElementText(element);
            }

            if (!string.IsNullOrEmpty(attributeValue))
            {
                attributeValues.Add(attributeValue);
            }

            foreach (IWebElement child in element.FindElements(By.XPath("./*")))
            {
                CollectAttributeValues(child, attributeName, attributeValues);
            }
        }

        public static string GetFullXPath(IWebElement element)
        {
            if (element == null)
            {
                return string.Empty;
            }

            string tagName = element.TagName;
            string path = tagName;

            if (tagName.Equals("html"))
            {
                return path;
            }

            IWebElement parent = element.FindElement(By.XPath(".."));
            List<IWebElement> siblings = parent.FindElements(By.XPath(tagName)).ToList();
            if (siblings.Count > 1)
            {
                int index = siblings.IndexOf(element) + 1;
                path += $"[{index}]";
            }
            return $"{GetFullXPath(parent)}/{path}";
        }

        public static string GetElementText(IWebElement element)
        {
            string elementText = element.Text;

            // Если текст пустой, пробуем найти текст среди дочерних элементов
            if (string.IsNullOrEmpty(elementText))
            {
                var childElements = element.FindElements(By.XPath(".//*")); // Находим все дочерние элементы
                foreach (var childElement in childElements)
                {
                    elementText = childElement.Text;
                    if (!string.IsNullOrEmpty(elementText))
                    {
                        break; // Останавливаемся после первого найденного непустого текста
                    }
                }
            }

            // Разделяем текст на строки и возвращаем первую строку
            if (!string.IsNullOrEmpty(elementText))
            {
                string[] lines = elementText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    return lines[0].Trim(); // Возвращаем первую строку, удаляя лишние пробелы
                }
            }

            return string.Empty; // Возвращаем пустую строку, если ничего не нашли
        }

        public static int FindLastNumberIndex(string[] parts)
        {
            int lastNumberIndex = -1;
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Contains("[") && parts[i].Contains("]"))
                {
                    lastNumberIndex = i;
                }
            }
            return lastNumberIndex;
        }

        public static int FindSecondLastNumberIndex(string[] parts)
        {
            int secondLastNumberIndex = -1;
            int lastNumberIndex = -1;

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Contains("[") && parts[i].Contains("]"))
                {
                    secondLastNumberIndex = lastNumberIndex;
                    lastNumberIndex = i;
                }
            }

            return secondLastNumberIndex;
        }
    }
}
