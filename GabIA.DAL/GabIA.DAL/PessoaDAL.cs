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
    public class PessoaDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public PessoaDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public int Inserir(PessoaENT entidade)
        {
            var sql = $@"INSERT INTO pessoa (id_pessoa, nome, iniciais, genero, nascimento, tipo, representacao, Procurador)
                VALUES (@id_pessoa, @nome, @iniciais, @genero, @nascimento, @tipo, @representacao, @Procurador);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public int GetPessoaId_FromNome(string _nome)
        {
            const string sql = "SELECT id_pessoa FROM pessoa where nome = @nome";
            return _dbConnection.ExecuteScalar<int>(sql, new { nome = _nome});
        }

        public void AddpessoaENT(PessoaENT entidade)
        {
            var sql = $@"INSERT INTO pessoa (id_pessoa, nome, iniciais, genero, nascimento, tipo, representacao, Procurador)
                        VALUES (@id_pessoa, @nome, @iniciais, @genero, @nascimento, @tipo, @representacao, @Procurador)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<PessoaENT> GetpessoaENTs()
        {
            const string sql = "SELECT * FROM pessoa";
            return _dbConnection.Query<PessoaENT>(sql).ToList();
        }

        public async Task<IEnumerable<PessoaENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM pessoa";
            return await _dbConnection.QueryAsync<PessoaENT>(sql);
        }

        public void UpdatepessoaENT(PessoaENT entidade)
        {
            var sql = $@"UPDATE pessoa SET id_pessoa = @id_pessoa, nome = @nome, iniciais = @iniciais, genero = @genero, nascimento = @nascimento, tipo = @tipo, representacao = @representacao, Procurador = @Procurador
                        WHERE id_pessoa = @id_pessoa";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeletepessoaENT(int id)
        {
            var sql = $@"DELETE FROM pessoa WHERE id_pessoa = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}