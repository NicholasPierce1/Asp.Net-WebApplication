using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using WebApplication_Playground.Models.RestModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using WebApplication_Playground.Models.Configuration;
using Microsoft.Extensions.Options;
using WebApplication_Playground.DepedencyInjection;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication_Playground.Controllers
{
    // [controller] -> dynamically inserts the class name in
    [AllowAnonymous]
    [Route("api/Values")]
    [ApiController]
    public class ValuesRestController : ControllerBase
    {

        private readonly IOptions<CustomConfiguration> customConfig;

        private readonly CustomInjectionInterface customInjection;

        private readonly IWrappedCustomInjection wrappedCustomInjection;


        // NOTE: [FromServices] is OPTIONAL
        public ValuesRestController(
            IOptions<CustomConfiguration> customConfig,
            [FromServices] CustomInjectionInterface customInjection,
            IWrappedCustomInjection wrappedCustomInjection
            )
        {
            this.customConfig = customConfig;
            this.customInjection = customInjection;
            this.wrappedCustomInjection = wrappedCustomInjection;
        }

        // GET: api/<ValuesRestController>
        [HttpGet]
        [Route("get")] // flexible for trailing "/"
        public IEnumerable<string> Get()
        {
            Console.WriteLine("service called");
            return new string[] { "value1", "value2" };
        }

        // GET api/<ValuesRestController>/5

        [HttpGet]
        [Route("get/{id}")] // {id?} for optional
        public object Get(int id)
        {
            return new { field1 = "hello", field2 = 3 };
        }

        /*
         * shows how to extract values from a request params (aka query)
         * shows how to extract values from a request header
         * shows how to extract multiple values from a request param with the same key (variadic)
         */
        [HttpGet]
        [Route("sum")]
        public int SumOf([FromQuery(Name = "x")] int x = 10, [FromHeader(Name = "MyHeader")] int headerValue = 30, [FromQuery] params int[] y)
        {
            return x + y.Aggregate<int>((x, y) => x + y) + headerValue;
        }

        [HttpGet]
        [Route("testRequest")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public IActionResult testRequest()
        {
            // NOTE: can use form too -> see "createStudentAlternative" for Form aggregation
            Dictionary<string, object> results = new Dictionary<string, object>();

            HttpContext context = base.HttpContext;
            
            HttpRequest request = context.Request;

            IHeaderDictionary headers = request.Headers;

            IQueryCollection queryParams = request.Query;

            Stream bodyStream = request.Body;
            StreamReader bodyStreamReader = new StreamReader(bodyStream);

            IRequestCookieCollection cookies = request.Cookies;

            // header (headerOne, headerTwo)
            results.Add("headers", new Dictionary<string, object>());
            StringValues x, y;
            ((Dictionary<string,object>)results["headers"])
                .Add("headerOne",headers.TryGetValue("headerOne", out x) ? x.First<string>() : null);

            // headers can be variadic too
            ((Dictionary<string, object>)results["headers"])
                .Add("headerTwo", headers.TryGetValue("headerTwo", out y) ? y.ToArray(): null);

            foreach(KeyValuePair<string,StringValues> keyPair in headers)
            {
                if (!((Dictionary<string, object>)results["headers"]).ContainsKey(keyPair.Key)) {

                    ((Dictionary<string, object>)results["headers"])[keyPair.Key] = new List<string>();

                    foreach (string val in keyPair.Value)
                        ((List<string>)((Dictionary<string, object>)results["headers"])[keyPair.Key]).Add(val);

                    // if size is 1 then (in an incredibly ugly-looking way) just store it as one string
                    if (((List<string>)((Dictionary<string, object>)results["headers"])[keyPair.Key]).Count == 1)
                        ((Dictionary<string, object>)results["headers"])[keyPair.Key] =
                            ((List<string>)((Dictionary<string, object>)results["headers"])[keyPair.Key])[0];
                }
            }

            // query params (x,y)
            results["queryParams"] = new Dictionary<string, object>();

            ((Dictionary<string, object>)results["queryParams"])["x"] = queryParams["x"].FirstOrDefault<string>();
            ((Dictionary<string, object>)results["queryParams"])["y"] = queryParams["y"].DefaultIfEmpty();

            // body (Dictionary<string,object>) -> blocking
            string body = bodyStreamReader.ReadToEnd();

            Console.WriteLine($"body: ({body})");

            results["body"] = JObject.Parse(body).ToString(Formatting.Indented);

            // cookies

            // read & store
            // cookies with tomcat >= 8 don't support spaces. Must encode to UT8 between cookie calls
            // %20 or + = space in URL encoded && %2B = plus sign
            Dictionary<string, string> cookieDictionary = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in cookies)
                cookieDictionary[pair.Key] = pair.Value;
            results["cookies"] = cookieDictionary;

            // add cookie (delete if already exists)
            if (cookies.ContainsKey("CookieFromService"))
                context.Response.Cookies.Delete("CookieFromService");

            context.Response.Cookies.Append(
                "CookieFromService",
                $"Today is: {DateTime.Parse("3/18/2022", new CultureInfo("en-US")).ToString("dd/MMM/yyyy")}"
            );

            /*
             * can use Response object modify cookies, headers, and even body via a BodyWriter (Stream)
             */

            return base.Ok(results);

        }

        // POST api/<ValuesRestController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        [HttpPost]
        [Route("createStudent")]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json")]
        public IActionResult createStudent(
            [FromForm(Name = "firstName")] string firstName,
            [FromForm(Name = "lastname")] string lastName,
            [FromForm(Name = nameof(Student.totalCredits))] int totalCredits,
            // any date,time, or date time format that is accepted
            [FromForm(Name = nameof(Student.expectedGraduationDate))] DateTime gradDate)
        {

            return base.Ok(
                new Student() { 
                    firstName = firstName,
                    lastName = lastName,
                    totalCredits = totalCredits,
                    expectedGraduationDate = gradDate
                });

        }

        [HttpPost]
        [Route("createStudent2")]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json")]
        public IActionResult createStudentAlternative()
        {

            JObject json = new JObject(
                from KeyValuePair<string, StringValues> pair in base.HttpContext.Request.Form
                select
                    new JProperty(pair.Key, pair.Value.FirstOrDefault<string>())
            );

            return base.Ok(JsonConvert.DeserializeObject<Student>(json.ToString()));

        }

        [HttpPut]
        [Route("putStudent")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public IActionResult addCreditsToStudent([FromBody] Student student)
        {

            student.addCredits(50);

            return base.Ok(student);

        }


        [HttpPost]
        [Route("validatePerson")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public IActionResult PersonValidation([FromBody] Person person)
        {

           // Console.WriteLine(person.);

            return base.Ok(person);
        }

        [HttpGet]
        [Route("getUser")]
        [Produces("application/json")]
        public IActionResult GetPrincipalUser()
        {
            ClaimsPrincipal user = base.User;

            List<Dictionary<string, object>> userClaims = new List<Dictionary<string, object>>();

            foreach (Claim claim in user.Claims)
                userClaims.Add(
                    new Dictionary<string, object>()
                    {
                        {nameof(claim.Type), claim.Type },
                        {nameof(claim.Value), claim.Value },
                        {nameof(claim.Issuer), claim.Issuer },
                        {nameof(claim.Subject), claim.Subject },
                        {nameof(claim.Properties), claim.Properties }
                    }
                );

            return base.Ok(
                new Dictionary<string, object>() {
                    {"name", user.Identity.Name},
                    {"isAuthenticated", user.Identity.IsAuthenticated}
                }
            );
        }

        [HttpGet]
        [Route("getConstants")]
        [Produces("application/json")]
        public IActionResult GetRuntimeConstants()
        {
            return base.Ok(this.customConfig.Value);
        }

        [HttpGet]
        [Route("getCustomInjection")]
        [Produces("text/plain")]
        public IActionResult GetCustomInjection()
        {
            // for consumers (non-controllers) just register them as injections as well to be 
            // applicable to receiving DIs
            // return base.Ok(this.customInjection.currentLocale);

            // MANUAL WAY
            // can create manual, lazy singletons that, upon first request, fires the constructor
            // which manually retrieves all dependencies (NOT RECOMMENDED)
            // see above for preferred way
            IServiceProvider services = this.HttpContext.RequestServices;
            CustomInjectionInterface customInjectionInterface =
                (CustomInjectionInterface)services.GetService(typeof(CustomInjectionInterface));

            return base.Ok(customInjectionInterface.currentLocale);
        }

        [HttpGet]
        [Route("getWrappedCustomInjection")]
        [Produces("application/json")]
        public IActionResult GetWrappedCustomInjection()
        {
            // for consumers (non-controllers) just register them as injections as well to be 
            // applicable to receiving DIs
            return base.Ok(this.wrappedCustomInjection.wrappedCultureInfo);
        }

        /*
        private Dictionary<string,object> createSubDictionary(IReadOnlyList<ModelStateEntry> subErrors)
        {

            Dictionary<string, object> validationResult = new Dictionary<string, object>();

            foreach (ModelStateEntry errors in subErrors)
            {
                errors.
                validationResult.Add(
                    errors.Key,
                    new Dictionary<string, object>()
                    {
                            {"Errors", errors.Errors},
                            {"AttemptedValue", errors.AttemptedValue},
                            {"ValidationState", errors.ValidationState }
                    }
                );

                if (errors.Children != null && errors.Children.Count != 0)
                    validationResult[errors.Key] = this.createSubDictionary(errors.Children);

            }

        }*/

    }
}
