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
            builder.UseSqlServer(@"Server=phucthinhautomation.ddns.net,1433;Database=oven;User Id=dev1;Password=DaPHA5eY@$AWysDW;TrustServerCertificate=True;Connection Timeout=10", b =>
            {
                b.MigrationsHistoryTable("__EFMigrationsHistory");
            });
            return new ApplicationDbContext(builder.Options);
        }
    }
}
