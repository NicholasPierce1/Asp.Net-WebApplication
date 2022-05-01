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

            using (SqlConnection sqlConnection = this.getSqlConnection())
            {
                sqlConnection.Open();
                Console.WriteLine("connection worked");

                using (SqlCommand sqlCommand = new SqlCommand())
                {

                    // NOTE: connection & command can be set in constructor
                    // pulled out for demo purposes
                    sqlCommand.Connection = sqlConnection;

                    sqlCommand.CommandText = "select * from custom.student";

                    // sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandType = CommandType.Text; // default

                    // sync operation
                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        Console.WriteLine("query worked");
                        while (sqlDataReader.Read())
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

                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
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

        internal int updateStudentWithLowTestScore(int threshold, bool increase, bool rollback)
        {
            int toReturn;

            string commandText = increase ?
                @"update Custom.student 
                 set total_score = total_score + 10 
                 where total_score = @threshold" :
                 @"update Custom.student 
                 set total_score = total_score - 10 
                 where total_score = @threshold";

            using (SqlConnection sqlConnection = this.getSqlConnection())
            {
                sqlConnection.Open();

                using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                        {
                            sqlCommand.Transaction = sqlTransaction;
                            sqlCommand.CommandText = commandText;
                            SqlParameter sqlParameter = sqlCommand.Parameters.AddWithValue("@threshold", threshold);

                            sqlParameter.SqlDbType = SqlDbType.Int;
                            sqlParameter.Direction = ParameterDirection.Input;

                            toReturn = sqlCommand.ExecuteNonQuery();

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        sqlTransaction.Rollback();
                        return -1; // error
                    }

                    if (rollback)
                        sqlTransaction.Rollback();
                    else
                        sqlTransaction.Commit();
                }

                sqlConnection.Close();

                return toReturn;
            }

        }

        /*
         * mock demo: 2 commands (cmd1, comd2) to simulate batch processing
         *   if cmd1 works than save
         *   if cmd2 works than commit all
         *   if cmd2 fails than rollback cmd2 but commit 1
         *   if cmd1 fails then rollback all
         */

        internal string simulateBatchSave(string failAt = "")
        {
            // if null then set to empty (no error)
            failAt = failAt ?? "";

            using (SqlConnection sqlConnection = this.getSqlConnection())
            {

                sqlConnection.Open();

                // delete where student name is like testBatch#
                // serves as a refresh before insertion/s
                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText =
                        @"delete from Custom.student
                        where name like 'testBatch%'";

                    sqlCommand.ExecuteNonQuery();
                }

                using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
                {

                    try
                    {
                        // batch 1
                        using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                        {

                            if (failAt.Equals("batchOne"))
                                throw new Exception(failAt);

                            sqlCommand.Transaction = sqlTransaction;

                            sqlCommand.CommandText =
                                @"insert Custom.student
                            (name, gender) values ('testBatch1', 'male')";

                            sqlCommand.ExecuteNonQuery();

                            sqlTransaction.Save("batchOne");

                        }

                        try
                        {
                            // batch 2
                            using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                            {

                                if (failAt.Equals("batchTwo"))
                                    throw new Exception(failAt);

                                sqlCommand.Transaction = sqlTransaction;

                                sqlCommand.CommandText =
                                    @"insert Custom.student
                            (name, gender) values ('testBatch2', 'female')";

                                sqlCommand.ExecuteNonQuery();

                                sqlTransaction.Commit();

                            }
                        }
                        catch (Exception e)
                        {

                            if (!e.Message.Equals(failAt))
                                Console.WriteLine(e);

                            sqlTransaction.Rollback("batchOne");
                            sqlTransaction.Commit();
                        }
                    }
                    catch (Exception e)
                    {

                        if (!e.Message.Equals(failAt))
                            Console.WriteLine(e);

                        sqlTransaction.Rollback();
                    }

                }

                sqlConnection.Close();

            }

            return String.IsNullOrEmpty(failAt) ?
                    $"Success on both operations" : $"failed at: {failAt}";

        }

        // reader.NextResultAsync();
        // get next data set result

        // PROCEDURES INVOCATIONS
        internal IEnumerable<Student> getAllStudentsProc()
        {

            using (SqlConnection sqlConnection = this.getSqlConnection())
            {

                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {

                    sqlCommand.CommandText = "Custom.GetAllStudents";
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                            yield return this._sqlEntityMapper.createSqlEntity<Student>(sqlDataReader);
                    }

                }

                sqlConnection.Close();

            }

        }

        internal IEnumerable<Student> getStudentsByGenderProc(Student.Gender gender)
        {

            using (SqlConnection sqlConnection = this.getSqlConnection())
            {

                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {

                    sqlCommand.CommandText = "Custom.GetStudentsByGender";
                    sqlCommand.CommandType = CommandType.StoredProcedure; // default
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
                    sqlParamter.SqlDbType = SqlDbType.VarChar;

                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                            yield return this._sqlEntityMapper.createSqlEntity<Student>(sqlDataReader);
                    }

                }

                sqlConnection.Close();

            }
        }

        internal int updateStudentWithLowTestScoreProc(int threshold, bool increase, bool rollback)
        {
            int toReturn;

            using (SqlConnection sqlConnection = this.getSqlConnection())
            {
                sqlConnection.Open();

                using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                        {
                            sqlCommand.Transaction = sqlTransaction;
                            sqlCommand.CommandText = "Custom.UpdateStudentWithLowTestScore";

                            SqlParameter returnValue = 
                                new SqlParameter() { SqlDbType = SqlDbType.Int, Direction = ParameterDirection.ReturnValue };


                            sqlCommand.Parameters.AddRange(
                                    new SqlParameter[]{
                                        new SqlParameter("@threshold", threshold){SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input},
                                        new SqlParameter("@toIncrease", increase ? 1 : 0){SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input},
                                        returnValue
                                    }
                                );
                            
                            toReturn = (int)returnValue.Value;

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        sqlTransaction.Rollback();
                        return -1; // error
                    }

                    if (rollback)
                        sqlTransaction.Rollback();
                    else
                        sqlTransaction.Commit();
                }

                sqlConnection.Close();

                return toReturn;
            }

        }

    }
}
