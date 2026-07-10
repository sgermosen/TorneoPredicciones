using CompeTournament.Backend.Data;
using CompeTournament.Backend.Extensions;
using CompeTournament.Backend.Helpers;
using CompeTournament.Backend.Persistence.Contracts;
using CompeTournament.Backend.Persistence.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace CompeTournament.Backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var signingKey = ResolveSigningKey(builder);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserFactory, CurrentUserFactory>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(cfg =>
            {
                cfg.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                cfg.SignIn.RequireConfirmedEmail = false;
                cfg.User.RequireUniqueEmail = true;
                cfg.Password.RequireDigit = true;
                cfg.Password.RequiredUniqueChars = 1;
                cfg.Password.RequireLowercase = true;
                cfg.Password.RequireNonAlphanumeric = false;
                cfg.Password.RequireUppercase = true;
                cfg.Password.RequiredLength = 8;
                cfg.Lockout.MaxFailedAccessAttempts = 5;
                cfg.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                cfg.Lockout.AllowedForNewUsers = true;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddAuthentication()
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromMinutes(1),
                        ValidIssuer = builder.Configuration["Tokens:Issuer"],
                        ValidAudience = builder.Configuration["Tokens:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            });

            var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MauiClient", policy =>
                {
                    if (corsOrigins.Length > 0)
                    {
                        policy.WithOrigins(corsOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                });
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter("auth", limiter =>
                {
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.PermitLimit = 10;
                    limiter.QueueLimit = 0;
                });
            });

            var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerCnn"));
                }
                else
                {
                    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteCnn"));
                }
            });

            builder.Services.AddTransient<SeedDb>();
            builder.Services.AddScoped<IUserHelper, UserHelper>();
            builder.Services.AddScoped<IMailHelper, MailHelper>();
            builder.Services.AddScoped<INotificationService, LoggerNotificationService>();

            builder.Services.AddScoped<IGroupRepository, GroupRepository>();
            builder.Services.AddScoped<ITournamentTypeRepository, TournamentTypeRepository>();
            builder.Services.AddScoped<ILeagueRepository, LeagueRepository>();
            builder.Services.AddScoped<ITeamRepository, TeamRepository>();
            builder.Services.AddScoped<IMatchRepository, MatchRepository>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Accounts/Login";
                options.AccessDeniedPath = "/Accounts/NotAuthorized";
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            await SeedAsync(app);

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.MapOpenApi();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "no-referrer";
                headers["X-Permitted-Cross-Domain-Policies"] = "none";
                await next();
            });

            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("MauiClient");
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static string ResolveSigningKey(WebApplicationBuilder builder)
        {
            var key = builder.Configuration["Tokens:Key"];

            if (string.IsNullOrWhiteSpace(key))
            {
                if (!builder.Environment.IsDevelopment())
                {
                    throw new InvalidOperationException(
                        "Tokens:Key no esta configurada. Defina una clave fuerte via variables de entorno o un gestor de secretos antes de ejecutar en produccion.");
                }

                key = "dev-only-insecure-signing-key-configure-Tokens-Key-via-user-secrets-0123456789";
                builder.Configuration["Tokens:Key"] = key;
            }
            else if (!builder.Environment.IsDevelopment() && Encoding.UTF8.GetByteCount(key) < 32)
            {
                throw new InvalidOperationException(
                    "Tokens:Key debe tener al menos 256 bits (32 bytes) en produccion.");
            }

            return key;
        }

        private static async Task SeedAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<SeedDb>();
            await seeder.SeedAsync();
        }
    }
}
