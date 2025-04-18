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
    public class AdvogadoDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public AdvogadoDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }

        public void AddadvogadoENT(AdvogadoENT entidade)
        {
            var sql = $@"INSERT INTO advogado (IdAdvogado, IdPessoa, IdEndereco, IdTelefone, IdRede, NrOab, OabUf, Atual)
                        VALUES (@IdAdvogado, @IdPessoa, @IdEndereco, @IdTelefone, @IdRede, @NrOab, @OabUf, @Atual)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<AdvogadoENT> GetadvogadoENTs()
        {
            const string sql = "SELECT * FROM advogado";
            return _dbConnection.Query<AdvogadoENT>(sql).ToList();
        }

        public async Task<IEnumerable<AdvogadoENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM advogado";
            return await _dbConnection.QueryAsync<AdvogadoENT>(sql);
        }

        public void UpdateadvogadoENT(AdvogadoENT entidade)
        {
            var sql = $@"UPDATE advogado SET IdAdvogado = @IdAdvogado, IdPessoa = @IdPessoa, IdEndereco = @IdEndereco, IdTelefone = @IdTelefone, IdRede = @IdRede, NrOab = @NrOab, OabUf = @OabUf, Atual = @Atual
                        WHERE IdAdvogado = @IdAdvogado";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteadvogadoENT(int id)
        {
            var sql = $@"DELETE FROM advogado WHERE IdAdvogado = @id";
            _dbConnection.Execute(sql, new { id });
        }

        public int Inserir(AdvogadoENT entidade)
        {
            var sql = $@"INSERT INTO advogado (IdAdvogado, IdPessoa, IdEndereco, IdTelefone, IdRede, NrOab, OabUf, Atual)
                VALUES (@IdAdvogado, @IdPessoa, @IdEndereco, @IdTelefone, @IdRede, @NrOab, @OabUf, @Atual);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }


    }
}