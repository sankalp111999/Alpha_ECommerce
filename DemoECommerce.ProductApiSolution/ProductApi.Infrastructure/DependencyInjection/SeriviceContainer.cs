using eCommerce.SharedLibrary.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Application.Interfaces;
using ProductApi.Infrastructure.Data;
using ProductApi.Infrastructure.Repositories;

namespace ProductApi.Infrastructure.DependencyInjection
{
    public static class SeriviceContainer
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration config)
        {
            //Add DB connectivity
            //Add authentication scheme
            SharedServiceContainer.AddSharedServices<ProductDbContext>(services, config, config["MySerilog:FileName"]!);

            //Create DI
            services.AddScoped<IProduct, ProductRepository>();   //Repository ko DI container mein register kiya. 
            return services;
        }

        public static IApplicationBuilder UseInfrastructurePolicy(this IApplicationBuilder app)
        {
            //register middleware such as:
            //Global Exception :handles external errors.
            //ListenToOnlyAPIGateway : blocks direct access.
            SharedServiceContainer.UseShardPolicies(app);   //Middleware pipeline setup kiya.
            return app;
        }
    }
}
