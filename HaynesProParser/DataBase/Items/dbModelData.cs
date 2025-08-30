using HaynesProParser.DataBase.Parents;
using System.Collections.Generic;

namespace HaynesProParser.DataBase.Items
{
    public class dbModelData : dbItemData
    {
        public string ModelGroupId { get; set; }
        public string StoryId { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsDiscontinued { get; set; }
        public string ImageLink { get; set; }
        public string LocationIdLink { get; set; }
        public string JackLocationLink { get; set; }
        public string DiagnosticConnectorLink { get; set; }

        public int MakerId { get; set; }
        public dbMakerData Maker { get; set; }
        public ICollection<dbModificationData> Modifications { get; set; } = [];
    }
}
