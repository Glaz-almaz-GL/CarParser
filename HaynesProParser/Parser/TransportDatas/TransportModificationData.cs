using HaynesProParser.DataBase;
using HaynesProParser.DataBase.Elements;
using HaynesProParser.DataBase.Items;
using HaynesProParser.Factories;
using HaynesProParser.Managers.WebManagers;
using HaynesProParser.Services;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HaynesProParser.Parser.TransportDatas
{
    public enum ETransportType
    {
        Car,
        Truck,
        Motorcycle,
        Axle,
        Trailer
    }

    public class TransportModificationData
    {
        public ETransportType TransportType { get; set; }
        public string MakerId { get; set; }
        public int TransportId { get; set; }
        public string TransportSiteId { get; set; }
        public int dbId { get; set; }
        public string ModificationId { get; set; }
        public string Type { get; set; }
        public string Axle { get; set; }
        public string EngineCode { get; set; }
        public string Capacity { get; set; }
        public string Power { get; set; }
        public string Years { get; set; }
        public string FuelType { get; set; }
        public string BuildType { get; set; }
        public string Tonnage { get; set; }
        public (string LoadInfoPageLink, string GuidesLink) ModificationLink { get; set; }
        public List<TransportModificationQuickGuideTypeData> TransportModificationGuideTypesDatas { get; set; } = [];

        public TransportModificationQuickGuideTypeService TransportModificationGuideTypeService { get; set; }
        public DriverFactory ModificationFactory { get; set; }

        public TransportModificationData(ETransportType transportType, string makerId, int transportId, string transportSiteId, string modId, string type, string axle, string engineCode, string capacity, string power, string years, string fuelType, string buildType, string tonnage)
        {
            TransportType = transportType;
            MakerId = makerId;
            TransportId = transportId;
            TransportSiteId = transportSiteId;
            ModificationId = modId;
            Type = type;
            Axle = axle;
            EngineCode = engineCode;
            Capacity = capacity;
            Power = power;
            Years = years;
            FuelType = fuelType;
            BuildType = buildType;
            Tonnage = tonnage;
            ModificationLink = GetTransportLink();

            Debug.WriteLine($"Transport Modification Data [MakerId: {MakerId}; TransportSiteId: {TransportSiteId}; ModificationId: {ModificationId}; Type: {Type}; Axle: {Axle}; EngineCode: {EngineCode}; Capacity: {Capacity}; Power: {Power}; ModelYear: {Years}; BuildType: {BuildType}; Tonnage: {Tonnage}]");
        }

        private async Task SaveToDatabaseAsync()
        {
            try
            {
                using var db = new AppDbContext();
                var existing = await db.Modifications.FirstOrDefaultAsync(m => m.SiteIdentifier == ModificationId);
                if (existing == null)
                {
                    var years = Years.Split("-");
                    int startYear = int.Parse(years[0]);
                    int? endYear = years[1].Contains("...") ? null : int.Parse(years[1]);

                    var dbMod = new dbModificationData
                    {
                        Name = Type,
                        SiteIdentifier = ModificationId,
                        ParentIdentifier = TransportSiteId,
                        ModelId = TransportId,
                        Axle = Axle,
                        EngineCode = EngineCode,
                        Capacity = Capacity,
                        Power = Power,
                        StartYear = startYear,
                        EndYear = endYear,
                        FuelTypeId = dbFuelTypes.GetFuelTypeId(FuelType),
                        BuildType = BuildType,
                        Tonnage = Tonnage
                    };

                    db.Modifications.Add(dbMod);

                    dbId = dbMod.Id + 1;

                    await db.SaveChangesAsync();
                    Debug.WriteLine($"[DB] Сохранена модификация: {Type} (ID: {ModificationId})");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Ошибка БД] Не удалось сохранить модификацию {ModificationId}: {ex.Message}");
            }
        }

        private (string loadInfoPageLink, string guidesLink) GetTransportLink()
        {
            return TransportType switch
            {
                ETransportType.Car => ($"https://www.workshopdata.com/touch/site/layout/modelDetail?typeId={ModificationId}&currentPage=QUICKGUIDES", $"https://www.workshopdata.com/touch/site/layout/repairTimes?typeId={ModificationId}&groupId=QUICKGUIDES"),
                ETransportType.Truck => ($"https://www.workshopdata.com/touch/site/layout/modelDetail?typeId={ModificationId}&currentPage=QUICKGUIDES", $"https://www.workshopdata.com/touch/site/layout/modelDetail?typeId={ModificationId}&axleSelectionSkiped=true&groupId=QUICKGUIDES"),
                ETransportType.Motorcycle => throw new NotImplementedException(),
                ETransportType.Axle => throw new NotImplementedException(),
                ETransportType.Trailer => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        public async Task GetModificationQuickGuidesDatasAsync((ChromeDriver, WebDriverWait) driverPool)
        {
            if (!string.IsNullOrEmpty(ModificationId) && ModificationId != "#" && !FuelType.Contains("disable"))
            {
                await SaveToDatabaseAsync();

                try
                {
                    await Task.Delay(5000);

                    ModificationFactory = new();
                    TransportModificationGuideTypeService = new(ModificationFactory);

                    await WebDriverManager.NavigateAsync(driverPool.Item1, ModificationLink.LoadInfoPageLink);
                    await WebDriverManager.NavigateAsync(driverPool.Item1, ModificationLink.GuidesLink);

                    TransportModificationGuideTypesDatas = await TransportModificationGuideTypeService.GetTypeGuidesAsync(MakerId, dbId + 1, TransportSiteId, ModificationId, driverPool);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при загрузке QuickGuides для модификации {ModificationId}: {ex.Message}");
                }
            }
        }
    }
}
