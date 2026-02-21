using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace eCommerce.SharedLibrary.DependencyInjection
{
    public static class JWTAuthenticationScheme
    {
        public static IServiceCollection AddJWTAuthenticationScheme(this IServiceCollection services, IConfiguration config)     // Extension method banaya DI container ke liye. services mein JWT add karega.
                                                                                                                                 //IServiceCollection mein HAMARA BANAYA METHOD ADD HO GAYA!
                                                                                                                                 //Ab services.AddJWTAuthenticationScheme(services,config) call kar sakte hain!
        {
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)  //Authentication ON kiya. Default scheme = JWT Bearer.
                .AddJwtBearer("Bearer", options =>   //JWT Bearer specific configuration start.
                {
                    var key = Encoding.UTF8.GetBytes(config.GetSection("Authentication:Key").Value!); //appsettings.json se secret key liya → byte array banaya(encryption ke liye).
                    string issuer = config.GetSection("Authentication:Issuer").Value!;         //Issuer = "Kon banaya token?" (Gateway), 
                    string audience = config.GetSection("Authentication:Audience").Value!;    //Audience = "Kon use karega?" (Order API).

                    options.RequireHttpsMetadata = false;   //Development mein HTTP allow (Production mein true karna).
                    options.SaveToken = true; //Token ko HttpContext mein save karega(baad mein use ke liye).
                    options.TokenValidationParameters = new TokenValidationParameters   //Token validation rules start.
                    {

                        //4 cheezein check karega: Issuer, Audience, Expiry, Signing key.
                        ValidateIssuer = true,      
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        // Expected values set kiye (config se aaye).
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                    };

                });
            return services;
        }
    }
}
