using Microsoft.AspNetCore.Authentication.Cookies;

using AdventureWorks.Client.Blazor.Common;
using AdventureWorks.Client.Blazor.Common.Views;
using AdventureWorks.Client.Common.DataObjects;
using AdventureWorks.Client.Common.ViewModels;
using AdventureWorks.Services.Common.Enumerations;
using AdventureWorks.Services.Common;
using AdventureWorks.Services.Entities;
#if !EF6
using Microsoft.EntityFrameworkCore;
#endif
using Syncfusion.Blazor;
using System.Resources;
using Xomega.Framework;
using Xomega.Framework.Blazor.Components;
using Xomega.Framework.Services;


string ConfigConnectionString = "add:AdventureWorksEntities:connectionString";

App.AdditionalAssemblies = new[] { typeof(Program).Assembly };

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddXmlFile("db.config", optional: false, reloadOnChange: false);

var services = builder.Services;

// ASP.NET configuration
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
services.AddSyncfusionBlazor();
services.AddRazorPages();
services.AddServerSideBlazor();
services.AddHttpContextAccessor();

// Xomega Framework configuration
services.AddErrors(builder.Environment.IsDevelopment());
services.AddSingletonLookupCacheProvider();
services.AddXmlResourceCacheLoader(typeof(Operators).Assembly, ".enumerations.xres", false);
services.AddOperators();
services.AddTransient<IPrincipalProvider, ContextPrincipalProvider>();
services.AddScoped<SignInManager>();

// App services configuration
services.AddSingleton<ResourceManager>(sp => new CompositeResourceManager(
    AdventureWorks.Client.Common.Messages.ResourceManager,
    AdventureWorks.Client.Common.Labels.ResourceManager,
    AdventureWorks.Services.Entities.Messages.ResourceManager,
    Xomega.Framework.Messages.ResourceManager));
string connStr = builder.Configuration.GetValue<string>(ConfigConnectionString);
#if EF6
services.AddScoped(sp => new AdventureWorksEntities(connStr));
#else
services.AddDbContext<AdventureWorksEntities>(opt => opt.UseLazyLoadingProxies().UseSqlServer(connStr));
#endif
services.AddServiceImplementations();
services.AddDataObjects();
services.AddViewModels();
services.AddLookupCacheLoaders();

MainMenu.Items.Insert(0, new MenuItem()
{
    ResourceKey = AdventureWorks.Client.Common.Messages.HomeView_NavMenu,
    IconClass = "bi bi-house-door",
    Href = "/"
});

// TODO: add authorization with any security policies

foreach (var mi in MainMenu.Items)
    mi.ForEachItem(mi =>
    {
        // TODO: set security policy for navigation menu items here
        mi.Policy = null;
    });

// Build and configure the app
var app = builder.Build();

var key = "[Your Syncfusion License Key]";
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(key);
if (app.Environment.IsDevelopment())
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

app.MapControllers();
app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

app.Run();