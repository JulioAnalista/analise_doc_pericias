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
    public class PublicacoesDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public PublicacoesDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(PublicacoesENT entidade)
        {
            var sql = $@"INSERT INTO publicacoes (idpublicacoes, id_processo, data, DJe, disponibilizacao, pagina, publicacao)
                VALUES (@idpublicacoes, @id_processo, @data, @DJe, @disponibilizacao, @pagina, @publicacao);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }

        public void AddpublicacoesENT(PublicacoesENT entidade)
        {
            var sql = $@"INSERT INTO publicacoes (idpublicacoes, id_processo, data, DJe, disponibilizacao, pagina, publicacao)
                        VALUES (@idpublicacoes, @id_processo, @data, @DJe, @disponibilizacao, @pagina, @publicacao)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<PublicacoesENT> GetpublicacoesENTs()
        {
            const string sql = "SELECT * FROM publicacoes";
            return _dbConnection.Query<PublicacoesENT>(sql).ToList();
        }

        public async Task<IEnumerable<PublicacoesENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM publicacoes";
            return await _dbConnection.QueryAsync<PublicacoesENT>(sql);
        }

        public void UpdatepublicacoesENT(PublicacoesENT entidade)
        {
            var sql = $@"UPDATE publicacoes SET idpublicacoes = @idpublicacoes, id_processo = @id_processo, data = @data, DJe = @DJe, disponibilizacao = @disponibilizacao, pagina = @pagina, publicacao = @publicacao
                        WHERE idpublicacoes = @idpublicacoes";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeletepublicacoesENT(int id)
        {
            var sql = $@"DELETE FROM publicacoes WHERE idpublicacoes = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}