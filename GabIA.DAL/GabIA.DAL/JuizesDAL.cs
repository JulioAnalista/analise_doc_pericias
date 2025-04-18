using MySql.Data.MySqlClient;
            using System;
            using System.Collections.Generic;
            using System.Data;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using Dapper;
            using GabIA.ENT;
            using System.Configuration;

            namespace GabIA.DAL
            {
                public class JuizesDAL
                {
                    private readonly IDbConnection _dbConnection;
                    private string connectionString;

                    public JuizesDAL()
                    {
                        connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
                        _dbConnection = new MySqlConnection(connectionString);
                    }
                    public int Inserir(JuizesENT entidade)
                    {
                        var sql = $@"INSERT INTO juizes (IdJuiz, IdPessoa, IdProcesso, DatetimeInicial, DatetimeFinal)
                            VALUES (@IdJuiz, @IdPessoa, @IdProcesso, @DatetimeInicial, @DatetimeFinal);
                            SELECT LAST_INSERT_ID();";

                        return _dbConnection.ExecuteScalar<int>(sql, entidade);
                    }
        public void AddjuizesENT(JuizesENT entidade)
                    {
                        var sql = $@"INSERT INTO juizes (IdJuiz, IdPessoa, IdProcesso, DatetimeInicial, DatetimeFinal)
                                    VALUES (@IdJuiz, @IdPessoa, @IdProcesso, @DatetimeInicial, @DatetimeFinal)";

                        _dbConnection.Execute(sql, entidade);
                    }

                    public List<JuizesENT> GetjuizesENTs()
                    {
                        const string sql = "SELECT * FROM juizes";
                        return _dbConnection.Query<JuizesENT>(sql).ToList();
                    }

                    public async Task<IEnumerable<JuizesENT>> ObterTodosAsync()
                    {
                        const string sql = "SELECT * FROM juizes";
                        return await _dbConnection.QueryAsync<JuizesENT>(sql);
                    }

                    public void UpdatejuizesENT(JuizesENT entidade)
                    {
                        var sql = $@"UPDATE juizes SET IdJuiz = @IdJuiz, IdPessoa = @IdPessoa, IdProcesso = @IdProcesso, DatetimeInicial = @DatetimeInicial, DatetimeFinal = @DatetimeFinal
                                    WHERE IdJuiz = @IdJuiz";

                        _dbConnection.Execute(sql, entidade);
                    }

                    public void DeletejuizesENT(int id)
                    {
                        var sql = $@"DELETE FROM juizes WHERE IdJuiz = @id";
                        _dbConnection.Execute(sql, new { id });
                    }
                }
            }