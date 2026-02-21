using eCommerce.SharedLibrary.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Application.Interfaces;
using OrderApi.Infrastructure.Data;
using OrderApi.Infrastructure.Repositories;

namespace OrderApi.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration config)
        {
            //Add DB Connectivity
            //Add Authentication Scheme
            SharedServiceContainer.AddSharedServices<OrderDbContext>(services, config, config["MySerilog:FileName"]!);


            //Create Dependency Injection
            services.AddScoped<IOrder, OrderRepository>();

            return services;
        }


        public static IApplicationBuilder UserInfrastructurePolicy(this IApplicationBuilder app)
        {
            //register middleware such as:
            //Global Exception :handles external errors.
            //ListenToOnlyAPIGateway : blocks direct access.
            SharedServiceContainer.UseShardPolicies(app);   //Middleware pipeline setup kiya.
            return app;
        }
    }
}
