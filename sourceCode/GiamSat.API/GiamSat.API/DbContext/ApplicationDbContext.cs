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
            builder.Entity<FT07_RevoConfig>();
            builder.Entity<FT08_RevoRealtime>();
            builder.Entity<FT09_RevoDatalog>();
            builder.Entity<FT10_TemperatureConfig>();
            builder.Entity<FT11_TemperatureRealtime>();
            builder.Entity<FT12_TemperatureDatalog>();
            builder.Entity<FT13_TemperatureAlarmLog>();

            builder.Entity<RoleToPermission>(entity =>
            {
                entity.ToTable("RoleToPermissions");
                entity.HasKey(x => x.Id);
            });

            // SQL Views (no key)
            builder.Entity<RevoGetTotalShaftCountDto>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("RevoReportHourVm");
            });

            builder.Entity<RevoReportStepVm>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("RevoReportStepVm");
            });

            builder.Entity<RevoReportShaftVm>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("RevoReportShaftVm");
            });

            builder.Entity<RevoReportHourVm>().HasNoKey();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
        }

        public DbSet<FT01> FT01 { get; set; }
        public DbSet<FT02> FT02 { get; set; }
        public DbSet<FT03> FT03 { get; set; }
        public DbSet<FT04> FT04 { get; set; }
        public DbSet<FT05> FT05 { get; set; }
        public DbSet<FT06> FT06 { get; set; }
        public DbSet<FT07_RevoConfig> FT07_RevoConfigs { get; set; }
        public DbSet<FT08_RevoRealtime> FT08_RevoRealtimes { get; set; }
        public DbSet<FT09_RevoDatalog> FT09_RevoDatalogs { get; set; }
        public DbSet<RevoGetTotalShaftCountDto> RevoTotalShaftCounts { get; set; }
        public DbSet<FT10_TemperatureConfig> FT10_TemperatureConfigs { get; set; }
        public DbSet<FT11_TemperatureRealtime> FT11_TemperatureRealtimes { get; set; }
        public DbSet<FT12_TemperatureDatalog> FT12_TemperatureDatalogs { get; set; }
        public DbSet<FT13_TemperatureAlarmLog> FT13_TemperatureAlarmLogs { get; set; }
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<RoleToPermission> RoleToPermissions { get; set; }
    }
}
