using eCommerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace eCommerce.SharedLibrary.Middleware
{
    public class GlobalException(RequestDelegate next)   //RequestDelegate : Ek function jo HttpContext le aur Task return kare
    {
        public async Task InvokeAsync(HttpContext context)    // GlobalException.InvokeAsync() automatically chalti hai jab request aati hai, await next() se Order API ko bhejti hai, phir wapas aake error check karti hai!
        {
            //Declare default variables
            string message = "Sorry! Internal Server Error. Baad mein aaiyega :)";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";

            try
            {
                await next(context);   //Next middleware ko call karo
                /*
                 GlobalException.InvokeAsync()
                 await next()
                    ↓
                 RateLimit(100 req / min exceed → StatusCode = 429)
                    ↓
                 Auth(chal gaya kyunki RateLimit ne next() call kiya)
                    ↓
                 OrderAPI(chal gaya)
                    ↓
                 OrderAPI complete
                    ↓
                 Auth complete  
                    ↓
                 RateLimit complete(StatusCode= 429 set)
                    ↓
                 GlobalException resume → 429 detect → Friendly message */

                //check if Response here is too many requests // 429: status code
                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    message = "Too many requests. Please try again later.";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    title = "Too Many Requests";
                    await ModifyHeader(context, message, statusCode, title);
                }

                //if Response here is UnAuthorized //401 satus code
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "You are not authorized to access this resource.";
                    await ModifyHeader(context, message, statusCode, title);
                }

                //if response is forbidden //403 status code
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out of Access";
                    message = "You don't have permission to access this resource.";
                    statusCode = (int)StatusCodes.Status403Forbidden;              
                    await ModifyHeader(context, message, statusCode, title);
                }

                
            }
            catch (Exception ex)
            {
                //Log Original Excptions  / File , Debugger , Console
                //Error ko file/console/database mein save kar do
                LogException.LogExceptions(ex);

                // check if Exception is Timeout
                if (ex is TaskCanceledException || ex is TimeoutException)
                {
                    message = "The request has timed out. Please try again later.";
                    title = "Timeout Error";
                    statusCode = (int)StatusCodes.Status408RequestTimeout;
                }
                //if exception caught then do the default
                //if none of the exceptions then do the default
                await ModifyHeader(context, message, statusCode, title);
            }
        }

        private async Task ModifyHeader(HttpContext context, string message, int statusCode, string title)
        {
            //display scary-free message tothe client
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
            {
                Detail = message,
                Status = statusCode,
                Title = title
            }), CancellationToken.None);
            return;
        }
    }
}
