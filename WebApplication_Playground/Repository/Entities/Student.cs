using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Repository.Entities
{
    public sealed class Student : ISqlEntity<Student>
    {

        public enum Gender : int
        {
            male = 0,
            female = 1
        }

        public int id { get; set; }

        public string name { get; set; }

        public Gender gender { get; set; }

        public int total_score { get; set; }

        public Student() { }

        public void updateWithReader(SqlDataReader sqlDataReader)
        {
            // NOTE: check Entity -> Dog.cs
            // much better way of extrating elements without downcast and/or implicit-conversion
            this.id = (int)sqlDataReader[nameof(this.id)];
            this.name = (string)sqlDataReader[nameof(this.name)];
            this.gender = Enum.Parse<Gender>((string)sqlDataReader[nameof(this.gender)]);
            this.total_score = (int)sqlDataReader[nameof(this.total_score)];
        }

    }
}
