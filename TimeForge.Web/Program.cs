using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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


// Interceptors
builder.Services.AddSingleton<SoftDeleteInterceptor>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<TimeForgeDbContext>(
    (sp ,options) => options
        .UseSqlServer(connectionString)
        .AddInterceptors(
            sp.GetRequiredService<SoftDeleteInterceptor>()));



builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options =>
    {
        //TODO remove after testing
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TimeForgeDbContext>();
builder.Services.AddControllersWithViews();



var app = builder.Build();

//Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        //Get TimeForgeDbContext for IoC
        var context = services.GetRequiredService<TimeForgeDbContext>();

        //Apply any pending migrations to ensure database is up to date
        await context.Database.MigrateAsync(); 
        
        //TODO Add IdentitySeeder to DbInitializer
        
        //Seed Manager if it doesent exist
        await IdentitySeeder.SeedManagerAsync(services);
        
        //Seed projects with their own tags and tasks if none exist
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
}
else
{
    app.UseExceptionHandler("/Error/500");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// app.MapControllerRoute(
//     name: "MyArea",
//     pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();