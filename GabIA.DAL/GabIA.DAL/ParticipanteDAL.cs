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
    public class ParticipanteDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public ParticipanteDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(ParticipanteENT entidade)
        {
            var sql = $@"INSERT INTO participante (IdParticipante, IdAto, IdPessoa, IdPosicao)
                VALUES (@IdParticipante, @IdAto, @IdPessoa, @IdPosicao);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }

        public void AddparticipanteENT(ParticipanteENT entidade)
        {
            var sql = $@"INSERT INTO participante (IdParticipante, IdAto, IdPessoa, IdPosicao)
                        VALUES (@IdParticipante, @IdAto, @IdPessoa, @IdPosicao)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<ParticipanteENT> GetparticipanteENTs()
        {
            const string sql = "SELECT * FROM participante";
            return _dbConnection.Query<ParticipanteENT>(sql).ToList();
        }

        public async Task<IEnumerable<ParticipanteENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM participante";
            return await _dbConnection.QueryAsync<ParticipanteENT>(sql);
        }

        public void UpdateparticipanteENT(ParticipanteENT entidade)
        {
            var sql = $@"UPDATE participante SET IdParticipante = @IdParticipante, IdAto = @IdAto, IdPessoa = @IdPessoa, IdPosicao = @IdPosicao
                        WHERE IdParticipante = @IdParticipante";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteparticipanteENT(int id)
        {
            var sql = $@"DELETE FROM participante WHERE IdParticipante = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}