using DualBid.Application.Config;
using DualBid.Application.Profiles;
using DualBid.Application.Services.Implementations;
using DualBid.Application.Services.Interfaces;
//NUEVO SIGNALR
using DualBid.Hubs;
using DualBid.Infraestructure.Data;
using DualBid.Infraestructure.Models;
using DualBid.Infraestructure.Repository.Implementations;
using DualBid.Infraestructure.Repository.Interfaces;
using DualBid.Infraestructure.Services.Implementations;
using DualBid.Middleware;
using DualBid.Services.BackgroundServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
//Sin este using no se puede usar Encoding.UTF8 en la configuración de Serilog para los archivos de log.
using System.Text;

// =======================
// Configurar Serilog 
// ======================= 
// Crear carpeta Logs automáticamente (evita errores si no existe) 
Directory.CreateDirectory("Logs");

// Configuración Serilog 
var logger = new LoggerConfiguration()
    // Nivel mínimo global (recomendado: Information) 
    .MinimumLevel.Information()

    // Reducir ruido de logs internos de Microsoft 
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    //Mostrar SQL ejecutado por EF Core 
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
LogEventLevel.Information)

    // Enriquecer logs con contexto (RequestId, etc.) 
    .Enrich.FromLogContext()

    // Consola: útil para depurar en Visual Studio 
    .WriteTo.Console()

    // Archivos separados por nivel (rolling diario) 
    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
        .WriteTo.File(@"Logs \Info-.log",
            shared: true,
            encoding: Encoding.UTF8,
            rollingInterval: RollingInterval.Day))

    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
        .WriteTo.File(@"Logs \Warning-.log",
            shared: true,
            encoding: Encoding.UTF8,
            rollingInterval: RollingInterval.Day))

    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
        .WriteTo.File(@"Logs \Error-.log",
            shared: true,
            encoding: Encoding.UTF8,
            rollingInterval: RollingInterval.Day))

    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
        .WriteTo.File(@"Logs \Fatal-.log",
            shared: true,
            encoding: Encoding.UTF8,
            rollingInterval: RollingInterval.Day))

    .CreateLogger();

// Paso obligatorio ANTES de crear builder 
Log.Logger = logger;




var builder = WebApplication.CreateBuilder(args);

// Mapeo de la clase AppConfig para leer appsettings.json
builder.Services.Configure<AppConfig>(builder.Configuration);


// Integrar Serilog al host 
builder.Host.UseSerilog( Log.Logger);



// Add services to the container.
builder.Services.AddControllersWithViews();

// Session + HttpContext

builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//****************
// Configurar Dependency Injection
//****************

//*** Repositories
builder.Services.AddTransient<IRepositoryUser, ReposiroryUser>();

builder.Services.AddTransient<IRepositoryCategory, RepositoryCategory>();

builder.Services.AddTransient<IRepositoryRole, RepositoryRole>();

builder.Services.AddTransient<IRepositoryAuctionState, RepositoryAuctionState>();

builder.Services.AddTransient<IRepositoryUserStatus, RepositoryUserStatus>();

builder.Services.AddTransient<IRepositoryAuction, RepositoryAuction>();

builder.Services.AddTransient<IRepositoryBid, RepositoryBid>();

builder.Services.AddTransient<IRepositoryComic, RepositoryComic>();

builder.Services.AddTransient<IRepositoryPublisher, RepositoryPublisher>();

builder.Services.AddTransient<IRepositoryStateConservation, RepositoryStateConservation>();

builder.Services.AddTransient<IRepositoryImgComic, RepositoryImgComic>();



//*** Services
builder.Services.AddTransient<IServiceUser, ServiceUser>();

builder.Services.AddTransient<IServiceCategory, ServiceCategory>();

builder.Services.AddTransient<IServiceRole, ServiceRole>();

builder.Services.AddTransient<IServiceAuctionState, ServiceAuctionState>();

builder.Services.AddTransient<IServiceUserStatus, ServiceUserStatus>();

builder.Services.AddTransient<IServiceAuction, ServiceAuction>();

builder.Services.AddTransient<IServiceBid, ServiceBid>();

builder.Services.AddTransient<IServiceComic, ServiceComic>();

builder.Services.AddTransient<IServicePublisher, ServicePublisher>();

builder.Services.AddTransient<IServiceStateConservation, ServiceStateConservation>();

builder.Services.AddTransient<IServiceImgComic, ServiceImgComic>();

builder.Services.AddSignalR();

//Reportes
// DualBid/Program.cs
builder.Services.AddScoped<IServiceReports, ServiceReports>();

//builder.Services.AddHostedService<AuctionMonitorService>();
//builder.Services.AddScoped<IAuctionMonitorService, AuctionMonitorService>();

// 1. Crear UNA sola instancia Singleton
builder.Services.AddSingleton<AuctionMonitorService>();

// 2. Registrarla como IHostedService (usa la misma instancia del paso 1)
builder.Services.AddHostedService(sp => sp.GetRequiredService<AuctionMonitorService>());

// Configurar AutoMapper
builder.Services.AddAutoMapper(config =>
{
    /*** Profiles */
    config.AddProfile<UserProfile>();

    config.AddProfile<CategoryProfile>();

    config.AddProfile<RoleProfile>();

    config.AddProfile<AuctionStateProfile>();

    config.AddProfile<UserStatusProfile>();

    config.AddProfile<AuctionProfile>();

    config.AddProfile<BidProfile>();

    config.AddProfile<ComicProfile>();

    config.AddProfile<PublisherProfile>();

    config.AddProfile<StateConservationProfile>();

    config.AddProfile<ImgComicProfile>();


});

//Seguridad
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Login/Index";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.AccessDeniedPath = "/Login/Forbidden";
    });

builder.Services.AddControllersWithViews(options => {
    options.Filters.Add(
        new ResponseCacheAttribute
        {
            NoStore = true,
            Location = ResponseCacheLocation.None,
        });
});



// Configurar SQL Server DbContext
var connectionString = builder.Configuration.GetConnectionString("SqlServerDataBase");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'SqlServerDataBase' en appsettings.json / appsettings.Development.json.");
}

builder.Services.AddDbContext<DualBidContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Reintentos ante fallos transitorios (recomendado)
        sqlOptions.EnableRetryOnFailure();
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});





var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}else
{
    // Middleware personalizado 
    app.UseMiddleware<ErrorHandlingMiddleware>();
}

//  El orden de estos comandos debe ser obligatoriamente este.

app.UseHttpsRedirection();

//Signal R requiere que UseStaticFiles esté antes de UseRouting para servir correctamente los archivos necesarios para la comunicación en tiempo real (como el script de SignalR).
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

//Activar soporte a la solicitud de registro con Serilog (recomienda usarlo después de UseRouting y antes de UseEndpoints / MapControllerRoute)
app.UseSerilogRequestLogging();

app.UseAuthorization();


//Signal R
app.MapHub<AuctionHub>("/auctionHub");

app.UseAntiforgery();



app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
