using Microsoft.AspNetCore.Http;

namespace eCommerce.SharedLibrary.Middleware
{
    public class ListenToOnlyAPIGateway(RequestDelegate next)      //Sirf API Gateway se request allow, baaki sabko 503 error!
    {
        public async Task InvokeAsync(HttpContext context)
        {
            //Extract specific header from the request
            var signedHeader = context.Request.Headers["Api-Gateway"];     //signing-Key = Api-Gateway

            //NULL means ,the request is not from API Gateway
            if (signedHeader.FirstOrDefault() is null)      // FirstOrDefault() : Pehla item le, na mile to null de de
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Service Unavailable. Requests are allowed only from API Gateway.");
            }
            else await next(context);      //Agar header mil gaya, to aage badh ja
        }
    }
}
