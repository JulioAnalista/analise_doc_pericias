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
    public class CausaDePedirDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public CausaDePedirDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }

        public int InserirOuAtualizarCausaFromNeoGab(int idElemento, ProcessoCompletoENT processoCompleto)
        {
            if (_dbConnection.State == ConnectionState.Closed)
            {
                _dbConnection.Open();
            }

            // Verificar se existe um registro para o idElemento
            var sqlExistente = "SELECT IdCausa FROM CausaDePedir WHERE IdElemento = @IdElemento";
            var idExistente = _dbConnection.Query<int?>(sqlExistente, new { IdElemento = idElemento }).FirstOrDefault();

            if (idExistente.HasValue)
            {
                // Atualizar o registro existente com o idElemento
                //var sqlAtualizar = "UPDATE CausaDePedir SET IdElemento = @IdElemento WHERE IdCausa = @IdCausa";
                //_dbConnection.Execute(sqlAtualizar, new { IdElemento = idElemento, IdCausa = idExistente.Value });
                return idExistente.Value;
            }
            else
            {
                // Inserir um novo registro com o idElemento
                var sqlInserir = "INSERT INTO CausaDePedir (IdElemento) VALUES (@IdElemento); SELECT LAST_INSERT_ID();";
                var idNovo = _dbConnection.Query<int>(sqlInserir, new { IdElemento = idElemento }).Single();
                return idNovo;
            }
        }

        public void AddCausaDePedirENT(CausaDePedirENT entidade)
        {
            var sql = $@"INSERT INTO CausaDePedir (IdCausa, Causa)
                        VALUES (@IdCausa, @Causa)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<CausaDePedirENT> GetCausaDePedirENTs()
        {
            const string sql = "SELECT * FROM CausaDePedir";
            return _dbConnection.Query<CausaDePedirENT>(sql).ToList();
        }

        public List<CausaDePedirENT> ObterCausasDePedirPorIdElemento(int idElemento)
        {
            const string sql = "SELECT * FROM CausaDePedir WHERE IdElemento = @IdElemento";
            return _dbConnection.Query<CausaDePedirENT>(sql, new { IdElemento = idElemento }).ToList();
        }

        public async Task<IEnumerable<CausaDePedirENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM CausaDePedir";
            return await _dbConnection.QueryAsync<CausaDePedirENT>(sql);
        }
        public int Inserir(CausaDePedirENT entidade)
        {
            var sql = $@"INSERT INTO CausaDePedir (IdCausa, Causa)
                VALUES (@IdCausa, @Causa);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void UpdateCausaDePedirENT(CausaDePedirENT entidade)
        {
            var sql = $@"UPDATE CausaDePedir SET IdCausa = @IdCausa, Causa = @Causa
                        WHERE IdCausa = @IdCausa";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteCausaDePedirENT(int id)
        {
            var sql = $@"DELETE FROM CausaDePedir WHERE IdCausa = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}