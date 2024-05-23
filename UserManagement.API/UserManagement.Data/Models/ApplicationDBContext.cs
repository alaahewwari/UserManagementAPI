
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Data.Models
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //SEEDING DATA
            SeedRoles(builder);
        }
        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                               new IdentityRole { Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "1" },
                               new IdentityRole { Name = "Customer", NormalizedName = "CUSTOMER" },
                               new IdentityRole { Name = "Owner", NormalizedName = "OWNER" }
                               );
        }
    }
}
