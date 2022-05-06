using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_Playground.Filters;

namespace WebApplication_Playground.Controllers
{
    [Route("api/advance")]
    [AllowAnonymous]
    [ApiController]
    [ServiceFilter(typeof(MyServiceFilter))]
    public class AdvanceController : ControllerBase
    {

        private readonly ILogger _logger;

        public AdvanceController([FromServices] ILogger<AdvanceController> logger)
        {
            this._logger = logger;
        }

        [HttpGet]
        [Route("")]
        [Produces("application/json")]
        public IActionResult testFilter()
        {
            try {
                this._logger.LogInformation("Hello from {controllerName}!", nameof(AdvanceController));
                return base.Ok(new { isOk = true});
            }
            catch(Exception ex)
            {
                return base.Problem(detail: "error occured here!", statusCode: StatusCodes.Status400BadRequest, title: "something bad happened");
            }
        }

    }
}
