using GiamSat.Models;
using Microsoft.EntityFrameworkCore;

namespace GiamSat.API
{
    /// <summary>
    /// DbContext kết nối tới external SQL Server chứa dữ liệu đo tần số Auto Fre.
    /// Connection string: "ConnStrExternal" trong appsettings.json.
    /// </summary>
    /// <remarks>
    /// File:    GiamSat.API/DbContext/FreMeasurementDbContext.cs
    /// Author:  Cong.Nguyen
    /// Created: 2026-05-28
    /// </remarks>
    public class FreMeasurementDbContext : DbContext
    {
        public FreMeasurementDbContext(DbContextOptions<FreMeasurementDbContext> options) : base(options)
        {
        }

        public DbSet<FreMeasurementRecord> FreMeasurements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<FreMeasurementRecord>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
