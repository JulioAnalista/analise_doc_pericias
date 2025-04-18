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
    public class AtoProcessualDAL
    {
        private readonly IDbConnection _dbConnection;
        private string connectionString;

        public AtoProcessualDAL()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DB_ConnectionString"].ConnectionString;
            _dbConnection = new MySqlConnection(connectionString);
        }

        public List<AtoProcessualENT> ObterAtosProcessuais(int idProcessoJudicial)
        {
            var sql = @"
                SELECT * FROM atoProcessual
                WHERE idProcesso = @IdProcessoJudicial";
            var atosProcessuais = _dbConnection.Query<AtoProcessualENT>(sql, new { IdProcessoJudicial = idProcessoJudicial.ToString() }).ToList();
            //Debug.WriteLine(atosProcessuais.Count());
            return atosProcessuais;
        }

        
        public List<AtoProcessualENT> ObterMovimentosPorIdProcesso(int idProcesso)
        {
            var sql = @"
                SELECT * FROM atoProcessual
                WHERE idProcesso = @NumeroProcesso";
            var atosProcessuais = _dbConnection.Query<AtoProcessualENT>(sql, new { NumeroProcesso = idProcesso }).ToList();

            return atosProcessuais;
        }


        public int Inserir(AtoProcessualENT entidade)
        {
            var sql = $@"INSERT INTO atoprocessual (IdAtoProcessual, IdProcesso, Data, Tipo, Continuacao, Documento, Publicado, Texto, AssinadoPor, DataInclusao, resumo, idMovimento)
                VALUES (@IdAtoProcessual, @IdProcesso, @Data, @Tipo, @Continuacao, @Documento, @Publicado, @Texto, @AssinadoPor, @DataInclusao, @resumo, @idMovimento);
                SELECT LAST_INSERT_ID();";

            return _dbConnection.ExecuteScalar<int>(sql, entidade);
        }

        public List<AtoProcessualENT> GetAtoProcessualByIdProcesso(int idProcesso)
        {
            return _dbConnection.Query<AtoProcessualENT>("SELECT * FROM AtoProcessual WHERE IdProcesso = @Id", new { Id = idProcesso }).ToList();
        }


        public void AddatoprocessualENT(AtoProcessualENT entidade)
        {
            var sql = $@"INSERT INTO atoprocessual (IdAtoProcessual, IdProcesso, Data, Tipo, Continuacao, Documento, Publicado, Texto, AssinadoPor, DataInclusao, resumo, idMovimento)
                        VALUES (@IdAtoProcessual, @IdProcesso, @Data, @Tipo, @Continuacao, @Documento, @Publicado, @Texto, @AssinadoPor, @DataInclusao, @resumo, @idMovimento)";

            _dbConnection.Execute(sql, entidade);
        }
 
        public List<AtoProcessualConsulta> ObterAtoProcessualPorNumeroProcesso(string numeroProcesso)
        {


            string sql = @"
                    SELECT atoProcessual.DataInclusao, atoProcessual.resumo, atoProcessual.idMovimento, tipo_ato_processual.tipo
                    FROM atoProcessual
                    INNER JOIN tipo_ato_processual ON atoProcessual.tipo = tipo_ato_processual.Id_Tipo_Ato_Processual
                    INNER JOIN processo_judicial ON atoProcessual.idProcesso = processo_judicial.idProcesso
                    INNER JOIN processo ON processo_judicial.idProcesso = processo.id_processo
                    WHERE processo.numero_processo = @NumeroProcesso";

            var parametros = new { NumeroProcesso = numeroProcesso };

            return _dbConnection.Query<AtoProcessualConsulta>(sql, parametros).ToList();
        }


        public List<AtoProcessualENT> ObterMovimentosComDescricaoPorIdProcesso1(string numeroProcesso)
        {
            string sql = @"
                SELECT ap.*, tap.tipo AS DescricaoAto
                FROM atoprocessual ap
                INNER JOIN tipo_ato_processual tap ON ap.tipo = tap.id_tipo_ato_processual
                INNER JOIN processo_judicial pj ON ap.idProcesso = pj.idProcesso
                INNER JOIN processo p ON pj.idProcesso = p.id_processo
                WHERE p.numero_processo = @NumeroProcesso";

            var parametros = new { NumeroProcesso = numeroProcesso };

            return _dbConnection.Query<AtoProcessualENT>(sql, parametros).ToList();
        }


        public List<AtoProcessualENT> ObterAtosProcessuaisPorNumeroProcesso(int numeroProcesso)
        {
            var sql = @"
                SELECT * FROM atoProcessual
                WHERE idProcesso = @IdProcessoJudicial";
            var atosProcessuais = _dbConnection.Query<AtoProcessualENT>(sql, new { IdProcessoJudicial = numeroProcesso.ToString() }).ToList();
            //Debug.WriteLine(atosProcessuais.Count());
            return atosProcessuais;
        }





        public List<AtoProcessualConsulta> ObterListaAtoProcessualPorProcessoJudicial(int IdProcesso)
        {


            string sql = @"
                    SELECT atoProcessual.DataInclusao, atoProcessual.resumo, atoProcessual.idMovimento, atoProcessual.tipo
                    FROM atoProcessual WHERE idProcesso = @IdProc";

            var parametros = new { IdProc = IdProcesso.ToString()};

            return _dbConnection.Query<AtoProcessualConsulta>(sql, parametros).ToList();
        }



        public AtoProcessualENT ObterAtoProcessualPorNumeroProcessoII(string resumo)
        {
            const string sql = "SELECT * FROM atoprocessual where resumo = @resu";
            return _dbConnection.ExecuteScalar<AtoProcessualENT>(sql, new { resumo = resumo });
        }

        public List<AtoProcessualENT> GetatoprocessualENTs()
        {
            const string sql = "SELECT * FROM atoprocessual";
            return _dbConnection.Query<AtoProcessualENT>(sql).ToList();
        }

        public async Task<IEnumerable<AtoProcessualENT>> ObterTodosAsync()
        {
            const string sql = "SELECT * FROM atoprocessual";
            return await _dbConnection.QueryAsync<AtoProcessualENT>(sql);
        }

        public void UpdateatoprocessualENT(AtoProcessualENT entidade)
        {
            var sql = $@"UPDATE atoprocessual SET IdAtoProcessual = @IdAtoProcessual, IdProcesso = @IdProcesso, Data = @Data, Tipo = @Tipo, Continuacao = @Continuacao, Documento = @Documento, Publicado = @Publicado, Texto = @Texto, AssinadoPor = @AssinadoPor, DataInclusao = @DataInclusao, resumo = @resumo, idMovimento = @idMovimento
                        WHERE IdAtoProcessual = @IdAtoProcessual";

            _dbConnection.Execute(sql, entidade);
        }

        public void DeleteatoprocessualENT(int id)
        {
            var sql = $@"DELETE FROM atoprocessual WHERE IdAtoProcessual = @id";
            _dbConnection.Execute(sql, new { id });
        }
    }
}