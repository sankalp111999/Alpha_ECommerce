using eCommerce.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace eCommerce.SharedLibrary.DependencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedServices<TContext> (this IServiceCollection services, IConfiguration config, string fileName) where TContext : DbContext     //Generic extension method. TContext = OrderDbContext, ProductDbContext etc.
        {
            // MSSQL connection + Retry logic (DB down ho to 3 baar try karega).
            services.AddDbContext<TContext>(option => option.UseSqlServer(
                config.GetConnectionString("eCommerceConnection"),sqlServerOption =>
                sqlServerOption.EnableRetryOnFailure()));

            //configure Serilog logging
            // Serilog setup start. Information level se logging.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()        //Logs Debug window + Console mein dikhega.
                .WriteTo.Console()
                .WriteTo.File(path: $"{fileName}- .text",    //Daily log files banega: OrderAPI-2026-01-16.log
                 //Fancy log format + Daily rotation.
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();      

            //JWT Authentication Scheme
            JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, config);     // JWT setup call kiya.

            return services;
        }

        public static IApplicationBuilder UseShardPolicies(this IApplicationBuilder app)   //Middleware pipeline setup.
        {
            //Register Middlewares
            app.UseMiddleware<GlobalException>();     //GlobalException = Error handler
            app.UseMiddleware<ListenToOnlyAPIGateway>();     //ListenToOnlyAPIGateway = Direct access block

            return app;
        }
    }
}
