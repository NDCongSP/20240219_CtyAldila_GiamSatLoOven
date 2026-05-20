using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.TemperatureMonitoring
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=ovenDB")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<GiamSat.Models.FT10_TemperatureConfig> FT10_TemperatureConfigs { get; set; }
        public virtual DbSet<GiamSat.Models.FT11_TemperatureRealtime> FT11_TemperatureRealtimes { get; set; }
        public virtual DbSet<GiamSat.Models.FT12_TemperatureDatalog> FT12_TemperatureDatalogs { get; set; }
        public virtual DbSet<GiamSat.Models.FT13_TemperatureAlarmLog> FT13_TemperatureAlarmLogs { get; set; }
    }
}
