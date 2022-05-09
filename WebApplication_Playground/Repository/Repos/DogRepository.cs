using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_Playground.Repository.Entities;
using WebApplication_Playground.Repository.Shared;

namespace WebApplication_Playground.Repository.Repos
{
    public sealed class DogRepository
    {

        private readonly IConnection _connection;

        private readonly SqlEntityMapper _sqlEntityMapper;

        public DogRepository([FromServices] IConnection connection, [FromServices] SqlEntityMapper sqlEntityMapper)
        {
            this._connection = connection;
            this._sqlEntityMapper = sqlEntityMapper;
        }

        private SqlConnection getConnection()
        {
            return new SqlConnection(this._connection.connectionString);
        }

        public IEnumerable<Dog> getAllDogs()
        {

            using (SqlConnection sqlConnection = this.getConnection())
            {

                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {

                    sqlCommand.CommandText = "custom.GetAllDogs";
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;

                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                            yield return this._sqlEntityMapper.createSqlEntity<Dog>(sqlDataReader);
                    }

                }

                sqlConnection.Close();

            }

        }

        public Dog getDogById([NotNull] Guid id)
        {

            Dog createdDog;

            using (SqlConnection sqlConnection = this.getConnection())
            {

                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {

                    sqlCommand.CommandText = "custom.GetDogById";
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;

                    sqlCommand.Parameters.AddRange(
                            new SqlParameter[]
                            {
                                new SqlParameter("@id", SqlDbType.UniqueIdentifier) {Value = id, Direction = ParameterDirection.Input }
                            }
                        );

                    using(SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        if (!sqlDataReader.Read())
                            throw new Exception($"Dog with id({id}) does not exist.");

                        createdDog = this._sqlEntityMapper.createSqlEntity<Dog>(sqlDataReader);
                    }

                }

                sqlConnection.Close();

            }

            return createdDog;

        }

        public Guid createDog([NotNull] Dog dog)
        {

            Guid createdGuid;

            using (SqlConnection sqlConnection = this.getConnection())
            {

                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {

                    sqlCommand.CommandText = "custom.InsertDog";
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;

                    sqlCommand.Parameters.AddRange(
                            new SqlParameter[]
                            {
                                new SqlParameter("@id", SqlDbType.UniqueIdentifier) {Direction = ParameterDirection.InputOutput },
                                new SqlParameter("@name", SqlDbType.NVarChar) {Value = dog.name, Direction = ParameterDirection.Input },
                                new SqlParameter("@breed", SqlDbType.NVarChar) {Value = dog.breed, Direction = ParameterDirection.Input },
                                new SqlParameter("@gender", SqlDbType.NVarChar) {
                                    Value = Enum.GetName(typeof(Dog.Gender), dog.gender)!,
                                    Direction = ParameterDirection.Input 
                                }

                            }
                        );

                    Console.WriteLine($"how many dogs created? (should be one) {sqlCommand.ExecuteNonQuery()}");

                    createdGuid = (Guid)sqlCommand.Parameters["@id"].Value;

                }

                sqlConnection.Close();

            }

            return createdGuid;

        }

        public void updateDogByName([NotNull] string name)
        {

            using (SqlConnection sqlConnection = this.getConnection())
            {

                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {

                    sqlCommand.CommandText = "custom.UpdateDogByName";
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;

                    sqlCommand.Parameters.AddRange(
                            new SqlParameter[]
                            {
                                new SqlParameter("@name", SqlDbType.NVarChar) {Value = name, Direction = ParameterDirection.Input }
                            }
                        );
                    int rows;
                    if ((rows = sqlCommand.ExecuteNonQuery()) != 2) // 1 row changed then reverted
                        throw new Exception($"expected 1 row to be affected but that was not the case. {rows}");

                }

                sqlConnection.Close();

            }

        }

    }
}
