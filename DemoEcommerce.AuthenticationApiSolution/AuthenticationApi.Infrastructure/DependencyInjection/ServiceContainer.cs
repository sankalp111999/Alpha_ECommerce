using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Infrastructure.Data;
using AuthenticationApi.Infrastructure.Repositories;
using eCommerce.SharedLibrary.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationApi.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration config)   //these are parameters not method DI
        {
            //Add DB Connectivity here
            //JWT Add Authentication Scheme here
            SharedServiceContainer.AddSharedServices<AuthenticationDbContext>(services, config, config["MySerilog:FileName"]!);

            //Create Dependency Injections for Repositories here
            services.AddScoped<IUser, UserRepository>();

            return services;

        }

        public static IApplicationBuilder UseInfrastructureService(this IApplicationBuilder app)
        {
            //Use Middlewares here
            SharedServiceContainer.UseShardPolicies(app);
            return app;
        }
    }
}
