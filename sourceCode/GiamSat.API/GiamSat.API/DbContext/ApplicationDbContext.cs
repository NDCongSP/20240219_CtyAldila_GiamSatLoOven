using GiamSat.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
            builder.Entity<FT07_RevoConfig>();
            builder.Entity<FT08_RevoRealtime>();
            builder.Entity<FT09_RevoDatalog>();
            builder.Entity<FT10_TemperatureConfig>();
            builder.Entity<FT11_TemperatureRealtime>();
            builder.Entity<FT12_TemperatureDatalog>();
            builder.Entity<FT13_TemperatureAlarmLog>();
            builder.Entity<Permissions>();
            builder.Entity<RoleToPermission>();

            builder.Entity<RevoGetTotalShaftCountDto>().HasNoKey().ToView(null);
            builder.Entity<RevoReportStepVm>().HasNoKey();
            builder.Entity<RevoReportShaftVm>().HasNoKey();
            builder.Entity<RevoReportHourVm>().HasNoKey();

            // Cấu hình cho RevoReportHourVm
            builder.Entity<RevoGetTotalShaftCountDto>(entity =>
            {
                entity.HasNoKey(); // View thường không có Primary Key
                entity.ToView("RevoReportHourVm"); // Tên chính xác của View trong SQL
            });

            // Tương tự cho các View khác
            builder.Entity<RevoReportShaftVm>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("RevoReportShaftVm");
            });

            builder.Entity<RevoReportStepVm>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("RevoReportStepVm");
            });
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
        public DbSet<GiamSat.Models.FT07_RevoConfig> FT07_RevoConfigs { get; set; }
        public DbSet<GiamSat.Models.FT08_RevoRealtime> FT08_RevoRealtimes { get; set; }
        public DbSet<GiamSat.Models.FT09_RevoDatalog> FT09_RevoDatalogs { get; set; }
        public DbSet<GiamSat.Models.RevoGetTotalShaftCountDto> RevoTotalShaftCounts { get; set; }
        public DbSet<GiamSat.Models.FT10_TemperatureConfig> FT10_TemperatureConfigs { get; set; }
        public DbSet<GiamSat.Models.FT11_TemperatureRealtime> FT11_TemperatureRealtimes { get; set; }
        public DbSet<GiamSat.Models.FT12_TemperatureDatalog> FT12_TemperatureDatalogs { get; set; }
        public DbSet<GiamSat.Models.FT13_TemperatureAlarmLog> FT13_TemperatureAlarmLogs { get; set; }
        public DbSet<GiamSat.Models.Permissions> Permissions { get; set; }
        public DbSet<GiamSat.Models.RoleToPermission> RoleToPermissions { get; set; }

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
