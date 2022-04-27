using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Repository.Shared
{
    public interface IConnection
    {

        public abstract string connectionString { get; }

    }
}
