using GiamSat.API.Hubs;
using GiamSat.API.Middleware;
using GiamSat.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class Startup
    {
        readonly ApplicationDbContext _context;

        public Startup(IConfiguration configuration, ApplicationDbContext context = null)
        {
            Configuration = configuration;
            _context = context;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.

            // For Entity Framework
            //GlobalVariable.ConString = EncodeMD5.DecryptString(Configuration.GetConnectionString("ConnStr"), "PTAut0m@t!0n30!)@)20");
            GlobalVariable.ConString = Configuration.GetConnectionString("ConnStr");
            //services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(GlobalVariable.ConString));

            services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(
               Configuration.GetConnectionString("ConnStr"),
               sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                   maxRetryCount: 5, // Maximum number of retry attempts
                   maxRetryDelay: TimeSpan.FromSeconds(10), // Maximum delay between retries
                   errorNumbersToAdd: null // List of additional SQL error numbers to consider transient
               )));
            //_context.Database.SetCommandTimeout(TimeSpan.FromSeconds(300));

            #region khoi tao data
            //var c = new ConfigModel()
            //{
            //    DeadbandAlarm = 5000,//5s
            //    Gain = 1,
            //    DataLogInterval = 5000,//5s
            //    DataLogWhenRunProfileInterval = 1000//1s
            //    ,
            //    DisplayRealtimeInterval = 1000
            //    ,
            //    RefreshInterval = 1000
            //    ,
            //    ChartRefreshInterval = 1000
            //    ,
            //    ChartPointNum = 30
            //};

            //OvensInfo ov = new OvensInfo();

            //for (int i = 1; i <= 13; i++)
            //{
            //    var steps = new List<StepModel>();
            //    steps.Add(new StepModel()
            //    {
            //        Id = 1,
            //        StepType = EnumProfileStepType.RampTime,
            //        Hours = 1,
            //        Minutes = 30,
            //        Seconds = 1,
            //        SetPoint = 170
            //    });
            //    steps.Add(new StepModel()
            //    {
            //        Id = 2,
            //        StepType = EnumProfileStepType.Soak,
            //        Hours = 0,
            //        Minutes = 50,
            //        Seconds = 10,
            //        SetPoint = 171
            //    });
            //    steps.Add(new StepModel()
            //    {
            //        Id = 3,
            //        StepType = EnumProfileStepType.End,
            //        Hours = 0,
            //        Minutes = 0,
            //        Seconds = 0,
            //        SetPoint = 0
            //    });

            //    var profiles = new List<ProfileModel>();
            //    profiles.Add(new ProfileModel()
            //    {
            //        Id = 1,
            //        Name = $"Profile {i}",
            //        Steps = steps
            //    });

            //    var chanel = i <= 5 ? 1 : i > 5 && i <= 10 ? 2 : 3;
            //    ov.Add(new OvenInfoModel()
            //    {
            //        Id = i,
            //        Name = $"Oven {i}",
            //        Profiles = profiles,
            //        Path = $"Local Station/ChannelTemperature1Channel{chanel}/Oven{i}"
            //    });

            //}

            //var ft01 = new FT01()
            //{
            //    Id = Guid.NewGuid(),
            //    C000 = JsonConvert.SerializeObject(c),
            //    C001 = JsonConvert.SerializeObject(ov)
            //};

            //var con = JsonConvert.SerializeObject(ft01);

            //var d = new RealtimeDisplays();

            //for (int i = 1; i <= 13; i++)
            //{
            //    d.Add(new RealtimeDisplayModel()
            //    {
            //        OvenId = i,
            //        OvenName = $"Oven {i}",
            //        Status = 1,
            //        Alarm=0,
            //        ConnectionStatus=1,
            //        DoorStatus = 1,
            //        Temperature = 150 + i,
            //        ProfileNumber_CurrentStatus = 1,
            //        ProfileName = "Profile 1",
            //        ProfileStepNumber_CurrentStatus = 1,
            //        ProfileStepType_CurrentStatus = EnumProfileStepType.RampTime,
            //        HoursRemaining_CurrentStatus = 1,
            //        MinutesRemaining_CurrentStatus = 10,
            //        SecondsRemaining_CurrentStatus = 5
            //    });
            //}



            //var ft02 = new FT02()
            //{
            //    C000 = JsonConvert.SerializeObject(d),
            //    CreatedDate = DateTime.Now,
            //    CreatedMachine = Environment.MachineName,
            //};
            //var dd = JsonConvert.SerializeObject(ft02);



            #endregion

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

                // SignalR (WebSocket) không gửi được "Authorization" header — đọc token từ query "?access_token="
                // khi request đi vào endpoint /hubs/*.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var accessToken = ctx.Request.Query["access_token"];
                        var path = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            ctx.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                foreach (var permission in AppPermissions.GetAll())
                {
                    options.AddPolicy(permission, policy => policy.Requirements.Add(new PermissionRequirement(permission)));
                }
            });

            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddControllers();

            // SignalR cho realtime log streaming (LogsHub).
            services.AddSignalR();

            //AddRepoServices(services);//add transient tu dong

            services.AddScoped<ISFT01, SFT01>();
            services.AddScoped<ISFT02, SFT02>();
            services.AddScoped<ISFT03, SFT03>();
            services.AddScoped<ISFT04, SFT04>();
            services.AddScoped<ISFT05, SFT05>();
            services.AddScoped<ISFT06, SFT06>();
            services.AddScoped<ISFT07, SFT07>();
            services.AddScoped<ISFT08, SFT08>();
            services.AddScoped<ISFT09, SFT09>();
            services.AddScoped<ISFT14, SFT14>();
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

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                  {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                          {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                          },
                          Scheme = "Bearer",
                          Name = "Bearer",
                          In = ParameterLocation.Header,

                        },
                        new List<string>()
                      }
                    });
            });

            // Allow arbitrary client browser apps to access the API.
            // In a production environment, make sure to allow only origins you trust.
            //services.AddCors(cors => cors.AddDefaultPolicy(policy => policy//.WithOrigins("http://*:5001/")
            //    .AllowAnyHeader()
            //    .AllowAnyMethod()
            //    .AllowAnyOrigin()
            //    .WithExposedHeaders("Content-Disposition")));

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()                           
                           .AllowAnyHeader();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Re-configure Serilog tại đây để thêm SignalR sink (sau khi DI đã build xong, an toàn).
            try
            {
                var hub = app.ApplicationServices.GetService(typeof(Microsoft.AspNetCore.SignalR.IHubContext<LogsHub>))
                          as Microsoft.AspNetCore.SignalR.IHubContext<LogsHub>;
                if (hub != null)
                {
                    var minLevelStr = Configuration["Logs:SignalRMinLevel"] ?? "Warning";
                    if (!Enum.TryParse<Serilog.Events.LogEventLevel>(minLevelStr, true, out var minLevel))
                        minLevel = Serilog.Events.LogEventLevel.Warning;

                    // Tạo logger mới = logger hiện tại + SignalR sink. Gán làm static logger.
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
                        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.Logger(Log.Logger)  // bao bọc logger cũ (console + 3 file)
                        .WriteTo.Sink(new GiamSat.API.Logging.SignalRLogSink(hub, minLevel))
                        .CreateLogger();
                    Log.Information("SignalR log sink đã được register");
                }
            }
            catch (Exception sinkEx)
            {
                Log.Warning(sinkEx, "Không register được SignalR log sink — bỏ qua");
            }

            #region Migration DB
            // Chạy seed BACKGROUND để API listen ngay, không bị chặn bởi DB chậm/timeout.
            // Trước đây dùng .Wait() block thread chính → API treo nếu DB không reach.
            Log.Information("Configure pipeline — kicking off background seeder...");
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = app.ApplicationServices.CreateScope();
                    Log.Information("[Seeder] Bắt đầu seed roles/users");
                    await SeedingData(scope);
                    Log.Information("[Seeder] Seed roles/users xong, sang permissions");
                    await PermissionSeeder.SeedAsync(
                        scope.ServiceProvider.GetService<ApplicationDbContext>(),
                        scope.ServiceProvider.GetService<RoleManager<IdentityRole>>());
                    Log.Information("[Seeder] Hoàn tất");
                }
                catch (Exception seedEx)
                {
                    Log.Error(seedEx, "[Seeder] Thất bại — API vẫn lên được, sẽ retry ở lần chạy sau");
                }
            });
            #endregion

            Log.Information("Configure: UseCors");
            app.UseCors("AllowAll");

            Log.Information("Configure: UseMiddleware<CorrelationIdMiddleware> + UseSerilogRequestLogging");
            app.UseMiddleware<CorrelationIdMiddleware>();
            // Elevate level theo HTTP status để dễ audit:
            //   5xx hoặc exception → Error
            //   401/403 (auth/security event) → Error  ← nên flag rõ ràng
            //   4xx khác (404, 400 client error) → Warning
            //   2xx/3xx → Information
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, elapsed, exception) =>
                {
                    if (exception != null) return Serilog.Events.LogEventLevel.Error;
                    var status = httpContext.Response.StatusCode;
                    if (status >= 500) return Serilog.Events.LogEventLevel.Error;
                    if (status == 401 || status == 403) return Serilog.Events.LogEventLevel.Error;
                    if (status >= 400) return Serilog.Events.LogEventLevel.Warning;
                    return Serilog.Events.LogEventLevel.Information;
                };
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            Log.Information("Configure: UseSwagger + SwaggerUI");
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GiamSat.API v1"));

            Log.Information("Configure: UseRouting + Authentication + Authorization");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            Log.Information("Configure: MapControllers + MapHub<LogsHub>");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<LogsHub>(LogsHub.HubPath);
            });
            Log.Information("Configure: PIPELINE READY — API listening...");
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
                services.AddScoped(type.Service, type.Implementation);
            }

            //services.AddTransient<ISDashboard, SDashboard>();
            return services;
        }

        private async Task SeedingData(IServiceScope scope)
        {
            #region User
            //get role
            var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>();

            //add role
            var res = await roleManager.FindByNameAsync("Admin");
            if (res == null) await roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });

            res = await roleManager.FindByNameAsync("User");
            if (res == null) await roleManager.CreateAsync(new IdentityRole() { Name = "User" });

            res = await roleManager.FindByNameAsync("Operator");
            if (res == null) await roleManager.CreateAsync(new IdentityRole() { Name = "Operator" });

            //add user
            var resUser = await userManager.FindByNameAsync("admin");
            if (resUser == null)
            {
                resUser = new IdentityUser()
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                };
                await userManager.CreateAsync(resUser, "admin@12345");
            }
            if (!await userManager.IsInRoleAsync(resUser, "Admin")) await userManager.AddToRoleAsync(resUser, "Admin");

            resUser = await userManager.FindByNameAsync("operator");
            if (resUser == null)
            {
                resUser = new IdentityUser()
                {
                    UserName = "operator",
                    Email = "operator@gmail.com",
                };
                await userManager.CreateAsync(resUser, "123@123");
            }
            if (!await userManager.IsInRoleAsync(resUser, "Operator")) await userManager.AddToRoleAsync(resUser, "Operator");

            resUser = await userManager.FindByNameAsync("user");
            if (resUser == null)
            {
                resUser = new IdentityUser()
                {
                    UserName = "user",
                    Email = "user@gmail.com",
                };
                await userManager.CreateAsync(resUser, "123@123");
            }
            if (!await userManager.IsInRoleAsync(resUser, "User")) await userManager.AddToRoleAsync(resUser, "User");
            #endregion

            #region Control PLC

            #endregion

            var revoConfigs = new RevoConfigs();
            for (int i = 1; i <= 9; i++)
            {
                revoConfigs.Add(new RevoConfigModel()
                {
                    Id = i,
                    Name = $"Revo {i}",
                    Path = $"Local Station/ChannelTemperature1Channel_Revo_{i}/Device1",
                    ConstringAccessDb= "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=D:\\MyCompany\\8.SourceCode\\3.Projects\\20240219_CtyAldila_GiamSatLoOven\\info\\RevoGoft\\Roll3.mdb;"
                });
            }

            var existing = scope.ServiceProvider.GetService<ApplicationDbContext>()
                .FT07_RevoConfigs
                .FirstOrDefault();
            if (existing == null)
            {
                await scope.ServiceProvider.GetService<ApplicationDbContext>()
                    .FT07_RevoConfigs
                    .AddAsync(new FT07_RevoConfig()
                    {
                        Id = Guid.NewGuid(),
                        C000 = JsonConvert.SerializeObject(revoConfigs),
                        Actived = true,
                        CreatedAt = DateTime.Now
                    });

                await scope.ServiceProvider.GetService<ApplicationDbContext>()
                    .SaveChangesAsync();

                 existing = scope.ServiceProvider.GetService<ApplicationDbContext>()
                .FT07_RevoConfigs
                .FirstOrDefault();
            }

        }
    }
}
