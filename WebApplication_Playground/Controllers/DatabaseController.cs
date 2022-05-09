using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_Playground.Repository.Adapter;
using WebApplication_Playground.Repository.Entities;
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

        [HttpGet]
        [Route("getStudentsByGender")]
        [Produces("application/json")]
        public IActionResult getStudentsByGender([FromQuery(Name = "gender")] string gender)
        {
            return base.Ok(
                this._adapter.getStudentsByGender(
                        Enum.Parse<Student.Gender>(gender)
                    )
                );
        }

        [HttpGet]
        [Route("getStudentsByGenderAndNameLength")]
        [Produces("application/json")]
        public IActionResult getStudentsByGenderAndNameLength(
            [FromQuery(Name = "gender")] string gender,
            [FromQuery(Name = "length")] int length
            )
        {
            return base.Ok(
                this._adapter.getStudentsByGenderAndNameLength(
                        Enum.Parse<Student.Gender>(gender),
                        length
                    )
                );
        }

        [HttpPut]
        [Route("updateStudentWithLowTestScore")]
        [Produces("text/plain")]
        public IActionResult updateStudentWithLowTestScore(
            [FromQuery(Name = "threshold")] int threshold,
            [FromQuery(Name = "increase")] string increase,
            [FromQuery(Name = "rollback")] string rollback
            )
        {
            Console.WriteLine("update called");
            int updated = this._adapter.updateStudentWithLowTestScore(
                    threshold,
                    Convert.ToBoolean(increase),
                    Convert.ToBoolean(rollback)
                );
            return base.Ok($"{updated}");
        }

        [HttpPost]
        [Route("simulateBatchSave")]
        [Produces("text/plain")]
        public IActionResult simulateBatchSave([FromQuery(Name = "failAt")] string failAt)
        {
            return base.Ok(this._adapter.simulateBatchSave(failAt));
        }

        [HttpGet]
        [Route("getAllStudentProc")]
        [Produces("application/json")]
        public IActionResult getAllStudentProc()
        {
            return base.Ok(this._adapter.getAllStudentProc());
        }

        [HttpGet]
        [Route("getStudentsByGenderProc")]
        [Produces("application/json")]
        public IActionResult getStudentsByGenderProc([FromQuery(Name = "gender")] string gender)
        {
            return base.Ok(this._adapter.getStudentsByGenderProc(Enum.Parse<Student.Gender>(gender)));
        }

        [HttpPut]
        [Route("updateStudentWithLowTestScoreProc")]
        [Produces("text/plain")]
        public IActionResult updateStudentWithLowTestScoreProc(
        [FromQuery(Name = "threshold")] int threshold,
        [FromQuery(Name = "increase")] string increase,
        [FromQuery(Name = "rollback")] string rollback
        )
            {
                int updated = this._adapter.updateStudentWithLowTestScore(
                        threshold,
                        Convert.ToBoolean(increase),
                        Convert.ToBoolean(rollback)
                    );
                return base.Ok($"{updated}");
            }

    }

}
