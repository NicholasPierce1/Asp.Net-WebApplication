using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Repository.Entities
{
    public interface ISqlEntity<T> where T : new()
    {

        public void updateWithReader(SqlDataReader sqlDataReader);

    }
}
