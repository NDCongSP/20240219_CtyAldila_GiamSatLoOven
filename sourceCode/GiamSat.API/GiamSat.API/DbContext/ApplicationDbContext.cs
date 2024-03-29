using GiamSat.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiamSat.API
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FT01>();
            builder.Entity<FT02>();
            //builder.Entity<DisplayRealTimeModel>().HasNoKey();//table không sử dụng khóa chính
            builder.Entity<FT03>();
            builder.Entity<FT04>();
            builder.Entity<FT05>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<GiamSat.Models.FT01> FT01 { get; set; }
        public DbSet<GiamSat.Models.FT02> FT02 { get; set; }
        public DbSet<GiamSat.Models.FT03> FT03 { get; set; }
        public DbSet<GiamSat.Models.FT04> FT04 { get; set; }
        public DbSet<GiamSat.Models.FT05> FT05 { get; set; }
    }
}
