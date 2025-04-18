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
                public class OrgaoJurisdicionalDAL
                {
                    private readonly IDbConnection _dbConnection;
                    private string connectionString;

                    public OrgaoJurisdicionalDAL()
                    {
                        connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
                        _dbConnection = new MySqlConnection(connectionString);
                    }

                    public void AddorgaojurisdicionalENT(OrgaoJurisdicionalENT entidade)
                    {
                        var sql = $@"INSERT INTO orgaojurisdicional (idorgao, orgaojurisdicionalnome, orgaojurisdicionalabrev)
                                    VALUES (@idorgao, @orgaojurisdicionalnome, @orgaojurisdicionalabrev)";

                        _dbConnection.Execute(sql, entidade);
                    }
                    public int Inserir(OrgaoJurisdicionalENT entidade)
                    {
                        var sql = $@"INSERT INTO orgaojurisdicional (idorgao, orgaojurisdicionalnome, orgaojurisdicionalabrev)
                            VALUES (@idorgao, @orgaojurisdicionalnome, @orgaojurisdicionalabrev);
                            SELECT LAST_INSERT_ID();";

                        return _dbConnection.ExecuteScalar<int>(sql, entidade);
                    }

        public List<OrgaoJurisdicionalENT> GetorgaojurisdicionalENTs()
                    {
                        const string sql = "SELECT * FROM orgaojurisdicional";
                        return _dbConnection.Query<OrgaoJurisdicionalENT>(sql).ToList();
                    }

                    public async Task<IEnumerable<OrgaoJurisdicionalENT>> ObterTodosAsync()
                    {
                        const string sql = "SELECT * FROM orgaojurisdicional";
                        return await _dbConnection.QueryAsync<OrgaoJurisdicionalENT>(sql);
                    }

                    public void UpdateorgaojurisdicionalENT(OrgaoJurisdicionalENT entidade)
                    {
                        var sql = $@"UPDATE orgaojurisdicional SET idorgao = @idorgao, orgaojurisdicionalnome = @orgaojurisdicionalnome, orgaojurisdicionalabrev = @orgaojurisdicionalabrev
                                    WHERE idorgao = @idorgao";

                        _dbConnection.Execute(sql, entidade);
                    }

                    public void DeleteorgaojurisdicionalENT(int id)
                    {
                        var sql = $@"DELETE FROM orgaojurisdicional WHERE idorgao = @id";
                        _dbConnection.Execute(sql, new { id });
                    }
                }
            }