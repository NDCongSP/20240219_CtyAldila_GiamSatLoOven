using GiamSat.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.

            // For Entity Framework
            GlobalVariable.ConString = EncodeMD5.DecryptString(Configuration.GetConnectionString("ConnStr"), "PTAut0m@t!0n30!)@)20");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(GlobalVariable.ConString));

            // For Identity
            services.AddIdentity<IdentityUser, IdentityRole>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
                o.Password.RequiredLength = 6;
                o.Password.RequiredUniqueChars = 0;


                o.Lockout.MaxFailedAccessAttempts = 5;

                o.SignIn.RequireConfirmedAccount = false;
                o.SignIn.RequireConfirmedEmail = false;
                o.SignIn.RequireConfirmedPhoneNumber = false;

            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Adding Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Adding Jwt Bearer
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });

            services.AddControllers();

            //AddRepoServices(services);//add transient tu dong
            services.AddTransient<ISDisplayRealtime, SDisplayRealtime>();
            services.AddTransient<ISDataLog, SDataLog>();
            services.AddTransient<ISChuongInfo, SChuongInfo>();
            services.AddTransient<ISRealtimeDisplay,SRealtimeDisplay>();
            services.AddTransient<ISFT100, SFT100>();
            services.AddTransient<ISFT101, SFT101>();
            services.AddTransient<ISAlarmLog, SAlarmLog>();
            services.AddTransient<ISAlarmSettings, SAlarmSettings>();

            services.AddScoped<SCommon>();

            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.CustomOperationIds(desc =>
                {
                    if (desc.ActionDescriptor is ControllerActionDescriptor descriptor)
                    {
                        return $"{descriptor.ControllerName}_{descriptor.ActionName}";
                    }
                    return desc.GroupName;
                });

                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GiamSat.API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Description = "Input your Bearer token to access this API",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                });
            });

            // Allow arbitrary client browser apps to access the API.
            // In a production environment, make sure to allow only origins you trust.
            services.AddCors(cors => cors.AddDefaultPolicy(policy => policy//.WithOrigins("http://*:5001/")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .WithExposedHeaders("Content-Disposition")));
            
            //services.AddCors();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GiamSat.API v1"));

            app.UseCors();
            //app.UseCors(o =>
            //{
            //    o.AllowAnyOrigin();
            //    o.AllowAnyHeader();
            //    o.AllowAnyMethod();
            //});

            app.UseRouting();

            //them
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public IServiceCollection AddRepoServices(IServiceCollection services)
        {
            var managers = typeof(Startup);

            var types = managers
                .Assembly
                .GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Service = t.GetInterface($"I{t.Name}"),
                    Implementation = t
                })
                .Where(t => t.Service != null);

            foreach (var type in types)
            {
                services.AddTransient(type.Service, type.Implementation);
            }

            //services.AddTransient<ISDashboard, SDashboard>();
            return services;
        }
    }
}
