using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Data;
using WebApplication_Playground.Repository.Entities;
using WebApplication_Playground.Repository.Shared;

namespace WebApplication_Playground.Repository.Repos
{
    public sealed class StudentRepository
    {

        private readonly IConnection _connection;

        private readonly SqlEntityMapper _sqlEntityMapper;

        public StudentRepository(IConnection connection, SqlEntityMapper sqlEntityMapper)
        {
            this._connection = connection;
            this._sqlEntityMapper = sqlEntityMapper;
        }

        private SqlConnection getSqlConnection()
        {
            // must dispose in real usage
            SecureString password = new SecureString();

            foreach (char character in "AppLogin")
                password.AppendChar(character);

            password.MakeReadOnly();

            SqlCredential sqlCredential = new SqlCredential("App", password);

            // apply credentials here if not using Microsoft authentication
            // you CAN apply credentials into connection string but that is NOT
            // a recommended practice per Microsoft 
            // https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlconnection.connectionstring?view=dotnet-plat-ext-6.0#examples
            return new SqlConnection(this._connection.connectionString);

        }
        
        internal IEnumerable<Student> getAllStudents()
        {

            using(SqlConnection sqlConnection = this.getSqlConnection())
            {
                sqlConnection.Open();
                Console.WriteLine("connection worked");

                using(SqlCommand sqlCommand = new SqlCommand())
                {

                    // NOTE: connection & command can be set in constructor
                    // pulled out for demo purposes
                    sqlCommand.Connection = sqlConnection;

                    sqlCommand.CommandText = "select * from custom.student";

                    // sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandType = CommandType.Text; // default

                    // sync operation
                    using(SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        Console.WriteLine("query worked");
                        while(sqlDataReader.Read())
                            yield return this._sqlEntityMapper.createSqlEntity<Student>(sqlDataReader);
                    }

                }

                sqlConnection.Close();

            }

        }

        internal IEnumerable<Student> getStudentsByGender(Student.Gender gender)
        {
            const string command =
                @"select * 
                 from Custom.student
                 where gender <> @gender";

            using (SqlConnection sqlConnection = this.getSqlConnection())
            {

                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {

                    sqlCommand.CommandText = command;
                    sqlCommand.CommandType = CommandType.Text; // default
                    SqlParameter sqlParamter = sqlCommand.Parameters.AddWithValue(
                        "@gender",
                        Enum.GetName(
                            typeof(Student.Gender),
                            gender
                        )!
                    );
                    
                    // can also set data type, direction, and size
                    // not needed
                    sqlParamter.Direction = ParameterDirection.Input;
                    sqlParamter.DbType = DbType.String;

                    using(SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while(sqlDataReader.Read())
                            yield return this._sqlEntityMapper.createSqlEntity<Student>(sqlDataReader);
                    }

                }

                sqlConnection.Close();

            }
        }

        internal IEnumerable<Student> getStudentsByGenderAndNameLength(Student.Gender gender, int length)
        {
            const string command =
                @" select *, len(name) as length 
                     from custom.student
                     where len(name) > @length and gender = @gender";

            using (SqlConnection sqlConnection = this.getSqlConnection())
            {

                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {

                    sqlCommand.CommandText = command;
                    sqlCommand.CommandType = CommandType.Text; // default
                    sqlCommand.Parameters.AddRange(
                        new SqlParameter[] {
                            new SqlParameter("@length", SqlDbType.Int) {Value = length, Direction = ParameterDirection.Input },
                            new SqlParameter("@gender", SqlDbType.NVarChar) {
                                Value = Enum.GetName(typeof(Student.Gender), gender)!,
                                Direction = ParameterDirection.Input
                            }
                        }
                    );

                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                            yield return this._sqlEntityMapper.createSqlEntity<Student>(sqlDataReader);
                    }

                }

                sqlConnection.Close();

            }
        }

    }
}
