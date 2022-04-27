using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Repository.Entities
{
    public sealed class SqlEntityMapper
    {

        public SqlEntityMapper() { }

        public T createSqlEntity<T>(SqlDataReader sqlDataReader) where T : ISqlEntity<T>, new()
        {
            T entity = new T();
            entity.updateWithReader(sqlDataReader);
            return entity;
        }

    }
}
