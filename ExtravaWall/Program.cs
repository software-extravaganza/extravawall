using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
using System.Data.Common;
using MySqlConnector;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpLogging(o => { });
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: false,
    reloadOnChange: true);

//var configurationBuilder = new ConfigurationBuilder();
//configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
//configurationBuilder.AddEnvironmentVariables();
//var configuration = configurationBuilder.Build();
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting web application");
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// builder.Services.AddLogging(config => {
//     config.AddConsole();
//     config.AddDebug();
// });
// builder.Logging.ClearProviders();
// builder.Logging.AddConsole();
// create a logger factory


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Extrava API", Version = "v1" });
});

builder.Services.AddOpenTelemetry()
    //.ConfigureResource(resource => resource.AddService(serviceName: app.Environment.ApplicationName))
    .WithMetrics(metrics => {
        metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter()
        .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel")
        .AddMeter("ExtravaWallRouter")
        .AddView("http.server.request.duration",
            new ExplicitBucketHistogramConfiguration {
                Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05,
                       0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
            });

    });

builder.Services.AddSingleton<ExtravaMetrics>();
builder.Services.AddMetrics();

builder.Services.AddSingleton<IDataSourceProvider, DataSourceProvider>();
builder.Services.AddTransient((provider) => {
    var dataSourceProvider = provider.GetRequiredService<IDataSourceProvider>();
    return dataSourceProvider.GetConnection();
});

builder.Services.AddDbContext<ExtravaWallContext>((provider, options) => {
    var logger = provider.GetRequiredService<ILogger>();
    var connection = provider.GetRequiredService<MySqlConnection>();
    var serverVersion = new MySqlServerVersion(ServerVersion.AutoDetect(connection));
    options.UseMySql(connection, serverVersion)
        .LogTo((log) => {
            logger.Debug(log.ToString());
        })
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors();
});

builder.Services.AddDbContext<ExtravaWallSystemContext>((provider, options) => {
    var logger = provider.GetRequiredService<ILogger>();
    var connection = provider.GetRequiredService<MySqlConnection>();
    var serverVersion = new MySqlServerVersion(ServerVersion.AutoDetect(connection));
    options.UseMySql(connection, serverVersion)
        .LogTo((log) => {
            logger.Debug(log.ToString());
        })
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors();
});



var app = builder.Build();
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpLogging();

    // app.UseExceptionHandler(exceptionHandlerApp => {
    //     exceptionHandlerApp.Run(async context => {
    //         context.Response.StatusCode = StatusCodes.Status500InternalServerError;

    //         context.Response.ContentType = Text.Plain;
    //         await context.Response.WriteAsync("An exception was thrown.");
    //         var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

    //         if (exceptionHandlerPathFeature?.Error is FileNotFoundException) {
    //             await context.Response.WriteAsync(" The file was not found.");
    //         }

    //         if (exceptionHandlerPathFeature?.Path == "/") {
    //             await context.Response.WriteAsync(" Page: Home.");
    //         }
    //     });
    // });

    using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope()) {
        var systemContext = serviceScope?.ServiceProvider.GetRequiredService<ExtravaWallSystemContext>();
        if (systemContext is null) {
            throw new Exception("System context is null");
        }

        systemContext.CreateDbIfNotExists();
        var context = serviceScope?.ServiceProvider.GetRequiredService<ExtravaWallContext>();
        if (context is null) {
            throw new Exception("Context is null");
        }

        context.Database.Migrate();
        context.Commit("Initial commit");
        context.Seed();
    }
}

app.MapPrometheusScrapingEndpoint();

// app.Use(async (context, next) => {
//     var tagsFeature = context.Features.Get<IHttpMetricsTagsFeature>();
//     if (tagsFeature != null) {
//         var source = context.Request.Query["utm_medium"].ToString() switch {
//             "" => "none",
//             "social" => "social",
//             "email" => "email",
//             "organic" => "organic",
//             _ => "other"
//         };
//         tagsFeature.Tags.Add(new KeyValuePair<string, object?>("mkt_medium", source));
//     }

//     await next.Invoke();
// });

app.MapGet("/", (ExtravaMetrics metrics) => {
    metrics.PacketInspected("tcp", 1);
    return "Hello OpenTelemetry! ticks:" + DateTime.Now.Ticks.ToString()[^3..];
});

app.MapGet("/branch", (ExtravaMetrics metrics, ExtravaWallSystemContext db) => {
    db.Branch("changes");
    return "Branch made!";
});

app.Run();