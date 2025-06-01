using ApiCraftSystem.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCraftSystem.Model
{
    public class Rate
    {
        public Guid Id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; } = string.Empty;
        public int Grade { get; set; } = 0;

        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}
