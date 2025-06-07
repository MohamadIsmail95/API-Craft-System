using ApiCraftSystem.Data;
using System.ComponentModel.DataAnnotations;

namespace ApiCraftSystem.Model
{
    public class Tenant
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public virtual ICollection<ApplicationUser>? ApplicationUsers { get; set; }
        public virtual ICollection<ApiStore>? ApiStores { get; set; }
    }
}
