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
using System.Diagnostics;

namespace GabIA.DAL
{
    public class PosicoesDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public PosicoesDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(PosicoesENT entidade)
        {
        var sql = $@"INSERT INTO posicoes (id_posicao, posicao)
            VALUES (@id_posicao, @posicao);
            SELECT LAST_INSERT_ID();";

        return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }

        public int GetIdPosicaoPart(string _posicao)
        {
            const string sql = "SELECT id_posicao FROM posicoes WHERE posicao LIKE CONCAT('%', @pos, '%')";
            return _dbConnection.ExecuteScalar<int>(sql, new { pos = _posicao });
        }

        public int GetIdPosicao(string _posicao)
        {
            const string sql = "SELECT * FROM posicoes where posicao = @pos";
            return _dbConnection.ExecuteScalar<int>(sql, new { pos = _posicao});
        }

        public void AddposicoesENT(PosicoesENT entidade)
        {
            var sql = $@"INSERT INTO posicoes (id_posicao, posicao)
                        VALUES (@id_posicao, @posicao)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<PosicoesENT> GetposicoesENTs()
        {
            const string sql = "SELECT * FROM posicoes";
            return _dbConnection.Query<PosicoesENT>(sql).ToList();
        }

        public async Task<IEnumerable<PosicoesENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM posicoes";
            return await _dbConnection.QueryAsync<PosicoesENT>(sql);
        }

        public void UpdateposicoesENT(PosicoesENT entidade)
        {
            var sql = $@"UPDATE posicoes SET id_posicao = @id_posicao, posicao = @posicao
                        WHERE id_posicao = @id_posicao";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteposicoesENT(int id)
        {
            var sql = $@"DELETE FROM posicoes WHERE id_posicao = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}