using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCraftSystem.Model
{
    public class ApiMap
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("ApiStore")]
        public Guid ApiStoreId { get; set; }

        [Required]
        public string FromKey { get; set; } = string.Empty;

        [Required]
        public string MapKey { get; set; } = string.Empty;

        [Required]
        public string DataType { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        public virtual ApiStore ApiStore { get; set; }


    }
}
