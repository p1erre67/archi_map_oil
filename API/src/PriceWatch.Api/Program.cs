using FluentValidation;
using MediatR;
using PriceWatch.Api.Extensions;
using PriceWatch.Api.Infrastructure;
using PriceWatch.Modules.History.Infrastructure;
using PriceWatch.Modules.Prices.Infrastructure;
using PriceWatch.SharedKernel.Application.Behaviours;
using Scalar.AspNetCore;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ───────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, config) =>
    config.ReadFrom.Configuration(ctx.Configuration));

// ── OpenAPI + Scalar UI ───────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ── Problem Details ───────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();

// ── CORS ──────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ── Modules ───────────────────────────────────────────────────────────────────
builder.Services.AddPricesModule(builder.Configuration);
builder.Services.AddHistoryModule(builder.Configuration);
// ── MediatR ───────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies([..ModuleAssemblies()]);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblies(ModuleAssemblies());

// ── Background services ───────────────────────────────────────────────────────
// Le PriceSyncBackgroundService ne tourne qu'en Development.
// En Production (Azure Container Apps), la sync est déclenchée par un cron GitHub Actions
// qui appelle POST /api/prices/sync — ça permet le scale-to-zero (0€ de coût).
builder.Services.Configure<PriceSyncOptions>(
    builder.Configuration.GetSection(PriceSyncOptions.SectionName));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<PriceSyncBackgroundService>();
}

// ── App pipeline ──────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTitle("PriceWatch API")
        .WithTheme(ScalarTheme.DeepSpace)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
}

app.UseCors();
app.UseSerilogRequestLogging();

app.MapModuleEndpoints([..ModuleAssemblies()]);// = WebApplicationExtensions.MapModuleEndpoints(app,[..ModuleAssemblies()]);

app.Run();

static Assembly[] ModuleAssemblies() =>
[
    typeof(PriceWatch.Modules.Prices.Infrastructure.DependencyInjection).Assembly,
    typeof(PriceWatch.Modules.History.Infrastructure.DependencyInjection).Assembly,
];

// Exposes Program to WebApplicationFactory in integration tests.
public partial class Program;
