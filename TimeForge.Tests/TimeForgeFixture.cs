using FastEndpoints.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Tests;

public class TimeForgeFixture : AppFixture<Program>
{
    
    protected TimeForgeDbContext Db { get; private set; } = null!;
    protected UserManager<User> UserManager { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;
    
    
    protected override ValueTask SetupAsync()
    {
        return base.SetupAsync();
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        a.UseEnvironment("Test");
        base.ConfigureApp(a);
    }

    protected override void ConfigureServices(IServiceCollection s)
    {
        var descriptor = s.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<TimeForgeDbContext>));

        if (descriptor != null) s.Remove(descriptor);

        // Add a fresh In-Memory database for testing
        s.AddDbContext<TimeForgeDbContext>(options =>
        {
            options.UseInMemoryDatabase("TimeForgeTestDb");
        });
        
        s.AddIdentityCore<User>(opt =>
                opt.User.RequireUniqueEmail = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TimeForgeDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        
        base.ConfigureServices(s);
    }

    protected override ValueTask TearDownAsync()
    {
        return base.TearDownAsync();
    }
}