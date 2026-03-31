using FluentValidation;
using MediatR;
using PriceWatch.Api.Extensions;
using PriceWatch.Api.Infrastructure;
using PriceWatch.Modules.Prices.Endpoints;
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

// ── Modules ───────────────────────────────────────────────────────────────────
builder.Services.AddPricesModule(builder.Configuration);
// ── MediatR ───────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies([..ModuleAssemblies()]);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblies(ModuleAssemblies());

// ── Background services ───────────────────────────────────────────────────────
builder.Services.Configure<PriceSyncOptions>(
    builder.Configuration.GetSection(PriceSyncOptions.SectionName));
builder.Services.AddHostedService<PriceSyncBackgroundService>();

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

app.UseSerilogRequestLogging();

app.MapModuleEndpoints([..ModuleAssemblies()]);// = WebApplicationExtensions.MapModuleEndpoints(app,[..ModuleAssemblies()]);

app.Run();

static Assembly[] ModuleAssemblies() =>
[
    typeof(PriceWatch.Modules.Prices.Infrastructure.DependencyInjection).Assembly,
];

// Exposes Program to WebApplicationFactory in integration tests.
public partial class Program;
