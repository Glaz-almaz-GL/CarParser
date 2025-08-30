using HaynesProParser.DataBase.Parents;
using System.Collections.Generic;

namespace HaynesProParser.DataBase.Items
{
    public class dbGuideData : dbItemData
    {
        public string OECode { get; set; }
        public string Time { get; set; }

        public int MainGuideId { get; set; }
        public dbMainGuideData MainGuide { get; set; }
        public ICollection<dbJobData> Jobs { get; set; }
    }
}
