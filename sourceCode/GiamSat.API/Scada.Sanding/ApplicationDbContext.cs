using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Sanding
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

        public virtual DbSet<GiamSat.Models.FT14_TipOdFreq> FT14_TipOdFreqs { get; set; }
        public virtual DbSet<GiamSat.Models.FT15_SandingRealtime> FT15_SandingRealtimes { get; set; }
        public virtual DbSet<GiamSat.Models.FT16_SandingLogData> FT16_SandingLogDatas { get; set; }
    }
}
