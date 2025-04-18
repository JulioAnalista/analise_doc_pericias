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
    public class OrgaoMinisterialDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public OrgaoMinisterialDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(OrgaoMinisterialENT entidade)
        {
            var sql = $@"INSERT INTO orgaoministerial (idorgaoministerial, orgaoministerial, orgaoministerialsigla, orgaoministerialabrev)
                            VALUES (@idorgaoministerial, @orgaoministerial, @orgaoministerialsigla, @orgaoministerialabrev);
                            SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }

        public void AddrepresentanteENT(OrgaoMinisterialENT entidade)
        {
            var sql = $@"INSERT INTO orgaoministerial (idorgaoministerial, orgaoministerial, orgaoministerialsigla, orgaoministerialabrev)
                                    VALUES (@idorgaoministerial, @orgaoministerial, @orgaoministerialsigla, @orgaoministerialabrev)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<OrgaoMinisterialENT> GetOrgaoMinisterialENTs()
        {
            const string sql = "SELECT * FROM orgaoministerial";
            return _dbConnection.Query<OrgaoMinisterialENT>(sql).ToList();
        }

        public OrgaoMinisterialENT GetOrgaoMinisterialByIdParte(int idParte)
        {
            return _dbConnection.QueryFirstOrDefault<OrgaoMinisterialENT>("SELECT * FROM OrgaoMinisterial WHERE idorgaoministerial = @Id", new { Id = idParte });
        }


        public int GetId_OrgaoMinisterial(string _orgao)
        {
            const string sql = "SELECT idorgaoministerial FROM orgaoministerial where orgaoministerial = @descr";
            return _dbConnection.ExecuteScalar<int>(sql, new { descr = _orgao});
        }

        public async Task<IEnumerable<OrgaoMinisterialENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM orgaoministerial";
            return await _dbConnection.QueryAsync<OrgaoMinisterialENT>(sql);
        }

        public void UpdateOrgaoMinisterialENT(OrgaoMinisterialENT entidade)
        {
            var sql = $@"UPDATE orgaoministerial SET idorgaoministerial = @idorgaoministerial, orgaoministerial = @orgaoministerial, orgaoministerialsigla = @orgaoministerialsigla, orgaoministerialabrev = @orgaoministerialabrev
                                    WHERE idorgaoministerial = @idorgaoministerial";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteOrgaoMinisterialENT(int id)
        {
            var sql = $@"DELETE FROM orgaoministerial WHERE idorgaoministerial = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}