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
    public class DocumentodeIdentificacaoDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public DocumentodeIdentificacaoDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(DocumentodeIdentificacaoENT entidade)
        {
            var sql = $@"INSERT INTO documentodeidentificacao (IdDocumento, IdPessoa, Nome, DataExp, IdNum, Numero)
                VALUES (@IdDocumento, @IdPessoa, @Nome, @DataExp, @IdNum, @Numero);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void AdddocumentodeidentificacaoENT(DocumentodeIdentificacaoENT entidade)
        {
            var sql = $@"INSERT INTO documentodeidentificacao (IdDocumento, IdPessoa, Nome, DataExp, IdNum, Numero)
                        VALUES (@IdDocumento, @IdPessoa, @Nome, @DataExp, @IdNum, @Numero)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<DocumentodeIdentificacaoENT> GetdocumentodeidentificacaoENTs()
        {
            const string sql = "SELECT * FROM documentodeidentificacao";
            return _dbConnection.Query<DocumentodeIdentificacaoENT>(sql).ToList();
        }

        public async Task<IEnumerable<DocumentodeIdentificacaoENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM documentodeidentificacao";
            return await _dbConnection.QueryAsync<DocumentodeIdentificacaoENT>(sql);
        }

        public void UpdatedocumentodeidentificacaoENT(DocumentodeIdentificacaoENT entidade)
        {
            var sql = $@"UPDATE documentodeidentificacao SET IdDocumento = @IdDocumento, IdPessoa = @IdPessoa, Nome = @Nome, DataExp = @DataExp, IdNum = @IdNum, Numero = @Numero
                        WHERE IdDocumento = @IdDocumento";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeletedocumentodeidentificacaoENT(int id)
        {
            var sql = $@"DELETE FROM documentodeidentificacao WHERE IdDocumento = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}