using ElGato_API.Models.Requests;
using ElGato_API.Models.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ElGato_API.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>()
             .HasOne(a => a.UserInformation)
             .WithOne(u => u.AppUser)
             .HasForeignKey<UserInformation>(ui => ui.UserId);

            modelBuilder.Entity<AppUser>()
             .HasOne(a => a.CalorieInformation)
             .WithOne(u => u.AppUser)
             .HasForeignKey<CalorieInformation>(ui => ui.UserId);


            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AppUser> AppUser { get; set; }
        public DbSet<UserInformation> UserInformation { get; set; }
        public DbSet<CalorieInformation> CalorieInformation { get; set; }
        public DbSet<ReportedIngredients> ReportedIngredients { get; set; }
        public DbSet<AddProductRequest> AddProductRequest { get; set; }
        public DbSet<ReportedMeals> ReportedMeals { get; set; }

    }
}
