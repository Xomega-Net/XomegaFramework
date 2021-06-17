using AdventureWorks.Client.Blazor.Common;
using AdventureWorks.Client.Blazor.Common.Views;
using AdventureWorks.Client.Objects;
using AdventureWorks.Client.ViewModels;
using AdventureWorks.Enumerations;
using AdventureWorks.Services;
using AdventureWorks.Services.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Syncfusion.Blazor;
using System.Resources;
using Xomega.Framework;
using Xomega.Framework.Blazor.Components;
using Xomega.Framework.Services;

namespace AdventureWorks.Client.Blazor.Server
{
    public class Startup
    {
        public const string ConfigConnectionString = "add:AdventureWorksEntities:connectionString";

        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this.configuration = configuration;
            this.env = env;
            App.AdditionalAssemblies = new[] { GetType().Assembly };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // ASP.NET configuration
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddSyncfusionBlazor();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpContextAccessor();

            // Xomega Framework configuration
            services.AddErrors(env.IsDevelopment());
            services.AddSingletonLookupCacheProvider();
            services.AddXmlResourceCacheLoader(typeof(Operators).Assembly, ".enumerations.xres", false);
            services.AddOperators();

            // App services configuration
            services.AddSingleton<ResourceManager>(sp => new CompositeResourceManager(
                Client.Common.Messages.ResourceManager,
                Client.Common.Labels.ResourceManager,
                Services.Entities.Messages.ResourceManager,
                Xomega.Framework.Messages.ResourceManager));
            string connStr = configuration.GetValue<string>(ConfigConnectionString);
            services.AddDbContext<AdventureWorksEntities>(opt => opt.UseLazyLoadingProxies().UseSqlServer(connStr));
            services.AddServiceImplementations();
            services.AddDataObjects();
            services.AddViewModels();
            services.AddLookupCacheLoaders();

            MainMenu.Items.Insert(0, new MenuItem()
            {
                ResourceKey = Client.Common.Messages.HomeView_NavMenu,
                IconClass = "bi bi-house-door",
                Href = "/"
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var key = "[Your Syncfusion License Key]";
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(key);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapRazorPages();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
