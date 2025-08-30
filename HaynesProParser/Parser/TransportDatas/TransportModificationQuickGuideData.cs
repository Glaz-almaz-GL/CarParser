using HaynesProParser.DataBase;
using HaynesProParser.DataBase.Items;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HaynesProParser.Parser.TransportDatas
{
    public class TransportModificationQuickGuideData
    {
        public string MakerId { get; set; }
        public string CarId { get; set; }
        public string ModificationId { get; set; }
        public int MainGuideId { get; set; }
        public string MainGuideSiteId { get; set; }
        public string Name { get; set; }
        public int dbId { get; set; }
        public string Id { get; set; }
        public string OECode { get; set; }
        public string Time { get; set; }
        public List<TransportJobData> SubJobDatas { get; set; }

        public TransportModificationQuickGuideData(
            string makerId, string carId, string modificationId, int mainGuideId, string mainGuideSiteId,
            string name, string id,
            string oeCode, string time, List<TransportJobData> subJobDatas)
        {
            MakerId = makerId;
            CarId = carId;
            ModificationId = modificationId;
            MainGuideId = mainGuideId;
            MainGuideSiteId = mainGuideSiteId;
            Name = name;
            Id = id;
            OECode = oeCode;
            SubJobDatas = subJobDatas;
            Time = time;
            SubJobDatas = subJobDatas;

            Console.ForegroundColor = ConsoleColor.Green;
            Debug.WriteLine($"QuickGuide: {Name} | QuickGuideId: {Id} | OE: {OECode} | QuickGuideTime: {Time}");
        }

        public async Task SaveToDatabaseAsync()
        {
            try
            {
                Debug.WriteLine($"DB MainGuide Id: {MainGuideId}");

                using var db = new AppDbContext();
                var existing = await db.Guides.FirstOrDefaultAsync(m => m.SiteIdentifier == Id);
                if (existing == null)
                {
                    var dbGuide = new dbGuideData
                    {
                        Name = Name,
                        SiteIdentifier = Id,
                        ParentIdentifier = MainGuideSiteId,
                        MainGuideId = MainGuideId,
                        OECode = OECode,
                        Time = Time
                    };
                    db.Guides.Add(dbGuide);

                    dbId = dbGuide.Id;

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Ошибка БД] Сохранение Guide: {ex.Message} {ex.InnerException}");
            }
        }
    }
}