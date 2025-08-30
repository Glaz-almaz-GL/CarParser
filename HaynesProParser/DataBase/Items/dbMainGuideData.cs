using HaynesProParser.DataBase.Parents;
using System.Collections.Generic;

namespace HaynesProParser.DataBase.Items
{
    public class dbMainGuideData : dbItemData
    {
        public int GuideTypeId { get; set; }
        public dbGuideTypeData GuideType { get; set; }
        public ICollection<dbGuideData> Guides { get; set; } = [];
    }
}
