using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using GabIA.ENT;

namespace GabIA.DAL
{
    public class ProcessoPartesRepository
    {
        private readonly string _connectionString;

        public ProcessoPartesRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<PessoaENT> GetPartesPorProcessoId(int processoId)
        {
            List<PessoaENT> partes = new List<PessoaENT>();

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"SELECT P.*
                                 FROM pessoa P
                                 JOIN partesdoprocesso PP ON P.Id = PP.PessoaId
                                 JOIN elementos E ON PP.ElementoId = E.Id
                                 JOIN processo_judicial PJ ON E.ProcessoJudicialId = PJ.Id
                                 WHERE PJ.ProcessoId = @ProcessoId";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProcessoId", processoId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PessoaENT pessoa = new PessoaENT
                            {
                                IdPessoa = reader.GetInt32("Id"),
                                Nome = reader.GetString("Nome"),
                                // Adicione outras colunas/propriedades aqui conforme necessário
                            };

                            partes.Add(pessoa);
                        }
                    }
                }
            }

            return partes;
        }
    }
}
