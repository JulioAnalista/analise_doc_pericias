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
    public class AcessoaoBancoRegistroDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public AcessoaoBancoRegistroDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }

        public void AddAcessoaoBancoRegistroENT(AcessoaoBancoRegistroENT entidade)
        {
            var sql = $@"INSERT INTO acessoaobancoregistro (Idacesso, Usuario, Momento)
                        VALUES (@Idacesso, @Usuario, @Momento)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<AcessoaoBancoRegistroENT> GetAcessoaoBancoRegistroENTs()
        {
            const string sql = "SELECT * FROM acessoaobancoregistro";
            return _dbConnection.Query<AcessoaoBancoRegistroENT>(sql).ToList();
        }

        public async Task<IEnumerable<AcessoaoBancoRegistroENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM acessoaobancoregistro";
            return await _dbConnection.QueryAsync<AcessoaoBancoRegistroENT>(sql);
        }

        public int Inserir(AcessoaoBancoRegistroENT entidade)
        {
            var sql = $@"INSERT INTO acessoaobancoregistro (Idacesso, Usuario, Momento)
                VALUES (@Idacesso, @Usuario, @Momento);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }

        public void UpdateAcessoaoBancoRegistroENT(AcessoaoBancoRegistroENT entidade)
        {
            var sql = $@"UPDATE acessoaobancoregistro SET Idacesso = @Idacesso, Usuario = @Usuario, Momento = @Momento
                        WHERE Idacesso = @Idacesso";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteAcessoaoBancoRegistroENT(int id)
        {
            var sql = $@"DELETE FROM acessoaobancoregistro WHERE Idacesso = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}