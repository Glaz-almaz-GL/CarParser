using HaynesProParser.DataBase;
using HaynesProParser.DataBase.Elements;
using HaynesProParser.DataBase.Items;
using HaynesProParser.Managers.WebManagers;
using HaynesProParser.Parser.TransportDatas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaynesProParser
{
    public partial class Form1 : Form
    {
        private readonly List<TransportTypeData> TransportTypeDatas = [];

        public Form1()
        {
            InitializeComponent();
            WebDriverManager.InitializeDriverCleanup();
        }

        public async Task Start()
        {
            TransportTypeDatas.Clear();

            WebDriverManager.ParallelMakersCount = 2;
            WebDriverManager.ParallelModelsCount = 1;
            WebDriverManager.ParallelModificationCount = 2;
            WebDriverManager.ParallelGuideTypesCount = 5;

            WebDriverManager.NavigateExceptionTimeOutMinutes = 5;
            WebDownloadManager.SortByFolders = false;

            //WebDriverManager.CreateCurrentDriver();
            //await WebDriverManager.DemoLoginAsync();

            await FindMakersAsync();
            //await FindMakersAsync(true);

            //CreateTransportDB();

            //CreateTransportNodes(true);

            //CurrentDriver.Driver?.Quit();
        }

        private void CreateTransportDB()
        {
            Debug.WriteLine("--=== Загрузка библиотек дял создания базы данных ===--");

            using var db = new AppDbContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            using var transaction = db.Database.BeginTransaction();

            Debug.WriteLine("--=== Начало создания базы данных транспорта ===--");

            try
            {
                int totalMakers = 0, totalModels = 0, totalModifications = 0;
                int totalGuideTypes = 0, totalMainGuides = 0, totalGuides = 0, totalJobs = 0;

                foreach (var transportTypeData in TransportTypeDatas)
                {
                    // 1. TransportType
                    var transportType = new dbTransportTypeData
                    {
                        Name = transportTypeData.Name
                    };
                    db.TransportTypes.Add(transportType);
                    db.SaveChanges();

                    Debug.WriteLine($"🔧 Обработка типа транспорта: {transportType.Name}");

                    // === Иерархический проход: Maker → Model → Modification → ... ===
                    foreach (var makerData in transportTypeData.TransportMakerDatas)
                    {
                        totalMakers++;
                        Debug.WriteLine($"[MAKER {totalMakers}] {makerData.MakerName} (ID: {makerData.MakerId})");

                        var dbMaker = new dbMakerData
                        {
                            ParentIdentifier = "car",
                            SiteIdentifier = makerData.MakerId,
                            Name = makerData.MakerName,
                            ImageLink = makerData.MakerImgLink,
                            TransportTypeId = transportType.Id
                        };
                        db.Makers.Add(dbMaker);
                        db.SaveChanges();

                        foreach (var modelData in makerData.TransportModelDatas)
                        {
                            totalModels++;
                            var years = modelData.TransportYears.Split('-');
                            bool isDiscontinued = years[1].Trim().Contains("...");
                            int? endYear = isDiscontinued ? null : int.Parse(years[1].Trim().Replace("...", ""));

                            var dbModel = new dbModelData
                            {
                                SiteIdentifier = modelData.TransportModelId,
                                ParentIdentifier = makerData.MakerId,
                                ModelGroupId = modelData.TransportId,
                                StoryId = modelData.TransportStoryId,
                                Name = modelData.TransportName,
                                StartYear = int.Parse(years[0].Trim()),
                                EndYear = endYear,
                                IsDiscontinued = isDiscontinued,
                                LocationIdLink = modelData.LocationIdLink,
                                DiagnosticConnectorLink = modelData.DiagnosticConnectorLink,
                                JackLocationLink = modelData.JackLocationLink,
                                MakerId = dbMaker.Id
                            };
                            db.Models.Add(dbModel);
                            db.SaveChanges();

                            foreach (var modData in modelData.TransportModificationDatas)
                            {
                                totalModifications++;
                                var modYears = modData.Years.Split('-');
                                bool modIsDiscontinued = string.IsNullOrEmpty(modYears[1].Trim()) || modYears[1].Trim().Contains("...");
                                int? modEndYear = modIsDiscontinued ? null : int.Parse(modYears[1].Trim());

                                var dbMod = new dbModificationData
                                {
                                    SiteIdentifier = modData.ModificationId,
                                    ParentIdentifier = modData.TransportSiteId,
                                    Name = modData.Type,
                                    EngineCode = modData.EngineCode,
                                    Capacity = modData.Capacity,
                                    Power = modData.Power,
                                    StartYear = int.Parse(modYears[0].Trim()),
                                    EndYear = modEndYear,
                                    IsDiscontinued = modIsDiscontinued,
                                    FuelTypeId = dbFuelTypes.GetFuelTypeId(modData.FuelType.Replace(" disabled", "").Trim()),
                                    ModelId = dbModel.Id
                                };
                                db.Modifications.Add(dbMod);
                                db.SaveChanges();

                                foreach (var gtData in modData.TransportModificationGuideTypesDatas)
                                {
                                    totalGuideTypes++;
                                    var dbGt = new dbGuideTypeData
                                    {
                                        SiteIdentifier = gtData.LevelId,
                                        ParentIdentifier = modData.ModificationId,
                                        Name = gtData.Name,
                                        ModificationId = dbMod.Id
                                    };
                                    db.GuideTypes.Add(dbGt);
                                    db.SaveChanges();

                                    foreach (var mgData in gtData.MainQuickGuideDatas)
                                    {
                                        totalMainGuides++;
                                        var dbMg = new dbMainGuideData
                                        {
                                            SiteIdentifier = mgData.Id,
                                            ParentIdentifier = gtData.LevelId,
                                            Name = mgData.Name,
                                            GuideTypeId = dbGt.Id
                                        };
                                        db.MainGuides.Add(dbMg);
                                        db.SaveChanges();

                                        foreach (var gData in mgData.TransportModificationQuickGuidesDatas)
                                        {
                                            totalGuides++;
                                            var dbG = new dbGuideData
                                            {
                                                SiteIdentifier = gData.Id,
                                                ParentIdentifier = mgData.Id,
                                                Name = gData.Name,
                                                OECode = gData.OECode,
                                                Time = gData.Time,
                                                MainGuideId = dbMg.Id
                                            };
                                            db.Guides.Add(dbG);
                                            db.SaveChanges();

                                            foreach (var jData in gData.SubJobDatas)
                                            {
                                                totalJobs++;
                                                var dbJ = new dbJobData
                                                {
                                                    SiteIdentifier = jData.Id,
                                                    ParentIdentifier = gData.Id,
                                                    Name = jData.Name,
                                                    Type = jData.Type,
                                                    Time = jData.Time,
                                                    GuideId = dbG.Id
                                                };
                                                db.Jobs.Add(dbJ);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Сохраняем все работы
                    if (totalJobs > 0)
                    {
                        db.SaveChanges();
                    }

                    transaction.Commit();

                    Debug.WriteLine("=== Создание базы данных завершено успешно! ===");
                    Debug.WriteLine($"📊 Итого: {totalMakers} производителей, {totalModels} моделей, {totalModifications} модификаций, " +
                                   $"{totalGuideTypes} типов руководств, {totalMainGuides} основных, {totalGuides} руководств, {totalJobs} работ");

                    Debug.WriteLine($"Кол-во остановок: {WebDriverManager.TimeOutCount}");
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine($"❌ Ошибка при создании БД: {ex.Message}");
                Debug.WriteLine($"📌 Стек: {ex.StackTrace}");
                throw;
            }
        }

        private async Task FindMakersAsync()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            TransportTypeData transportTypeData = new(ETransportType.Car, "Car");
            await transportTypeData.GetMakers();

            stopwatch.Stop();
            Debug.WriteLine($"Времени на обработку всех марок (учитывая паузы) ушло: {stopwatch.Elapsed.TotalMinutes:F2}");
            Debug.WriteLine($"Времени на обработку всех марок (не учитывая паузы) ушло: {stopwatch.Elapsed.TotalMinutes - (15 * WebDriverManager.TimeOutCount):F2}");
            Debug.WriteLine($"Максимальное кол-во брендов для параллельной обработки: {WebDriverManager.ParallelMakersCount}");
            Debug.WriteLine($"Максимальное кол-во моделей для параллельной обработки: {WebDriverManager.ParallelModelsCount}");
            Debug.WriteLine($"Максимальное кол-во модификаций для параллельной обработки: {WebDriverManager.ParallelModificationCount}");
            Debug.WriteLine($"Максимальное кол-во драйверов на 1 модификацию: {WebDriverManager.ParallelGuideTypesCount}");

            //TransportTypeData truckTypeData = new(ETransportType.Truck, "Truck");
            //await truckTypeData.GetMakers();

            TransportTypeDatas.Add(transportTypeData);
        }

        private async void btStart_Click(object sender, EventArgs e)
        {
            btStart.Enabled = false;

            await Task.Run(Start);

            btStart.Enabled = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            WebDriverManager.Quit();
        }
    }
}
