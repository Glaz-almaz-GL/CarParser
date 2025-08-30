using HaynesProParser.DataBase;
using HaynesProParser.DataBase.Items;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HaynesProParser.Parser.TransportDatas
{
    public class TransportModificationMainQuickGuideData
    {
        public string MakerId { get; set; }
        public string CarId { get; set; }
        public string ModificationId { get; set; }
        public int dbId { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public int GuideTypeId { get; set; }
        public string GuideType { get; set; }
        public List<TransportModificationQuickGuideData> TransportModificationQuickGuidesDatas { get; set; }

        public TransportModificationMainQuickGuideData(string makerId, string carId, string modificationId, string name, string id, int guideTypeId, string guideType, List<TransportModificationQuickGuideData> carModificationQuickGuidesDatas)
        {
            MakerId = makerId;
            CarId = carId;
            ModificationId = modificationId;
            Name = name;
            GuideTypeId = guideTypeId;
            Id = id;
            GuideType = guideType;
            TransportModificationQuickGuidesDatas = carModificationQuickGuidesDatas;
        }

        public async Task SaveToDatabaseAsync()
        {
            try
            {
                await using var db = new AppDbContext();
                var existing = await db.MainGuides.FirstOrDefaultAsync(m => m.SiteIdentifier == Id);
                if (existing == null)
                {
                    var dbMainGuide = new dbMainGuideData
                    {
                        Name = Name,
                        SiteIdentifier = Id,
                        ParentIdentifier = GuideType,
                        GuideTypeId = GuideTypeId
                    };
                    db.MainGuides.Add(dbMainGuide);

                    dbId = dbMainGuide.Id;

                    await db.SaveChangesAsync();
                    Debug.WriteLine($"[DB] Сохранён главный гайд: {Name} (ID: {Id})");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Ошибка БД] Не удалось сохранить главный гайд {Name}: {ex.Message} {ex.InnerException}");
            }
        }
    }
}
