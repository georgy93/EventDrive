namespace EventDrive.API.Controllers
{
    using Common;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Mime;

    /// <summary>
    /// Use this as a base class for all the controllers and put common logic here
    /// </summary>
    [ApiController] // this will return automatically error response when model state is invalid
                    // We can tackle it by adding this configuration
                    // services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesErrorResponseType(typeof(ErrorResponse))]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public abstract class BaseController : ControllerBase
    {
    }
}