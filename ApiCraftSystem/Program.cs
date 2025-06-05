using ApiCraftSystem.ActionFilters;
using ApiCraftSystem.Components;
using ApiCraftSystem.Components.Account;
using ApiCraftSystem.Data;
using ApiCraftSystem.HangFire;
using ApiCraftSystem.Helper.Mapper;
using ApiCraftSystem.Helper.Utility;
using ApiCraftSystem.Repositories.ApiServices;
using ApiCraftSystem.Repositories.ApiShareService;
using ApiCraftSystem.Repositories.EmailService;
using ApiCraftSystem.Repositories.GenericService;
using ApiCraftSystem.Repositories.RateService;
using ApiCraftSystem.Repositories.SchedulerService;
using ApiCraftSystem.Repositories.TenantService;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System.ComponentModel;
using System.Text;
namespace ApiCraftSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
            builder.Services.AddScoped<IApiService, ApiService>();
            builder.Services.AddScoped<ISchedulerService, SchedulerService>();
            builder.Services.AddScoped<IDynamicDataService, DynamicDataService>();
            builder.Services.AddScoped<IRateService, RateService>();
            builder.Services.AddScoped<ITenantService, TenantService>();
            builder.Services.AddScoped<IApiShareService, ApiShareService>();


            //------------Email Config--------------------------

            builder.Services.AddTransient<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
            builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();


            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                });
            // .AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            //builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddSignInManager()
            //    .AddDefaultTokenProviders();
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
             .AddEntityFrameworkStores<ApplicationDbContext>()
             .AddSignInManager()
             .AddDefaultTokenProviders();



            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            // Register AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient(); // Basic registration
            builder.Services.AddControllers(); // <--- Required for API support


            // Add Hangfire services
            builder.Services.AddHangfire(config =>
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
           .UseSimpleAssemblyNameTypeSerializer()
           .UseRecommendedSerializerSettings()
           .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseActivator(new HangfireActivator(builder.Services.BuildServiceProvider())));

            builder.Services.AddHangfireServer();


            // Read EPPlus configuration
            var epplusSection = builder.Configuration.GetSection("EPPlus");
            var licenseContext = epplusSection.GetValue<string>("LicenseContext");
            var licenseKey = epplusSection.GetValue<string>("LicenseKey");

            if (licenseContext == "NonCommercial")
            {
                ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization"); //This will also set the Company property to the organization name provided in the argument.
            }


            //JWT Token 

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer(options =>
        {
            var config = builder.Configuration;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
            };
        });

            builder.Services.AddScoped<JwtTokenGenerator>();




            var app = builder.Build();

            app.UseStaticFiles();


            // Custom middleware to redirect unauthenticated users from root
            app.Use(async (context, next) =>
            {
                // Redirect root "/" to login if not authenticated
                if (context.Request.Path == "/" && !context.User.Identity.IsAuthenticated)
                {
                    context.Response.Redirect("/Account/Login"); // adjust route as needed
                    return;
                }

                await next();
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Use Hangfire dashboard
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            }); // /hangfire

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapControllers(); // <--- Maps your API endpoints

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
