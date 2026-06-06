using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace GiamSat.API
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            //var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            //// Đọc connection string từ appsettings.json
            //var configuration = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json")
            //    .Build();

            //optionsBuilder.UseSqlServer(
            //    configuration.GetConnectionString("ConnStr"),
            //    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            //        maxRetryCount: 5,
            //        maxRetryDelay: TimeSpan.FromSeconds(10),
            //        errorNumbersToAdd: null
            //    ));
            //return new ApplicationDbContext(optionsBuilder.Options);

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseSqlServer(@"Server=49.212.161.31;Initial Catalog=FBT_DEV2;Persist Security Info=False;User ID=sa;Password=@shuei249;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=300;", b =>
            {
                b.MigrationsHistoryTable("__EFMigrationsHistoryWMS");
            });
            return new ApplicationDbContext(builder.Options);
        }
    }
}
