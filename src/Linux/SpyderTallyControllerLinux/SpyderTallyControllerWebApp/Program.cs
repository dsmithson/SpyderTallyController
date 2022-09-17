using SpyderTallyControllerWebApp;
using SpyderTallyControllerWebApp.Models;

var spyderManager = new Spyder.Client.SpyderClientManager();
spyderManager.ServerListChanged += async (s, e) =>
{
    foreach (var server in (await spyderManager.GetServers()))
        server.DrawingDataThrottleInterval = TimeSpan.FromMilliseconds(100);
};
await spyderManager.StartupAsync();

var builder = WebApplication.CreateBuilder(args);

//Allow access on outside adapters
builder.WebHost.UseUrls("http://*:5000");

builder.Services.AddSingleton(spyderManager);
builder.Services.AddSingleton<ISpyderRepository, SpyderRepository>();
builder.Services.AddSingleton<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddSingleton<IRelayRepository, RelayRepository>();
builder.Services.AddSingleton<IDisplayRepository, DisplayRepository>();
builder.Services.AddSingleton<SpyderTallyEngine>();

builder.Services.AddScoped<ISystemHealthRepository, SystemHealthRepository>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

//Force immediate construction of SpyderTallyEngine (and implicitly it's dependencies)
var engine = app.Services.GetService(typeof(SpyderTallyEngine));
var display = app.Services.GetService(typeof(IDisplayRepository));

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
