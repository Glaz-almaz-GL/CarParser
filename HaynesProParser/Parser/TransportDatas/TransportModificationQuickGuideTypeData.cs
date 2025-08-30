using HaynesProParser.DataBase;
using HaynesProParser.DataBase.Items;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HaynesProParser.Parser.TransportDatas
{
    public class TransportModificationQuickGuideTypeData
    {
        public string Name { get; set; }
        public int ModId { get; set; }
        public string ModSiteId { get; set; }
        public int dbId { get; set; }
        public string LevelId { get; set; }
        public string GuideTypeLink { get; set; }
        public List<TransportModificationMainQuickGuideData> MainQuickGuideDatas { get; set; }

        public TransportModificationQuickGuideTypeData(string name, int modId, string modSiteId, string levelId, string guideTypeLink, List<TransportModificationMainQuickGuideData> mainQuickGuideDatas)
        {
            Name = name;
            ModId = modId;
            ModSiteId = modSiteId;
            LevelId = levelId;
            GuideTypeLink = guideTypeLink;
            MainQuickGuideDatas = mainQuickGuideDatas;
        }

        public async Task SaveToDatabaseAsync()
        {
            try
            {
                using var db = new AppDbContext();
                var existing = await db.GuideTypes.FirstOrDefaultAsync(m => m.SiteIdentifier == LevelId);
                if (existing == null)
                {
                    var dbGuideType = new dbGuideTypeData
                    {
                        Name = Name,
                        SiteIdentifier = LevelId,
                        ParentIdentifier = ModSiteId,
                        ModificationId = ModId// Будет установлено позже, если нужно
                    };
                    db.GuideTypes.Add(dbGuideType);

                    dbId = dbGuideType.Id;

                    await db.SaveChangesAsync();
                    Debug.WriteLine($"[DB] Сохранён тип гайда: {Name} (LevelId: {LevelId})");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Ошибка БД] Не удалось сохранить тип гайда {Name}: {ex.Message} {ex.InnerException}");
            }
        }
    }
}
