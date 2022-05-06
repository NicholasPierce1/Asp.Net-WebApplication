using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Filters
{

    [AttributeUsage(validOn: AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class MyServiceFilter : ActionFilterAttribute
    {

        private readonly ILogger _logger;

        public MyServiceFilter([FromServices] ILogger<MyServiceFilter> logger)
        {
            this._logger = logger;
        }

        /*
         * order:
         *  - OnActionExecuting
         *  - OnActionExecuted
         *  - OnResultExecuting
         *  - OnResultExecuted
         */


        // after model binds
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            this._logger.LogInformation("{filter}: coming from {context}",nameof(MyServiceFilter), nameof(this.OnActionExecuting));

            ModelStateDictionary modelState = context.ModelState;
            HttpContext httpContext = context.HttpContext;
            HttpRequest request = httpContext.Request;
            IHeaderDictionary headerDictionary = httpContext.Request.Headers; // can get form, query, body
            
            // from here you can validate/check anything as well as mutate data
            // you can create a result for a response w/ data to be sent as well

            // context.Result = new OkObjectResult("valueReturned");
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            this._logger.LogInformation("{filter}: coming from {context}", nameof(MyServiceFilter), nameof(this.OnActionExecuted));

        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            this._logger.LogInformation("{filter}: coming from {context}", nameof(MyServiceFilter), nameof(this.OnResultExecuting));

        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            this._logger.LogInformation("{filter}: coming from {context}", nameof(MyServiceFilter), nameof(this.OnResultExecuted));

        }

    }
}
