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
                public class RevelDAL
                {
                    private readonly IDbConnection _dbConnection;
                    private string connectionString;

                    public RevelDAL()
                    {
                        connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
                        _dbConnection = new MySqlConnection(connectionString);
                    }

                    public int Inserir(RevelENT entidade)
                    {
                        var sql = $@"INSERT INTO revel (id_revel, Id_processo, Id_pessoa, Data_revelia, Nomeado_curador, Id_curador)
                            VALUES (@id_revel, @Id_processo, @Id_pessoa, @Data_revelia, @Nomeado_curador, @Id_curador);
                            SELECT LAST_INSERT_ID();";

                        return _dbConnection.ExecuteScalar<int>(sql, entidade);
                    }
                    public void AddrevelENT(RevelENT entidade)
                    {
                        var sql = $@"INSERT INTO revel (id_revel, Id_processo, Id_pessoa, Data_revelia, Nomeado_curador, Id_curador)
                                    VALUES (@id_revel, @Id_processo, @Id_pessoa, @Data_revelia, @Nomeado_curador, @Id_curador)";

                        _dbConnection.Execute(sql, entidade);
                    }

                    public List<RevelENT> GetrevelENTs()
                    {
                        const string sql = "SELECT * FROM revel";
                        return _dbConnection.Query<RevelENT>(sql).ToList();
                    }

                    public async Task<IEnumerable<RevelENT>> ObterTodosAsync()
                    {
                        const string sql = "SELECT * FROM revel";
                        return await _dbConnection.QueryAsync<RevelENT>(sql);
                    }

                    public void UpdaterevelENT(RevelENT entidade)
                    {
                        var sql = $@"UPDATE revel SET id_revel = @id_revel, Id_processo = @Id_processo, Id_pessoa = @Id_pessoa, Data_revelia = @Data_revelia, Nomeado_curador = @Nomeado_curador, Id_curador = @Id_curador
                                    WHERE id_revel = @id_revel";

                        _dbConnection.Execute(sql, entidade);
                    }

                    public void DeleterevelENT(int id)
                    {
                        var sql = $@"DELETE FROM revel WHERE id_revel = @id";
                        _dbConnection.Execute(sql, new { id });
                    }
                }
            }