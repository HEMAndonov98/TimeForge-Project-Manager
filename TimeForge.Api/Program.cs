using System.Text;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;
using Microsoft.IdentityModel.Tokens;
using TimeForge.Database;
using TimeForge.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<TimeForgeDbContext>(opt =>
{
    opt.UseInMemoryDatabase("TimeForgeDb");
});

// ------------------ Identity ------------------
builder.Services.AddIdentityCore<User>(opt =>
        opt.User.RequireUniqueEmail = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TimeForgeDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// ------------------ JWT Authentication ------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();


// ------------------ CORS ------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", bp =>
    {
        bp.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ------------------ FastEndpoints & Swagger ------------------
builder.Services.AddFastEndpoints()
.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "TimeForge API";
        s.Version = "v1";
    };
});

var app = builder.Build();

// ------------------ Middleware ------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Uncomment if you want HTTPS redirection
// app.UseHttpsRedirection();

app.UseCors("DevPolicy");
app.UseAuthentication();
app.UseAuthorization();

// FastEndpoints + Swagger (Docs recommend UseFastEndpoints first)
app.UseFastEndpoints()
.UseSwaggerGen();

app.Run();

public partial class Program { }