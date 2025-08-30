using HaynesProParser.DataBase;
using HaynesProParser.DataBase.Items;
using System;
using System.Diagnostics;
using System.Linq;

namespace HaynesProParser.Parser.TransportDatas
{
    public class TransportJobData
    {
        public int GuideId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int dbId { get; set; }
        public string Id { get; set; }
        public string Time { get; set; }

        public TransportJobData(string type, int guideId, string id, string name, string time)
        {
            GuideId = guideId;
            Type = type;
            Name = name;
            Id = id;
            Time = time;

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Debug.WriteLine($"IncludedOrFollowup: {Type} | SubJobName: {Name} | Time: {Time}");

            SaveToDatabase();
        }


        private void SaveToDatabase()
        {
            try
            {
                Debug.WriteLine($"DB GUIDE Id: {GuideId}");

                using var db = new AppDbContext();
                var existing = db.Jobs.FirstOrDefault(m => m.SiteIdentifier == Id);
                if (existing == null)
                {
                    var dbJob = new dbJobData
                    {
                        Name = Name,
                        SiteIdentifier = Id,
                        GuideId = GuideId,
                        Time = Time,
                        Type = Type
                    };
                    db.Jobs.Add(dbJob);

                    dbId = dbJob.Id + 1;

                    db.SaveChanges();
                    Debug.WriteLine($"[DB] Сохранена работа: {Name} (ID: {Id})");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Ошибка БД] Не удалось сохранить работу {Id}: {ex.Message}");
            }
        }
    }
}
