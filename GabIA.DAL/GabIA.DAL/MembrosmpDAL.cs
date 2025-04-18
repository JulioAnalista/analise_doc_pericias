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
                public class Membros_MP_DAL
                {
                    private readonly IDbConnection _dbConnection;
                    private string connectionString;

                    public Membros_MP_DAL()
                    {
                        connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
                        _dbConnection = new MySqlConnection(connectionString);
                    }

                    public void AddmembrosmpENT(MembrosMpENT entidade)
                    {
                        var sql = $@"INSERT INTO membrosmp (IdMembro, IdPessoa, IdProcesso, DataInicial, DataFinal, Atual)
                                    VALUES (@IdMembro, @IdPessoa, @IdProcesso, @DataInicial, @DataFinal, @Atual)";

                        _dbConnection.Execute(sql, entidade);
                    }


                    public int Inserir(MembrosMpENT entidade)
                    {
                        var sql = $@"INSERT INTO membrosmp (IdMembro, IdPessoa, IdProcesso, DataInicial, DataFinal, Atual)
                            VALUES (@IdMembro, @IdPessoa, @IdProcesso, @DataInicial, @DataFinal, @Atual);
                            SELECT LAST_INSERT_ID();";

                        return _dbConnection.ExecuteScalar<int>(sql, entidade);
                    }
        public List<MembrosMpENT> GetmembrosmpENTs()
                    {
                        const string sql = "SELECT * FROM membrosmp";
                        return _dbConnection.Query<MembrosMpENT>(sql).ToList();
                    }

                    public async Task<IEnumerable<MembrosMpENT>> ObterTodosAsync()
                    {
                        const string sql = "SELECT * FROM membrosmp";
                        return await _dbConnection.QueryAsync<MembrosMpENT>(sql);
                    }


                    public void UpdatemembrosmpENT(MembrosMpENT entidade)
                    {
                        var sql = $@"UPDATE membrosmp SET IdMembro = @IdMembro, IdPessoa = @IdPessoa, IdProcesso = @IdProcesso, DataInicial = @DataInicial, DataFinal = @DataFinal, Atual = @Atual
                                    WHERE IdMembro = @IdMembro";

                        _dbConnection.Execute(sql, entidade);
                    }

                    public void DeletemembrosmpENT(int id)
                    {
                        var sql = $@"DELETE FROM membrosmp WHERE IdMembro = @id";
                        _dbConnection.Execute(sql, new { id });
                    }
                }
            }