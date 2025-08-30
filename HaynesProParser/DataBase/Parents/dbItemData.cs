using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaynesProParser.DataBase.Parents
{
    public class dbItemData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public bool IsDeleted { get; set; }

        public string ParentIdentifier { get; set; }
        public string SiteIdentifier { get; set; }
        public string Name { get; set; }

        public dbItemData() { }
    }
}
