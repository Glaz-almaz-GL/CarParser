using HaynesProParser.DataBase;
using HaynesProParser.DataBase.Elements;
using HaynesProParser.DataBase.Items;
using HaynesProParser.Managers.TransportManagers;
using HaynesProParser.Managers.WebManagers;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HaynesProParser.Parser.TransportDatas
{
    public class TransportMakerData
    {
        public ETransportType TransportType { get; set; }
        public string MakerName { get; set; }
        public int dbId { get; set; }
        public string MakerId { get; set; }
        public string MakerImgLink { get; set; }
        public string MakerLink { get; set; }
        public List<TransportModelData> TransportModelDatas { get; set; } = [];

        public (ChromeDriver, WebDriverWait) MakerDriverPool { get; set; }

        public TransportMakerData(ETransportType transportType, string makerName, string makerId, string makerImgLink)
        {
            TransportType = transportType;
            MakerId = makerId;
            MakerName = makerName;
            MakerImgLink = makerImgLink;
            MakerLink = GetLink();

            Console.ForegroundColor = ConsoleColor.Blue;
            Debug.WriteLine($"TRANSPORT Maker Data [TransportType: {TransportType} Id: {MakerId}; Name: {MakerName}; ImgLink: {MakerImgLink}; Link: {MakerLink}]");

            SaveToDatabase();
        }

        private void SaveToDatabase()
        {
            try
            {
                using var db = new AppDbContext(); // Используем общий контекст
                var existing = db.Makers.FirstOrDefault(m => m.SiteIdentifier == MakerId);
                if (existing == null)
                {
                    int transportTypeId = dbTransportTypes.GetTransportTypeId(TransportType.ToString());
                    Debug.WriteLine($"Transprot Type ID: {transportTypeId}");

                    var dbMaker = new dbMakerData
                    {
                        Name = MakerName,
                        SiteIdentifier = MakerId,
                        TransportTypeId = transportTypeId, // Убедитесь, что это внешний ключ
                        ParentIdentifier = TransportType.ToString(),
                        ImageLink = MakerImgLink

                    };
                    db.Makers.Add(dbMaker);

                    dbId = dbMaker.Id + 1;

                    db.SaveChanges();
                    Debug.WriteLine($"[DB] Сохранён производитель: {MakerName} (ID: {MakerId})");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Ошибка БД] Не удалось сохранить производителя {MakerName}: {ex.Message} {ex.InnerException}");
            }
        }

        private string GetLink()
        {
            return TransportType switch
            {
                ETransportType.Car => $"https://www.workshopdata.com/touch/site/layout/modelOverview?makeId={MakerId}#allowed",
                ETransportType.Truck => $"https://www.workshopdata.com/touch/site/layout/modelOverviewTrucks?makeId={MakerId}#allowed",
                ETransportType.Motorcycle => throw new NotImplementedException(),
                ETransportType.Axle => throw new NotImplementedException(),
                ETransportType.Trailer => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        public async Task GetTransportDatasAsync()
        {
            if (string.IsNullOrEmpty(MakerId) || MakerId == "#")
            {
                return;
            }

            MakerDriverPool = WebDriverManager.CreateAnotherDriver(true);
            await WebDriverManager.DemoLoginAsync(MakerDriverPool.Item1, MakerDriverPool.Item2);

            List<(string carName, string carId, string carYears, string imgUrl)> models =
                await TransportMakerManager.GetTilesInfoAsync(MakerLink, MakerName, TransportType.ToString(), MakerDriverPool);

            // Ограничение: только 3 задачи одновременно
            var semaphore = new SemaphoreSlim(WebDriverManager.ParallelModelsCount, WebDriverManager.ParallelModelsCount);
            var tasks = new List<Task>();

            foreach ((string transportName, string transportId, string transportYears, string imgUrl) in models)
            {
                // Захватываем значения, чтобы избежать захвата переменной в замыкании
                var localTransportName = transportName;
                var localTransportId = transportId;
                var localTransportYears = transportYears;
                var localImgUrl = imgUrl;

                Task task = Task.Run(async () =>
                {
                    await semaphore.WaitAsync(); // Ждём разрешения запустить задачу
                    try
                    {
                        var stopwatch = Stopwatch.StartNew();

                        TransportModelData transportModel = new(
                            TransportType,
                            dbId,
                            MakerName,
                            MakerId,
                            localTransportName,
                            localTransportId,
                            localTransportYears,
                            localImgUrl
                        );

                        await transportModel.GetTransportModificationsAsync(); // Асинхронный вызов

                        stopwatch.Stop();

                        if (!string.IsNullOrEmpty(localTransportId) && localTransportId != "#")
                        {
                            Debug.WriteLine($"Времени на модель: {localTransportName} ({localTransportId}) ушло: {stopwatch.Elapsed.TotalMinutes:F2} мин");
                        }

                        // Добавляем модель в общий список ТОЛЬКО после успешной обработки
                        lock (TransportModelDatas) // потокобезопасность, если нужно
                        {
                            TransportModelDatas.Add(transportModel);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при обработке модели {localTransportName}: {ex.Message}");
                        // Можно добавить логирование
                    }
                    finally
                    {
                        semaphore.Release(); // Освобождаем слот
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks); // Ждём завершения всех задач

            // Закрываем драйвер после всех операций
            MakerDriverPool.Item1.Quit();
        }
    }
}
