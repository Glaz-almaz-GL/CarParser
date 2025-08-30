using HaynesProParser.DataBase.Parents;
using System.Collections.Generic;

namespace HaynesProParser.DataBase.Items
{
    public class dbModificationData : dbItemData
    {
        public string Axle { get; set; }
        public string EngineCode { get; set; }
        public string Capacity { get; set; }
        public string Volume { get; set; }
        public string Power { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsDiscontinued { get; set; } = true;
        public int FuelTypeId { get; set; }
        public string BuildType { get; set; }
        public string Tonnage { get; set; }
        public bool IsTruck { get; set; }

        public int ModelId { get; set; }
        public dbModelData Model { get; set; }
        public ICollection<dbGuideTypeData> GuideTypes { get; set; } = [];
    }
}
