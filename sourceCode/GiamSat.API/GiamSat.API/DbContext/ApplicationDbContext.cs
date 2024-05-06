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
        //public ApplicationDbContext(string con ) : base(GetOptions(con))
        //{
        //}
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FT01>();
            builder.Entity<FT02>();
            //builder.Entity<DisplayRealTimeModel>().HasNoKey();//table không sử dụng khóa chính
            builder.Entity<FT03>();
            builder.Entity<FT04>();
            builder.Entity<FT05>();
            builder.Entity<FT06>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.EnableSensitiveDataLogging();
        }

        public DbSet<GiamSat.Models.FT01> FT01 { get; set; }
        public DbSet<GiamSat.Models.FT02> FT02 { get; set; }
        public DbSet<GiamSat.Models.FT03> FT03 { get; set; }
        public DbSet<GiamSat.Models.FT04> FT04 { get; set; }
        public DbSet<GiamSat.Models.FT05> FT05 { get; set; }
        public DbSet<GiamSat.Models.FT06> FT06 { get; set; }

        //private static DbContextOptions<ApplicationDbContext> GetOptions(string connection)
        //{
        //    //if (string.IsNullOrEmpty(connection))
        //    //{
        //    //    connection = connectionString;
        //    //}
        //    //else if (connection.Length <= 4)
        //    //{
        //    //    connection = connectionString.Replace("Bat", connection);
        //    //}
        //    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        //    optionsBuilder.UseSqlServer(connection);
        //   // optionsBuilder.UseLazyLoadingProxies(_LazyLoadingProxies);

        //    return optionsBuilder.Options;
        //}
    }
}
