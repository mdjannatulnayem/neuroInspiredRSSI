using RSSI_Nuro.Authorization;
using RSSI_Nuro.Data;
using RSSI_Nuro.Extensions;
using RSSI_Nuro.Repositories.Contracts;
using RSSI_Nuro.Repositories;
using RSSI_Nuro.Services;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<ApplicationDbContext>(option => {
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
    });

builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<WorkerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

builder.Services.AddSwaggerGen(def => {
    def.AddSecurityDefinition("StaticApiKey", new OpenApiSecurityScheme
    {
        Description = "The Api key to access the controllers",
        Type = SecuritySchemeType.ApiKey,
        Name = "x-api-key",
        In = ParameterLocation.Header,
        Scheme = "StaticApiKeyAuthorizationScheme",
    });

    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Id = "StaticApiKey",
            Type = ReferenceType.SecurityScheme,
        },
        In = ParameterLocation.Header,
    };

    var requirement = new OpenApiSecurityRequirement
    {
        { scheme, new List<string>() }
    };

    def.AddSecurityRequirement(requirement);
});

builder.Services.AddScoped<ISatelliteDataRepository, SatelliteDataRepository>();
builder.Services.AddScoped<IEarthDataRepository, EarthDataRepository>();
builder.Services.AddAutoMapper(typeof(MappingConfiguration));
builder.Services.AddScoped<AuthFilter>();

// Configure CORS policy
builder.Services.AddCors(p => p.AddPolicy("corspolicy", build => {
    build.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("corspolicy");

// app.UseMiddleware<AuthMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
