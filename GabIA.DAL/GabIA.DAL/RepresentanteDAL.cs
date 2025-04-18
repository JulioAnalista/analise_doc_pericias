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
                public class RepresentanteDAL
                {
                    private readonly IDbConnection _dbConnection;
                    private string connectionString;

                    public RepresentanteDAL()
                    {
                        connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
                        _dbConnection = new MySqlConnection(connectionString);
                    }
                    public int Inserir(RepresentanteENT entidade)
                    {
                        var sql = $@"INSERT INTO representante (id_representante, id_advogado, processo, id_representado, mandato)
                            VALUES (@id_representante, @id_advogado, @processo, @id_representado, @mandato);
                            SELECT LAST_INSERT_ID();";

                        return _dbConnection.ExecuteScalar<int>(sql, entidade);
                    }

        public void AddrepresentanteENT(RepresentanteENT entidade)
                    {
                        var sql = $@"INSERT INTO representante (id_representante, id_advogado, processo, id_representado, mandato)
                                    VALUES (@id_representante, @id_advogado, @processo, @id_representado, @mandato)";

                        _dbConnection.Execute(sql, entidade);
                    }

                    public List<RepresentanteENT> GetrepresentanteENTs()
                    {
                        const string sql = "SELECT * FROM representante";
                        return _dbConnection.Query<RepresentanteENT>(sql).ToList();
                    }

                    public async Task<IEnumerable<RepresentanteENT>> ObterTodosAsync()
                    {
                        const string sql = "SELECT * FROM representante";
                        return await _dbConnection.QueryAsync<RepresentanteENT>(sql);
                    }

                    public void UpdaterepresentanteENT(RepresentanteENT entidade)
                    {
                        var sql = $@"UPDATE representante SET id_representante = @id_representante, id_advogado = @id_advogado, processo = @processo, id_representado = @id_representado, mandato = @mandato
                                    WHERE id_representante = @id_representante";

                        _dbConnection.Execute(sql, entidade);
                    }

                    public void DeleterepresentanteENT(int id)
                    {
                        var sql = $@"DELETE FROM representante WHERE id_representante = @id";
                        _dbConnection.Execute(sql, new { id });
                    }
                }
            }