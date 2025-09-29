using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaGestaoEscola.Web.Data;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.Helpers;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;
using SuperShop.Helpers;
using Syncfusion.Licensing;
using System.Text;

namespace SistemaGestaoEscola.Web
{
    public class Program
    {
        private const string ApiOrCookieScheme = "ApiOrCookie";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var syncfusionLicense = builder.Configuration["Syncfusion:LicenseKey"];
            if (!string.IsNullOrWhiteSpace(syncfusionLicense))
            {
                SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);
            }

            string connectionString;
            if (builder.Environment.IsDevelopment())
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
            else
                connectionString = builder.Configuration.GetConnectionString("SomeeConnection")!;

            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

            builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
            {
                o.TokenLifespan = TimeSpan.FromMinutes(30);
            });


            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = ApiOrCookieScheme;
                    options.DefaultAuthenticateScheme = ApiOrCookieScheme;
                    options.DefaultChallengeScheme = ApiOrCookieScheme;
                })
                .AddPolicyScheme(ApiOrCookieScheme, "API or Cookie", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var path = context.Request.Path;
                        return path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                            ? JwtBearerDefaults.AuthenticationScheme
                            : IdentityConstants.ApplicationScheme; // Cookie do Identity
                    };
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(jwt.Key) ? "fallback_key" : jwt.Key)
                        ),
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            var msg = builder.Environment.IsDevelopment()
                                ? "Não autorizado (Bearer ausente/inválido/expirado)."
                                : "Não autorizado.";
                            var result = System.Text.Json.JsonSerializer.Serialize(new { message = msg });
                            return context.Response.WriteAsync(result);
                        },
                        OnAuthenticationFailed = context =>
                        {
                            if (builder.Environment.IsDevelopment())
                            {
                                context.Response.StatusCode = 401;
                                context.Response.ContentType = "application/json";
                                var result = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    message = "Falha ao autenticar o token.",
                                    error = context.Exception.Message
                                });
                                return context.Response.WriteAsync(result);
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<ICourseDisciplinesRepository, CourseDisciplinesRepository>();
            builder.Services.AddScoped<IClassRepository, ClassRepository>();
            builder.Services.AddScoped<IClassStudentsRepository, ClassStudentsRepository>();
            builder.Services.AddScoped<IClassProfessorsRepository, ClassProfessorsRepository>();
            builder.Services.AddScoped<IStudentGradesRepository, StudentGradesRepository>();
            builder.Services.AddScoped<IAlertRepository, AlertRepository>();

            builder.Services.AddTransient<SeedDb>();
            builder.Services.AddScoped<IUserHelper, UserHelper>();

            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("Email"));
            builder.Services.AddScoped<IMailHelper, MailHelper>();

            builder.Services.AddScoped<IBlobHelper, BlobHelper>();
            builder.Services.AddScoped<ITimeZoneHelper, TimeZoneHelper>();
            builder.Services.AddMemoryCache();

            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "SistemaGestaoEscola API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT no header Authorization. Ex.: Bearer {seu token}",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.AddConsole();
            }

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error/500");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SistemaGestaoEscola API V1");
                c.RoutePrefix = "swagger";
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllers();

            RunSeeding(app);

            app.Run();
        }

        private static void RunSeeding(IHost app)
        {
            using var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<SeedDb>();
            seeder.SeedAsync().Wait();
        }
    }
}
