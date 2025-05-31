using ApiCraftSystem.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace ApiCraftSystem.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {

        public DbSet<ApiStore> ApiStores { get; set; }
        public DbSet<ApiHeader> ApiHeaders { get; set; }
        public DbSet<ApiMap> ApiMaps { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Rate> Rates { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApiHeader>().HasQueryFilter(h => !h.IsDeleted);
            modelBuilder.Entity<ApiMap>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<ApiStore>().HasQueryFilter(m => !m.IsDeleted);

        }
    }
}
