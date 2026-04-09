using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.TrackingTime_AutoRolling1
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext():base("name=ovenDB")
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
