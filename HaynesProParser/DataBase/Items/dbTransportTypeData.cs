using System.Collections.Generic;

namespace HaynesProParser.DataBase.Items
{
    public class dbTransportTypeData
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<dbMakerData> Makers { get; set; } = [];
    }
}
