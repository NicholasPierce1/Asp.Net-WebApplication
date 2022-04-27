using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_Playground.Repository.Adapter;
using WebApplication_Playground.Repository.Shared;

namespace WebApplication_Playground.Controllers
{
    [AllowAnonymous]
    [Route("api/database")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        
        // NOTE: not to use in abstract repo-pattern.
        // sanity test for dependency creation
        private readonly SqlServerConnection _sqlServerConnection;

        private readonly Adapter _adapter;

        public DatabaseController(
            [FromServices] SqlServerConnection sqlServerConnection,
            [FromServices] Adapter adapter
            )
        {
            this._sqlServerConnection = sqlServerConnection;
            this._adapter = adapter;
        }

        [HttpGet]
        [Route("getSqlServerConnection")]
        [Produces("text/plain")]
        public IActionResult getSqlServerConnection()
        {
            Console.WriteLine($"{nameof(DatabaseController)}: {this._sqlServerConnection.connectionString}");

            return base.Ok(this._sqlServerConnection.connectionString);
        }

        [HttpGet]
        [Route("getAllStudents")]
        [Produces("application/json")]
        public IActionResult getAllStudents()
        {
            return base.Ok(this._adapter.getAllStudents());
        }
        
    }
}
