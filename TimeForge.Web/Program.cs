using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using TimeForge.Infrastructure;
using TimeForge.Infrastructure.Interceptors;
using TimeForge.Infrastructure.Repositories;
using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Infrastructure.Seeders;

using TimeForge.Models;
using TimeForge.Services;
using TimeForge.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ITimeForgeRepository, TimeForgeRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddScoped<IManagerService, ManagerService>();
builder.Services.AddScoped<ITaskCollectionService, TaskCollectionService>();
builder.Services.AddScoped<IConnectionService, UserConnectionService>();

// Interceptors
builder.Services.AddSingleton<SoftDeleteInterceptor>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<TimeForgeDbContext>(
    (sp, options) => options
        .UseSqlServer(connectionString)
        .AddInterceptors(
            sp.GetRequiredService<SoftDeleteInterceptor>()));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TimeForgeDbContext>();

// Web API specific services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TimeForge API", Version = "v1" });
});

var cultureInfo = new CultureInfo("en-GB"); // Day/Month/Year
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var app = builder.Build();

// Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TimeForgeDbContext>();
        await context.Database.MigrateAsync();
        await IdentitySeeder.SeedManagerAsync(services);
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeForge API v1"));
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();