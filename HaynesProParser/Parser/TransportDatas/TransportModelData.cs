using HaynesProParser.DataBase;
using HaynesProParser.DataBase.Items;
using HaynesProParser.Managers.TransportManagers;
using HaynesProParser.Managers.WebManagers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HaynesProParser.Parser.TransportDatas
{
    public class TransportModelData
    {
        public ETransportType TransportType { get; set; }
        public string MakerName { get; set; }
        public int MakerId { get; set; }
        public string MakerSiteId { get; set; }
        public int dbId { get; set; }
        public string TransportName { get; set; }
        public string TransportId { get; set; }
        public string TransportStoryId { get; set; }
        public string TransportModelId { get; set; }
        public string TransportYears { get; set; }
        public string TransportImageLink { get; set; }
        public string TransportDescriptionLink { get; set; }
        public string LocationIdLink { get; set; }
        public string JackLocationLink { get; set; }
        public string DiagnosticConnectorLink { get; set; }

        public List<TransportModificationData> TransportModificationDatas { get; set; } = [];

        public (ChromeDriver, WebDriverWait) ModelDriverPool { get; set; }

        public TransportModelData(ETransportType transportType, int makerId, string makerName, string makerSiteId, string transportName, string transportId, string transportYears, string transportImageLink)
        {
            TransportType = transportType;
            MakerId = makerId;
            MakerName = makerName;
            MakerSiteId = makerSiteId;
            TransportName = transportName;
            TransportId = transportId;
            TransportYears = transportYears;
            TransportImageLink = transportImageLink;
            TransportDescriptionLink = GetLink();

            Debug.WriteLine($"[Transport Model Data] TransportType: {transportType}, MakerName: {makerName}, MakerSiteId: {makerSiteId}, TransportName: {transportName}, TransportId: {transportId}, TransportYears: {transportYears}, TramsportImgLink: {transportImageLink}");

            SaveToDatabase();
        }

        private void SaveToDatabase()
        {
            try
            {
                using var db = new AppDbContext();
                var existing = db.Models.FirstOrDefault(m => m.SiteIdentifier == TransportId);
                if (existing == null)
                {
                    var years = TransportYears.Split("-");
                    int startYear = int.Parse(years[0]);
                    int? endYear = years[1].Contains("...") ? null : int.Parse(years[1]);

                    Debug.WriteLine($"DB Maker Id: {MakerId}");

                    var dbModel = new dbModelData
                    {
                        Name = TransportName,
                        SiteIdentifier = TransportId,
                        ParentIdentifier = MakerSiteId,
                        MakerId = MakerId,
                        StoryId = TransportStoryId,
                        StartYear = startYear,
                        EndYear = endYear,
                        ImageLink = TransportImageLink,
                        LocationIdLink = LocationIdLink,
                        JackLocationLink = JackLocationLink,
                        DiagnosticConnectorLink = DiagnosticConnectorLink
                    };

                    db.Models.Add(dbModel);

                    dbId = dbModel.Id + 1;

                    db.SaveChanges();
                    Debug.WriteLine($"[DB] Сохранена модель: {TransportName} (ID: {TransportId})");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Ошибка БД] Не удалось сохранить модель {TransportName}: {ex.Message} {ex.InnerException}");
            }
        }

        private string GetLink()
        {
            return TransportType switch
            {
                ETransportType.Car => $"https://www.workshopdata.com/touch/site/layout/modelTypes?modelGroupId={TransportId}&makeId={MakerSiteId}#allowed",
                ETransportType.Truck => $"https://www.workshopdata.com/touch/site/layout/modelTypesTrucks?modelGroupId={TransportId}&makeId={MakerSiteId}#allowed",
                _ => $"https://www.workshopdata.com/touch/site/layout/modelTypes?modelGroupId={TransportId}&makeId={MakerSiteId}#allowed"
            };
        }

        public async Task GetTransportModificationsAsync()
        {
            if (string.IsNullOrEmpty(TransportId) || TransportId == "#")
            {
                return;
            }

            List<Task> tasks = [];
            List<Task> downloadTasks = [];

            // Очистка предыдущих данных
            TransportModificationDatas.Clear();

            ModelDriverPool = WebDriverManager.CreateAnotherDriver(true);
            await WebDriverManager.DemoLoginAsync(ModelDriverPool.Item1, ModelDriverPool.Item2);

            await WebDriverManager.NavigateAsync(ModelDriverPool.Item1, TransportDescriptionLink);
            if (ModelDriverPool.Item1.Url != TransportDescriptionLink)
            {
                await WebDriverManager.NavigateAsync(ModelDriverPool.Item1, TransportDescriptionLink);
            }

            // Получение storyId и modelId
            try
            {
                IWebElement locationIdBt = ModelDriverPool.Item1.FindElement(By.ClassName("id-location-btn"));
                string locationId = locationIdBt?.GetAttribute("href")?.Trim();
                TransportStoryId = WebElementsManager.GetLinkAttribute(locationId, "storyId");
                TransportModelId = WebElementsManager.GetLinkAttribute(locationId, "modelId");
            }
            catch (NoSuchElementException ex)
            {
                Debug.WriteLine($"LocationId Error: {ex.Message}");
            }

            // Формирование ссылок
            LocationIdLink = $"https://www.workshopdata.com/touch/site/layout/repairManualsPrint?modelId={TransportModelId}&storyId={TransportStoryId}&groupId=QUICKGUIDES&extraInfo=true";
            JackLocationLink = $"https://www.workshopdata.com/touch/site/layout/jackingPointsPrint?modelId={TransportModelId}";
            DiagnosticConnectorLink = $"https://www.workshopdata.com/touch/site/layout/eobdConnectorLocationsPrint?modelId={TransportModelId}";

            Debug.WriteLine($"LocationIdLink: {LocationIdLink}; JackLocationLink: {JackLocationLink}; DiagnosticConnectorLink: {DiagnosticConnectorLink}");

            ReadOnlyCollection<Cookie> cookies = ModelDriverPool.Item1.Manage().Cookies.AllCookies;

            // Задачи на скачивание PDF (всего 3 — можно запустить параллельно)
            downloadTasks.AddRange(
            [
        Task.Run(() => WebDownloadManager.DownloadFileAsync(LocationIdLink, "Location Id", ".pdf", new[] { TransportType.ToString(), MakerName, TransportName }, cookies)),
        Task.Run(() => WebDownloadManager.DownloadFileAsync(JackLocationLink, "Jack Location", ".pdf", new[] { TransportType.ToString(), MakerName, TransportName }, cookies)),
        Task.Run(() => WebDownloadManager.DownloadFileAsync(DiagnosticConnectorLink, "Diagnostic Connector", ".pdf", new[] { TransportType.ToString(), MakerName, TransportName }, cookies))
    ]);

            // === Ограничение: максимум 3 одновременные задачи ===
            var semaphore = new SemaphoreSlim(WebDriverManager.ParallelModificationCount, WebDriverManager.ParallelModificationCount);

            switch (TransportType)
            {
                case ETransportType.Car:
                    List<object> carRows = TransportModelManager.GetRowsInfo(ModelDriverPool);
                    foreach (CarTempRowData row in carRows?.Cast<CarTempRowData>() ?? Enumerable.Empty<CarTempRowData>())
                    {
                        var localRow = row; // Защита от замыкания

                        Task task = Task.Run(async () =>
                        {
                            await semaphore.WaitAsync();
                            try
                            {
                                TransportModificationData mod = new(
                                    TransportType,
                                    MakerSiteId,
                                    dbId,
                                    TransportId,
                                    localRow.ModId,
                                    localRow.ModType,
                                    string.Empty,
                                    localRow.EngineCode,
                                    localRow.Capacity,
                                    localRow.Power,
                                    localRow.ModelYears,
                                    localRow.FuelType,
                                    string.Empty,
                                    string.Empty
                                );

                                await mod.GetModificationQuickGuidesDatasAsync(ModelDriverPool);

                                lock (TransportModificationDatas)
                                {
                                    TransportModificationDatas.Add(mod);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Ошибка при обработке модификации (Car): {localRow.ModId} — {ex.Message}");
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });

                        tasks.Add(task);
                    }
                    break;

                case ETransportType.Truck:
                    List<object> truckRows = TransportModelManager.GetRowsInfo(ModelDriverPool, true);
                    foreach (TruckTempRowData row in truckRows?.Cast<TruckTempRowData>() ?? Enumerable.Empty<TruckTempRowData>())
                    {
                        var localRow = row;

                        Task task = Task.Run(async () =>
                        {
                            await semaphore.WaitAsync();
                            try
                            {
                                TransportModificationData mod = new(
                                    TransportType,
                                    MakerSiteId,
                                    dbId,
                                    TransportId,
                                    localRow.ModId,
                                    localRow.ModType,
                                    localRow.Axle,
                                    localRow.EngineCode,
                                    localRow.Capacity,
                                    localRow.Power,
                                    localRow.ModelYears,
                                    string.Empty,
                                    localRow.BuildType,
                                    localRow.Tonnage
                                );

                                await mod.GetModificationQuickGuidesDatasAsync(ModelDriverPool);

                                lock (TransportModificationDatas)
                                {
                                    TransportModificationDatas.Add(mod);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Ошибка при обработке модификации (Truck): {localRow.ModId} — {ex.Message}");
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });

                        tasks.Add(task);
                    }
                    break;
            }

            // Запускаем все задачи параллельно, но с ограничением
            await Task.WhenAll(tasks);
            await Task.WhenAll(downloadTasks);

            // Закрываем драйвер после завершения
            ModelDriverPool.Item1.Quit();
        }
    }
}
