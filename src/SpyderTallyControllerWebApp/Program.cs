using Spyder.Client.Net.Notifications;
using SpyderTallyControllerWebApp;
using SpyderTallyControllerWebApp.Hubs;
using SpyderTallyControllerWebApp.Models;
using SpyderTallyControllerWebApp.Services;

var serverEventListener = await SpyderServerEventListener.GetInstanceAsync();

var builder = WebApplication.CreateBuilder(args);

// Use ASPNETCORE_URLS if set (e.g. http://+:80 in production), otherwise default to port 5000 for development
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://*:5000");
}

builder.Services.AddSingleton(serverEventListener);
builder.Services.AddSingleton<ISpyderRepository, SpyderRepository>();
builder.Services.AddSingleton<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddSingleton<IRelayRepository, RelayRepository>();
builder.Services.AddSingleton<IDisplayRepository, DisplayRepository>();
builder.Services.AddSingleton<SpyderTallyEngine>();

builder.Services.AddScoped<ISystemHealthRepository, SystemHealthRepository>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddHostedService<TallyStatusBroadcaster>();

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

app.MapHub<TallyHub>("/tallyHub");

app.Run();
