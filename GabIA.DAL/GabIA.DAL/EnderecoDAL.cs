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
    public class EnderecoDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public EnderecoDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(EnderecoENT entidade)
        {
            var sql = $@"INSERT INTO endereco (IdEndereco, IdPessoa, IdLogradouro, Numero, Complemento, Bairro, IdCidade, IdEstado, Cep, Residencial, Compartilhado, Referencia, Atual, CoordLat, CoordLong)
                VALUES (@IdEndereco, @IdPessoa, @IdLogradouro, @Numero, @Complemento, @Bairro, @IdCidade, @IdEstado, @Cep, @Residencial, @Compartilhado, @Referencia, @Atual, @CoordLat, @CoordLong);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void AddenderecoENT(EnderecoENT entidade)
        {
            var sql = $@"INSERT INTO endereco (IdEndereco, IdPessoa, IdLogradouro, Numero, Complemento, Bairro, IdCidade, IdEstado, Cep, Residencial, Compartilhado, Referencia, Atual, CoordLat, CoordLong)
                        VALUES (@IdEndereco, @IdPessoa, @IdLogradouro, @Numero, @Complemento, @Bairro, @IdCidade, @IdEstado, @Cep, @Residencial, @Compartilhado, @Referencia, @Atual, @CoordLat, @CoordLong)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<EnderecoENT> GetenderecoENTs()
        {
            const string sql = "SELECT * FROM endereco";
            return _dbConnection.Query<EnderecoENT>(sql).ToList();
        }

        public async Task<IEnumerable<EnderecoENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM endereco";
            return await _dbConnection.QueryAsync<EnderecoENT>(sql);
        }

        public void UpdateenderecoENT(EnderecoENT entidade)
        {
            var sql = $@"UPDATE endereco SET IdEndereco = @IdEndereco, IdPessoa = @IdPessoa, IdLogradouro = @IdLogradouro, Numero = @Numero, Complemento = @Complemento, Bairro = @Bairro, IdCidade = @IdCidade, IdEstado = @IdEstado, Cep = @Cep, Residencial = @Residencial, Compartilhado = @Compartilhado, Referencia = @Referencia, Atual = @Atual, CoordLat = @CoordLat, CoordLong = @CoordLong
                        WHERE IdEndereco = @IdEndereco";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteenderecoENT(int id)
        {
            var sql = $@"DELETE FROM endereco WHERE IdEndereco = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}