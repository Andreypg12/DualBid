using DualBid.Application.Profiles;
using DualBid.Application.Services.Interfaces;
using DualBid.Application.Services.Implementations;
using DualBid.Infraestructure.Data;
using DualBid.Infraestructure.Repository.Implementations;
using DualBid.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;
using Serilog;

//Sin este using no se puede usar Encoding.UTF8 en la configuración de Serilog para los archivos de log.
using System.Text;
using DualBid.Middleware;

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

// Integrar Serilog al host 
builder.Host.UseSerilog( Log.Logger);



// Add services to the container.
builder.Services.AddControllersWithViews();

//****************
// Configurar Dependency Injection
//****************

//*** Repositories
builder.Services.AddTransient<IRepositoryUser, ReposiroryUser>();

//*** Services
builder.Services.AddTransient<IserviceUser, ServiceUser>();

// Configurar AutoMapper
builder.Services.AddAutoMapper(config =>
{
    /*** Profiles */
    config.AddProfile<UserProfile>();
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
app.UseRouting();

//Activar soporte a la solicitud de registro con Serilog (recomienda usarlo después de UseRouting y antes de UseEndpoints / MapControllerRoute)
app.UseSerilogRequestLogging();

app.UseAuthorization();


app.UseAntiforgery();



app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
