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
    public class ElementosDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;
        private readonly IDbConnection _db;
        private readonly CausaDePedirDAL _causaDePedirDAL;
        private readonly PedidoDAL _pedidoDAL;
        private readonly PartesDoProcessoDAL _partesDoProcessoDAL;


        public ElementosDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }
        public ElementosDAL(IDbConnection db, CausaDePedirDAL causaDePedirDAL, PedidoDAL pedidoDAL, PartesDoProcessoDAL partesDoProcessoDAL)
        {
            _db = db;
            _causaDePedirDAL = causaDePedirDAL;
            _pedidoDAL = pedidoDAL;
            _partesDoProcessoDAL = partesDoProcessoDAL;
        }
        public int Inserir(ElementosENT entidade)
        {
            var sql = $@"INSERT INTO elementos (IdElemento, Partes, CausaDePedir, Pedido, idprocesso)
                VALUES (@IdElemento, @Partes, @CausaDePedir, @Pedido, @idprocesso);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }
        public void AddelementosENT(ElementosENT entidade)
        {
            var sql = $@"INSERT INTO elementos (IdElemento, Partes, CausaDePedir, Pedido, idprocesso)
                        VALUES (@IdElemento, @Partes, @CausaDePedir, @Pedido, @idprocesso)";

            _dbConnection.Execute(sql, entidade);
        }
        public int InserirOuAtualizarElementoFromNeoGab(ProcessoCompletoENT processoCompleto, PartesDoProcessoDAL _partes, CausaDePedirDAL _causas, PedidoDAL _pedidos)
        {
            int prcJd = processoCompleto.IdPJ;
            if (_dbConnection.State == ConnectionState.Closed)
            {
                _dbConnection.Open();
            }

            // Verificar se o registro já existe baseado em IdProcesso (IdPJ)
            var sqlExistente = "SELECT IdElemento FROM Elementos WHERE IdProcesso = @IdProcesso";
            var idExistente = _dbConnection.Query<int?>(sqlExistente, new { IdProcesso = prcJd}).FirstOrDefault();
            int retorno =  0;

            if (idExistente.HasValue)
            {
                //int idPj = processoCompleto.IdPJ;
                // Atualizar o registro existente apenas com o campo idProcesso
                //var sqlAtualizar = "UPDATE Elementos SET IdProcesso = @IdProcesso WHERE IdElemento = @IdElemento";
                //_db.Execute(sqlAtualizar, new { IdProcesso = idPj, IdElemento = idExistente.Value });
                //idElemento = idExistente.Value;
                // Inserir ou atualizar em PartesDoProcesso
                //_partesDoProcessoDAL = new PartesDoProcessoDAL();
                _partes.InserirOuAtualizarParteFromNeoGab(idExistente.Value, processoCompleto);

                // Inserir ou atualizar em CausaDePedir
                _causas.InserirOuAtualizarCausaFromNeoGab(idExistente.Value, processoCompleto);

                // Inserir ou atualizar em Pedido
                _pedidos.InserirOuAtualizarPedidoFromNeoGab(idExistente.Value, processoCompleto);
                return idExistente.Value;
            }
            else
            {
                // Inserir um novo registro apenas com o campo idProcesso
                var sqlInserir = "INSERT INTO Elementos (IdProcesso) VALUES (@IdProcesso); SELECT LAST_INSERT_ID();";

                retorno = _dbConnection.Query<int>(sqlInserir, new { IdProcesso = prcJd}).Single();

                _partes.InserirOuAtualizarParteFromNeoGab(retorno, processoCompleto);

                // Inserir ou atualizar em CausaDePedir
                _causas.InserirOuAtualizarCausaFromNeoGab(retorno, processoCompleto);

                // Inserir ou atualizar em Pedido
                _pedidos.InserirOuAtualizarPedidoFromNeoGab(retorno, processoCompleto);


            }
            _dbConnection.Close();
            return retorno;
        }

        public List<ElementosENT> GetelementosENTs()
        {
            const string sql = "SELECT * FROM elementos";
            return _dbConnection.Query<ElementosENT>(sql).ToList();
        }

        public IEnumerable<ElementosENT> GetElementosByProcessoId(int idProcesso)
        {
            const string sql = "SELECT * FROM elementos WHERE idProcesso = @IdProcesso";

            // Usando Dapper para executar a consulta e mapear os resultados
            var elementos = _dbConnection.Query<ElementosENT>(sql, new { IdProcesso = idProcesso });

            return elementos;
        }

        public ElementosENT GetElementosByIdProcesso(int idProcesso)
        {
            return _dbConnection.QueryFirstOrDefault<ElementosENT>("SELECT * FROM Elementos WHERE IdProcesso = @Id", new { Id = idProcesso });
        }


        public async Task<IEnumerable<ElementosENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM elementos";
            return await _dbConnection.QueryAsync<ElementosENT>(sql);
        }

        public void UpdateelementosENT(ElementosENT entidade)
        {

            Debug.WriteLine(entidade.IdElemento);
            var sql = $@"UPDATE elementos SET IdElemento = @IdElemento, Partes = @Partes, CausaDePedir = @CausaDePedir, Pedido = @Pedido, idprocesso = @idprocesso
                        WHERE IdElemento = @IdElemento";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteelementosENT(int id)
        {
            var sql = $@"DELETE FROM elementos WHERE IdElemento = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}