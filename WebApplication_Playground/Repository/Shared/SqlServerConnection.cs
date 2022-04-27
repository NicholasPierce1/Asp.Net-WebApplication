using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Repository.Shared
{
    public sealed class SqlServerConnection : IConnection
    {

        public string connectionString { get; }

        public SqlServerConnection([FromServices] ConnectionHelper connectionHelper)
        {
            this.connectionString = connectionHelper.getConnection(nameof(SqlServerConnection));
            Console.WriteLine($"{nameof(SqlServerConnection)}: {this.connectionString}");
        }

    }
}
