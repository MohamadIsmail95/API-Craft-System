using System.ComponentModel.DataAnnotations;

namespace ApiCraftSystem.Model
{
    public class FullAudit
    {
        [Required]
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }


    }
}
