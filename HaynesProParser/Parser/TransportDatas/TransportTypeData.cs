using HaynesProParser.DataBase;
using HaynesProParser.DataBase.Elements;
using HaynesProParser.DataBase.Items;
using HaynesProParser.Managers.WebManagers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HaynesProParser.Parser.TransportDatas
{
    public class TransportTypeData
    {
        private ETransportType TransportType { get; set; }
        public string Name { get; set; }

        public List<TransportMakerData> TransportMakerDatas { get; set; } = [];

        public (ChromeDriver, WebDriverWait) TransportTypeDriverPool { get; set; }

        public TransportTypeData(ETransportType transportType, string name)
        {
            TransportType = transportType;
            Name = name;
            TransportTypeDriverPool = WebDriverManager.CreateAnotherDriver(true);
            WebDriverManager.DemoLogin(TransportTypeDriverPool.Item1, TransportTypeDriverPool.Item2);

            SaveToDatabase();
        }

        private void SaveToDatabase()
        {
            try
            {
                using var db = new AppDbContext();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                int tranportTypeId = dbTransportTypes.GetTransportTypeId(TransportType.ToString());
                var existing = db.TransportTypes.Find(tranportTypeId);
                if (existing == null)
                {
                    var dbTransportType = new dbTransportTypeData
                    {
                        Id = tranportTypeId,
                        Name = Name
                    };
                    db.TransportTypes.Add(dbTransportType);
                    db.SaveChanges();
                    Debug.WriteLine($"[DB] Сохранён тип транспорта: {Name} ({TransportType})");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Ошибка БД] Не удалось сохранить тип транспорта {Name}: {ex.Message}");
            }
        }

        private string GetLink()
        {
            return TransportType switch
            {
                ETransportType.Car => "https://www.workshopdata.com/touch/site/layout/makesOverview",
                ETransportType.Truck => "https://www.workshopdata.com/touch/site/layout/makesOverviewTrucks",
                ETransportType.Motorcycle => throw new NotImplementedException(),
                ETransportType.Axle => throw new NotImplementedException(),
                ETransportType.Trailer => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        private string GetElementId()
        {
            return TransportType switch
            {
                ETransportType.Car => "makes",
                ETransportType.Truck => "makeTrucks",
                ETransportType.Motorcycle => throw new NotImplementedException(),
                ETransportType.Axle => throw new NotImplementedException(),
                ETransportType.Trailer => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        public async Task GetMakers()
        {
            List<(string makerName, string makerId, string imgUrl)> makersTemp = [];
            TransportMakerDatas.Clear();

            // Определяем URL в зависимости от типа транспорта
            string url = GetLink();

            await WebDriverManager.NavigateAsync(TransportTypeDriverPool.Item1, url);

            // Определяем ID элемента в зависимости от типа транспорта
            string makesElementId = GetElementId();
            IWebElement makers = TransportTypeDriverPool.Item2.Until(d => d.FindElement(By.Id(makesElementId)));
            ReadOnlyCollection<IWebElement> makerTiles = makers.FindElements(By.ClassName("tile"));

            List<Task> downloadTasks = [];

            foreach (IWebElement tile in makerTiles)
            {
                IWebElement tileButton = tile.FindElement(By.TagName("a"));

                string makerName = tileButton.Text;
                string makerId = WebElementsManager.GetLinkAttribute(tileButton.GetAttribute("href"), "makeId");
                string imgUrl = tile.FindElement(By.TagName("img")).GetAttribute("src");

                string[] folderNames = [TransportType.ToString(), makerName];
                Task task = Task.Run(() => WebDownloadManager.DownloadFileAsync(imgUrl, makerName, folderNames: folderNames));
                downloadTasks.Add(task);

                makersTemp.Add((makerName, makerId, imgUrl));
            }

            // Ожидаем завершения загрузки изображений
            await Task.WhenAll(downloadTasks);

            // === Параллельный запуск GetTransportDatasAsync с лимитом в 3 задачи ===
            var semaphore = new SemaphoreSlim(WebDriverManager.ParallelMakersCount, WebDriverManager.ParallelMakersCount); // максимум 3 одновременные задачи
            var tasks = new List<Task>();

            foreach (var (makerName, makerId, imgUrl) in makersTemp)
            {
                // Захватываем переменные, чтобы избежать проблем с замыканием
                var localMakerName = makerName;
                var localMakerId = makerId;
                var localImgUrl = imgUrl;

                Task task = Task.Run(async () =>
                {
                    await semaphore.WaitAsync(); // Ждём разрешения на запуск
                    try
                    {
                        TransportMakerData maker = null;
                        switch (TransportType)
                        {
                            case ETransportType.Car:
                                maker = new(ETransportType.Car, localMakerName, localMakerId, localImgUrl);
                                break;
                            case ETransportType.Truck:
                                maker = new(ETransportType.Truck, localMakerName, localMakerId, localImgUrl);
                                break;
                        }

                        if (maker != null)
                        {

                            TransportMakerDatas.Add(maker);
                            await maker.GetTransportDatasAsync(); // Выполняем асинхронно
                        }
                    }
                    finally
                    {
                        semaphore.Release(); // Освобождаем место
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks); // Ждём завершения всех задач
        }
    }
}
