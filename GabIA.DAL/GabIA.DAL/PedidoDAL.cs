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
    public class PedidoDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public PedidoDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }

        public int InserirOuAtualizarPedidoFromNeoGab(int idElemento, ProcessoCompletoENT processoCompleto)
        {

            if (_dbConnection.State == ConnectionState.Closed)
            {
                _dbConnection.Open();
            }

            // Verificar se existe um registro para o idElemento
            var sqlExistente = "SELECT IdPedido FROM Pedido WHERE IdElemento = @IdElemento";
            var idExistente = _dbConnection.Query<int?>(sqlExistente, new { IdElemento = idElemento }).FirstOrDefault();

            if (idExistente.HasValue)
            {
                // Atualizar o registro existente com o idElemento
                //var sqlAtualizar = "UPDATE Pedido SET IdElemento = @IdElemento WHERE IdPedido = @IdPedido";
                //_dbConnection.Execute(sqlAtualizar, new { IdElemento = idElemento, IdPedido = idExistente.Value });
                return idExistente.Value;
            }
            else
            {
                // Inserir um novo registro com o idElemento
                var sqlInserir = "INSERT INTO Pedido (IdElemento) VALUES (@IdElemento); SELECT LAST_INSERT_ID();";
                var idNovo = _dbConnection.Query<int>(sqlInserir, new { IdElemento = idElemento }).Single();
                return idNovo;
            }
        }
        public int Inserir(PedidoENT entidade)
        {
            var sql = $@"INSERT INTO pedido (IdPedido, Pedido)
                VALUES (@IdPedido, @Pedido);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }

        public void AddPedidoENT(PedidoENT entidade)
        {
            var sql = $@"INSERT INTO pedido (IdPedido, Pedido)
                        VALUES (@IdPedido, @Pedido)";

            _dbConnection.Execute(sql, entidade);
        }

        public List<PedidoENT> GetPedidoENTs()
        {
            const string sql = "SELECT * FROM pedido";
            return _dbConnection.Query<PedidoENT>(sql).ToList();
        }

        public List<PedidoENT> ObterPedidosPorIdElemento(int idElemento)
        {
            const string sql = "SELECT * FROM Pedido WHERE IdElemento = @IdElemento";
            return _dbConnection.Query<PedidoENT>(sql, new { IdElemento = idElemento }).ToList();
        }
        public async Task<IEnumerable<PedidoENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM pedido";
            return await _dbConnection.QueryAsync<PedidoENT>(sql);
        }

        public void UpdatePedidoENT(PedidoENT entidade)
        {
            var sql = $@"UPDATE pedido SET IdPedido = @IdPedido, Pedido = @Pedido
                        WHERE IdPedido = @IdPedido";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeletePedidoENT(int id)
        {
            var sql = $@"DELETE FROM pedido WHERE IdPedido = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}