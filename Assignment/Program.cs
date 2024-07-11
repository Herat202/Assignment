using DotNetEnv;
using Assignment.Models;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from my.env file
Env.Load();

// Check and log if the environment variables are loaded correctly
var flickrApiKey = Environment.GetEnvironmentVariable("FLICKR_API_KEY");
var flickrApiSecret = Environment.GetEnvironmentVariable("FLICKR_API_SECRET");

if (string.IsNullOrEmpty(flickrApiKey) || string.IsNullOrEmpty(flickrApiSecret))
{
    builder.Logging.AddConsole();
    var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError("Flickr API Key or Secret is missing. Please ensure they are set in the environment variables.");
    Environment.Exit(1);
}

// Configure Serilog to read from appsettings.json and environment-specific JSON files
var configuration = builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration) // Read Serilog configuration from appsettings.json
    .WriteTo.Console() // Log to the console
    .WriteTo.File(new CompactJsonFormatter(), "logs/log.txt", rollingInterval: RollingInterval.Day) // Log to a file
    .CreateLogger();

// Attach Serilog to the host ensuring all logs are captured by Serilog
builder.Host.UseSerilog(); 

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add health checks for the main application (web application): the test services might possibly be dependent on the health of the main application.  
builder.Services.AddHealthChecks();

// Configure FlickrApiSettings to use environment variables
builder.Services.Configure<FlickrApiSettings>(options =>
{
    options.ApiKey = flickrApiKey ?? string.Empty;
    options.ApiSecret = flickrApiSecret ?? string.Empty;   
});

builder.Services.Configure<FlickrApiSettings>(builder.Configuration.GetSection("FlickrApiSettings"));

// Add HttpClient service
builder.Services.AddHttpClient<FetchService>();

builder.Services.AddTransient<IHttpClientWrapper, HttpClientWrapper>();
builder.Services.AddTransient<IFetchService, FetchService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment() && !app.Environment.EnvironmentName.Equals("Test", StringComparison.OrdinalIgnoreCase))
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


public partial class Program { }