using ApiCraftSystem.Model;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCraftSystem.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [ForeignKey("Tenant")]
        public Guid? TenantId { get; set; }

        [ForeignKey("Role")]
        public string? RoleId { get; set; }
        public virtual Tenant? Tenant { get; set; }
        public virtual IdentityRole? Role { get; set; }
        public virtual ICollection<Rate>? Rates { get; set; }
    }

}
