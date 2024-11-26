namespace EventDrive.Utils.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

public static class HttpContextExtensions
{
    private static readonly RouteData EmptyRouteData = new();
    private static readonly ActionDescriptor EmptyActionDescriptor = new();

    public static async Task WriteResultAsync<TResult>(this HttpContext context,
                                                       IActionResultExecutor<ObjectResult> resultExecutor,
                                                       TResult result,
                                                       int statusCode = StatusCodes.Status500InternalServerError)
    {
        var routeData = context.GetRouteData() ?? EmptyRouteData;
        var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);
        var objectResult = new ObjectResult(result)
        {
            StatusCode = statusCode,
            DeclaredType = typeof(TResult)
        };

        await resultExecutor.ExecuteAsync(actionContext, objectResult);
    }
}