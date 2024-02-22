using DataStreams.DataStreams.Services;
using DataStreams.DataStreams.Shared;
using DataStreams.Shared;
using Microsoft.FeatureManagement;

namespace DataStreams.DataStreams;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
        
        builder.Services.AddControllersWithViews();

        builder.Services.AddHttpClient<IDataService, DataService>();
        builder.Services.AddHttpClient<IStreamService, StreamService>();

        builder.Services.AddAzureAppConfiguration();

        // Load configuration from Azure App Configuration.
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            options.Connect(builder.Configuration["ConnectionStrings:AppConfig"])
                   // Load all keys that start with 'DataStreamsApp:'.
                   .Select("DataStreamsApp:*")
                   // Reload configuration if the registered sentinel key is modified.
                   .ConfigureRefresh(refreshOptions =>
                        refreshOptions.Register("DataStreamsApp:Settings:Sentinel", refreshAll: true));

            options.UseFeatureFlags(featureFlagOptions =>
            {
                featureFlagOptions.Select("DataStreamsApp:*");
            });
        });

        builder.Services.Configure<Settings>(builder.Configuration.GetSection("DataStreamsApp:Settings"));

        // Feature Management must be after Azure App Configuration so local settings takes precedence.
        builder.Services.AddFeatureManagement();

        // Override Azure App Configuration with local appsettings for development environment.
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            LoggingExtensions.UseColoring = true;
        }

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseAzureAppConfiguration();
        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
