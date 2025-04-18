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
    public class ParticipanteAtoDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public ParticipanteAtoDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(ParticipanteAtoENT entidade)
        {
            var sql = $@"INSERT INTO participanteato (IdParticipanteAto, IdAtoProcessual, IdPessoa, Posicao)
                VALUES (@IdParticipanteAto, @IdAtoProcessual, @IdPessoa, @Posicao);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void AddparticipanteatoENT(ParticipanteAtoENT entidade)
        {
            var sql = $@"INSERT INTO participanteato (IdParticipanteAto, IdAtoProcessual, IdPessoa, Posicao)
                        VALUES (@IdParticipanteAto, @IdAtoProcessual, @IdPessoa, @Posicao)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<ParticipanteAtoENT> GetparticipanteatoENTs()
        {
            const string sql = "SELECT * FROM participanteato";
            return _dbConnection.Query<ParticipanteAtoENT>(sql).ToList();
        }

        public async Task<IEnumerable<ParticipanteAtoENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM participanteato";
            return await _dbConnection.QueryAsync<ParticipanteAtoENT>(sql);
        }

        public void UpdateparticipanteatoENT(ParticipanteAtoENT entidade)
        {
            var sql = $@"UPDATE participanteato SET IdParticipanteAto = @IdParticipanteAto, IdAtoProcessual = @IdAtoProcessual, IdPessoa = @IdPessoa, Posicao = @Posicao
                        WHERE IdParticipanteAto = @IdParticipanteAto";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteparticipanteatoENT(int id)
        {
            var sql = $@"DELETE FROM participanteato WHERE IdParticipanteAto = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}