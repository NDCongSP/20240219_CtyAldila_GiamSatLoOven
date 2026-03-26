using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada_TrackingTime_AutoRolling
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(string connectionString)
        : base(connectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<GiamSat.Models.FT07_RevoConfig> FT07_RevoConfigs { get; set; }
        public virtual DbSet<GiamSat.Models.FT08_RevoRealtime> FT08_RevoRealtimes { get; set; }
        public virtual DbSet<GiamSat.Models.FT09_RevoDatalog> FT09_RevoDatalogs { get; set; }
    }
}
