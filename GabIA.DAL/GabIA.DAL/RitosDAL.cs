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
                public class RitosDAL
                {
                    private readonly IDbConnection _dbConnection;
                    private string connectionString;

                    public RitosDAL()
                    {
                        connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
                        _dbConnection = new MySqlConnection(connectionString);
                    }
                    public int Inserir(RitosENT entidade)
                    {
                        var sql = $@"INSERT INTO ritos (id_ritos, id_rito, id_DATETIME)
                            VALUES (@id_ritos, @id_rito, @id_DATETIME);
                            SELECT LAST_INSERT_ID();";

                        return _dbConnection.ExecuteScalar<int>(sql, entidade);
                    }
        public void AddritosENT(RitosENT entidade)
                    {
                        var sql = $@"INSERT INTO ritos (id_ritos, id_rito, id_DATETIME)
                                    VALUES (@id_ritos, @id_rito, @id_DATETIME)";

                        _dbConnection.Execute(sql, entidade);
                    }

                    public List<RitosENT> GetritosENTs()
                    {
                        const string sql = "SELECT * FROM ritos";
                        return _dbConnection.Query<RitosENT>(sql).ToList();
                    }

                    public async Task<IEnumerable<RitosENT>> ObterTodosAsync()
                    {
                        const string sql = "SELECT * FROM ritos";
                        return await _dbConnection.QueryAsync<RitosENT>(sql);
                    }

                    public void UpdateritosENT(RitosENT entidade)
                    {
                        var sql = $@"UPDATE ritos SET id_ritos = @id_ritos, id_rito = @id_rito, id_DATETIME = @id_DATETIME
                                    WHERE id_ritos = @id_ritos";

                        _dbConnection.Execute(sql, entidade);
                    }

                    public void DeleteritosENT(int id)
                    {
                        var sql = $@"DELETE FROM ritos WHERE id_ritos = @id";
                        _dbConnection.Execute(sql, new { id });
                    }
                }
            }