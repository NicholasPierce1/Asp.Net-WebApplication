using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Repository.Entities
{
    public sealed class Dog : ISqlEntity<Dog>
    {

        public enum Gender: int
        {
            male = 0,
            female = 1
        }

        public Guid id { get; set; }

        public string name { get; set; }

        public string breed { get; set; }

        public Gender gender { get; set; }

        public void updateWithReader(SqlDataReader sqlDataReader)
        {
            this.id = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal(nameof(this.id)));
            this.name = sqlDataReader.GetString(sqlDataReader.GetOrdinal(nameof(this.name)));
            this.breed = sqlDataReader.GetString(sqlDataReader.GetOrdinal(nameof(this.breed)));
            this.gender = Enum.Parse<Dog.Gender>(sqlDataReader.GetString(sqlDataReader.GetOrdinal(nameof(this.gender))));
        }

    }
}
