using System.Globalization;
using TimeForge.Database;
using TimeForge.Database.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FastEndpoints;
using Microsoft.OpenApi;
using TimeForge.Models;

var builder = WebApplication.CreateBuilder(args);


// Interceptors
builder.Services.AddSingleton<SoftDeleteInterceptor>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<TimeForgeDbContext>(
    (sp, options) => options
        .UseInMemoryDatabase("TimeForgeDb")
        .AddInterceptors(
            sp.GetRequiredService<SoftDeleteInterceptor>()));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<TimeForgeDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = System.Text.Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Web API specific services
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", bp =>
    {
        bp.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
    });
});

builder.Services.AddFastEndpoints();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TimeForge API", Version = "v1" });

    // JWT Security for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

var cultureInfo = new CultureInfo("en-GB"); // Day/Month/Year
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeForge API v1"));
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}
// app.UseHttpsRedirection();
app.UseCors("DevPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();
app.Run();
