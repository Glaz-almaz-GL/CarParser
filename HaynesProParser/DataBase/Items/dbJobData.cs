using HaynesProParser.DataBase.Parents;

namespace HaynesProParser.DataBase.Items
{
    public class dbJobData : dbItemData
    {
        public string Type { get; set; }
        public string Time { get; set; }

        public int GuideId { get; set; }
        public dbGuideData Guide { get; set; }
    }
}
