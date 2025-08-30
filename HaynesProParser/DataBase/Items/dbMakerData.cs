using HaynesProParser.DataBase.Parents;
using System.Collections.Generic;

namespace HaynesProParser.DataBase.Items
{
    public class dbMakerData : dbItemData
    {
        public string ImageLink { get; set; }

        public int TransportTypeId { get; set; }

        public dbTransportTypeData TransportType { get; set; }
        public ICollection<dbModelData> Models { get; set; } = [];
    }
}
