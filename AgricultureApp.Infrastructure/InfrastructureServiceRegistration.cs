using AgricultureApp.Application.Auth;
using AgricultureApp.Application.Farms;
using AgricultureApp.Application.LLM;
using AgricultureApp.Application.Notifications;
using AgricultureApp.Application.Users;
using AgricultureApp.Infrastructure.Auth;
using AgricultureApp.Infrastructure.Farms;
using AgricultureApp.Infrastructure.LLM;
using AgricultureApp.Infrastructure.Notifications;
using AgricultureApp.Infrastructure.Persistence;
using AgricultureApp.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgricultureApp.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHybridCache(options =>
            {
                options.DefaultEntryOptions = new()
                {
                    Expiration = TimeSpan.FromMinutes(30),
                    LocalCacheExpiration = TimeSpan.FromMinutes(30)
                };
            });

            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<AgricultureAppDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFarmRepository, FarmRepository>();
            services.AddScoped<IFarmService, FarmService>();
            services.AddScoped<IFarmNotificationService, SignalRFarmNotificationService>();
            services.AddScoped<ILlmService, LlmService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
