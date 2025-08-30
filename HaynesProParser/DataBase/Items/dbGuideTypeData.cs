using HaynesProParser.DataBase.Parents;
using System.Collections.Generic;

namespace HaynesProParser.DataBase.Items
{
    public class dbGuideTypeData : dbItemData
    {
        public int ModificationId { get; set; }
        public dbModificationData Modification { get; set; }
        public ICollection<dbMainGuideData> MainGuides { get; set; } = [];
    }
}
