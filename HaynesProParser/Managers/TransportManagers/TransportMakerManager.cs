using HaynesProParser.Managers.WebManagers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HaynesProParser.Managers.TransportManagers
{
    public static class TransportMakerManager
    {
        public static async Task<List<(string carName, string carId, string carYears, string imgUrl)>> GetTilesInfoAsync(string makerLink, string makerName, string transportType, (ChromeDriver, WebDriverWait) makerDriverPool)
        {
            List<Task> downloadTasks = [];

            List<(string carName, string carId, string carYears, string imgUrl)> models = [];
            await WebDriverManager.NavigateAsync(makerDriverPool.Item1, makerLink);

            if (makerDriverPool.Item1.Url != makerLink)
            {
                await WebDriverManager.NavigateAsync(makerDriverPool.Item1, makerLink);
            }

            IWebElement modelsDiv = makerDriverPool.Item1.FindElement(By.Id("models"));

            ReadOnlyCollection<IWebElement> modelTiles = modelsDiv.FindElements(By.ClassName("tile"));

            foreach (IWebElement tile in modelTiles)
            {
                IWebElement tileButton = tile.FindElement(By.TagName("a"));
                string transportName = tileButton.FindElement(By.TagName("p")).Text;
                string transportYears = tileButton.FindElement(By.TagName("span")).Text;

                string imgUrl = tile.FindElement(By.TagName("img")).GetAttribute("src");
                string transportId = WebElementsManager.GetLinkAttribute(tileButton.GetAttribute("href"), "modelGroupId");

                Task task = Task.Run(() => WebDownloadManager.DownloadFileAsync(imgUrl, transportName, folderNames: [transportType, makerName, transportName])); // Download model car img
                downloadTasks.Add(task);

                models.Add(new(transportName, transportId, transportYears, imgUrl));
            }
            await Task.WhenAll(downloadTasks);

            return models;
        }
    }
}