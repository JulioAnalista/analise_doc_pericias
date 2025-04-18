using Dapper;
using GabIA.DAL;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static GabIA.WPF.PreprocessamentoTextoIA;

namespace GabIA.WPF
{
    public class DataManager
    {
        private static string connectionString;
        private string _baseDirectory;

        public DataManager()
        {
            connectionString = "Server=localhost;Database=dbprocesso;Uid=root;Pwd=0%Cesar%0;";
        }

        public static void RemoveLinhasInvalidas(string filePath)
        {
            if (File.Exists(filePath))
            {
                string outputFilePath = Path.ChangeExtension(filePath, null) + "Mov" + Path.GetExtension(filePath);

                bool shouldWrite = false;
                using (StreamReader reader = new StreamReader(filePath))
                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    bool vouComparar = false;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {

                        if (line.Contains("Documentos", StringComparison.OrdinalIgnoreCase) && !vouComparar)
                        {
                            shouldWrite = true;
                            vouComparar = true;
                        }

                        if (shouldWrite)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }

                Console.WriteLine("Linhas gravadas com sucesso no arquivo '{0}'.", outputFilePath);
            }
            else
            {
                Console.WriteLine("O arquivo especificado não existe: '{0}'.", filePath);
            }
        }


        public static void ConvertCsvToJson(string filePath)
        {
            if (File.Exists(filePath))
            {
                string outputFilePath = Path.ChangeExtension(filePath, ".json");

                List<Dictionary<string, string>> jsonData = new List<Dictionary<string, string>>();

                using (StreamReader reader = new StreamReader(filePath))
                {
                    string[] headers = reader.ReadLine()?.Split(';');

                    if (headers == null)
                    {
                        Console.WriteLine("O arquivo CSV está vazio: '{0}'.", filePath);
                        return;
                    }

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] values = line.Split(';');
                        Dictionary<string, string> data = new Dictionary<string, string>();

                        for (int i = 0; i < headers.Length; i++)
                        {
                            if (i < values.Length)
                            {
                                data[headers[i]] = values[i];
                            }
                            else
                            {
                                data[headers[i]] = string.Empty;
                            }
                        }

                        jsonData.Add(data);
                    }
                }

                string json = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText(outputFilePath, json);

                Console.WriteLine("Arquivo convertido com sucesso para '{0}'.", outputFilePath);
            }
            else
            {
                Console.WriteLine("O arquivo especificado não existe: '{0}'.", filePath);
            }
        }

        public async Task CarregarDadosProcessoAsync(string DiretorioBase, string numeroProcesso)
        {


            string jsonFilePath = Path.Combine(DiretorioBase, "Json", numeroProcesso + "_P0001_Proc.Json");

            if (File.Exists(jsonFilePath))
            {
                MyData dataP = ReadJsonFile(jsonFilePath);
                InsertOrUpdateProcesso(dataP);

                Debug.WriteLine("atualizando base de dados 1");

                RemoverTextoInutil.Instance.AdicionarTextoEspecifico(dataP.NumeroProcesso, "");

                // Carregue os documentos do processo
                string jsonDocFilePath = Path.Combine(DiretorioBase, "json", numeroProcesso + "_table.Json");

                //RemoveLinhasInvalidas(Path.Combine(DiretorioBase, "json", numeroProcesso + "_table.csv"));

                //ConvertCsvToJson(Path.Combine(DiretorioBase, "json", numeroProcesso + "_table.csv"));

                int idProcesso = GetProcessId(numeroProcesso);
                // Para acessar
                int idPjLocal = 0;
                App.Current.Properties["ID_PJ"] = idProcesso;

                if (App.Current.Properties.Contains("ID_PJ"))
                {
                    idPjLocal = (int)App.Current.Properties["ID_PJ"];
                }


                if (File.Exists(jsonDocFilePath))
                {
                    List<CsvData> jsonRecords = ReadJsonFileAsCsvData(jsonDocFilePath);

                    // Insira os registros nas tabelas 'tipo_ato_processual' e 'atoprocessual'
                    foreach (CsvData csvData in jsonRecords)
                    {
                        // Verifica se o registro tem exatamente 4 campos
                        if (IsValidCsvData(csvData))
                        {

                            int tipoAtoProcessualId = GetOrCreateTipoAtoProcessualId(csvData.tipo);
                            int idAtoProcessual = int.Parse(csvData.id);

                            if (!AtoProcessualExists(idAtoProcessual, connectionString))
                            {
                                InsertAtoProcessual(csvData, tipoAtoProcessualId, idProcesso);
                            }
                            else
                            {
                                //edit atoprocessual
                                UpdateAtoProcessual(csvData, tipoAtoProcessualId, idAtoProcessual, idProcesso);
                            }
                        }
                    }
                }

                jsonFilePath = Path.Combine(DiretorioBase, "Json", numeroProcesso + "cab_Polos.json");

                if(File.Exists(jsonFilePath))
                {
                    Debug.WriteLine("atualizando base de dados Polos");
                    string json = File.ReadAllText(jsonFilePath);
                    JObject jsonObject = JObject.Parse(json);

                    var polos = jsonObject["polos"] as JObject;
                    if (polos == null)
                    {
                        throw new InvalidDataException("JSON file doesn't contain 'polos' object");
                    }

                    foreach (var prop in polos.Properties())
                    {
                        if (prop.Name == "Ativo" || prop.Name == "Passivo")
                        {
                            // Converter os valores do array JSON para uma lista de strings
                            List<string> nomesPessoas = prop.Value.ToObject<List<string>>();

                            foreach (string nomePessoa in nomesPessoas)
                            {
                                string nomeFormatado = Regex.Replace(nomePessoa, @"\s{2,}", " ").Trim();
                                int idPessoa = InsertOrUpdatePessoa(nomeFormatado);


                                //inserir parte no processo
                                //antes buscar o código de posição
                                // Obter o idPosicao baseado no nome da posição
                                int idPosicao = new PosicoesDAL().GetIdPosicaoPart(prop.Name.ToLower());

                                //antes verificar se já existe para este processo
                                int idElementoInserido = InsertElementoProcesso(idProcesso);

                                InsertOrUpdateParteDoProcesso(idPessoa, idElementoInserido, idPosicao);
                                //InsertParteDoProcesso(idPessoa, idElementoInserido);
                            }
                        }
                    }
                }



            }
            Debug.WriteLine("finalizando atualização base de dados");
        }
        private static bool IsValidCsvData(CsvData data)
        {
            // Verifica se todos os campos necessários estão presentes e não são nulos
            return data.data_hora != null && data.tipo != null && data.descricao != null && data.id != null;
        }

        public static void LimparTodasAsTabelas()
        {
            connectionString = "Server=localhost;Database=dbprocesso;Uid=root;Pwd=0%Cesar%0;";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Desativar temporariamente a verificação de chaves estrangeiras
                connection.Execute("SET FOREIGN_KEY_CHECKS = 0;");

                // Obter todos os nomes das tabelas
                var tabelas = connection.Query<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbprocesso';");

                // Truncar cada tabela
                foreach (var tabela in tabelas)
                {
                    connection.Execute($"TRUNCATE TABLE `{tabela}`;");
                }

                // Reativar a verificação de chaves estrangeiras
                connection.Execute("SET FOREIGN_KEY_CHECKS = 1;");

                connection.Close();
            }
        }

        public async Task MonitorarDadosAsync(CancellationToken cancellationToken)
        {
            foreach (string processDirectory in Directory.GetDirectories(_baseDirectory))
            {
                string processo = Path.GetFileName(processDirectory);
                string jsonFilePath = Path.Combine(processDirectory, "doc", processo + "Cab.json");

                if (File.Exists(jsonFilePath))
                {
                    MyData dataP = ReadJsonFile(jsonFilePath);
                    InsertOrUpdateProcesso(dataP);

                    // Carregue os documentos do processo
                    string jsonDocFilePath = Path.Combine(processDirectory, "doc", processo + "cab_doc.json");

                    int idProcesso = GetProcessId(processo);

                    if (File.Exists(jsonDocFilePath))
                    {
                        List<CsvData> jsonRecords = ReadJsonFileAsCsvData(jsonDocFilePath);

                        // Insira os registros nas tabelas 'tipo_ato_processual' e 'atoprocessual'
                        foreach (CsvData csvData in jsonRecords)
                        {
                            int tipoAtoProcessualId = GetOrCreateTipoAtoProcessualId(csvData.tipo);
                            int idAtoProcessual = int.Parse(csvData.id);

                            if (!AtoProcessualExists(idAtoProcessual, connectionString))
                            {
                                InsertAtoProcessual(csvData, tipoAtoProcessualId, idProcesso);
                            }
                            else
                            {
                                //edit atoprocessual
                                UpdateAtoProcessual(csvData, tipoAtoProcessualId, idAtoProcessual, idProcesso);
                            }
                        }
                    }

                    jsonFilePath = Path.Combine(processDirectory, "doc", processo + ".json");

                    string json = File.ReadAllText(jsonFilePath);
                    JObject jsonObject = JObject.Parse(json);

                    foreach (var prop in jsonObject.Properties())
                    {
                        if (prop.Name == "Ativo" || prop.Name == "Passivo")
                        {
                            // Dividir os nomes das pessoas que estão separados por ponto e vírgula
                            List<string> nomesPessoas = prop.Value.ToObject<string[]>()
                                .SelectMany(s => s.Split(';'))
                                .Select(s => s.Trim())
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .ToList();

                            foreach (string nomePessoa in nomesPessoas)
                            {
                                string nomeFormatado = Regex.Replace(nomePessoa, @"\s{2,}", " ").Trim();
                                int idPessoa = InsertOrUpdatePessoa(nomeFormatado);
                                //inserir parte no processo
                                int idElementoInserido = InsertElementoProcesso(idProcesso);
                                InsertParteDoProcesso(idPessoa, idElementoInserido);
                            }
                        }
                    }

                }
            }
        }

        public List<CsvData> ReadJsonFileAsCsvData(string jsonFilePath)
        {
            var jsonData = File.ReadAllText(jsonFilePath);
            var csvDataList = JsonConvert.DeserializeObject<List<CsvData>>(jsonData);
            return csvDataList;
        }

        public static void UpdateAtoProcessual(CsvData csvData, int tipoAtoProcessualId, int idAtoProcessual, int idProcesso)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string updateQuery = @"
                            UPDATE atoprocessual
                            SET idprocesso = @idProcesso, DataInclusao = @Data, 
                            idMovimento = @numeroDocumento, tipo = @tipo, resumo = @descricao
                            WHERE idMovimento = @idAtoProcessual;";


                //Debug.WriteLine(updateQuery);


                MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@idProcesso", idProcesso);
                string[] dateFormats = { "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyyHH:mm:ss" };

                DateTime dataInclusao = DateTime.ParseExact(csvData.data_hora, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);

                updateCommand.Parameters.AddWithValue("@numeroDocumento", idAtoProcessual);
                updateCommand.Parameters.AddWithValue("@Data", dataInclusao);
                updateCommand.Parameters.AddWithValue("@tipo", tipoAtoProcessualId);
                updateCommand.Parameters.AddWithValue("@descricao", csvData.descricao);
                updateCommand.Parameters.AddWithValue("@idAtoProcessual", idAtoProcessual);

                updateCommand.ExecuteNonQuery();
            }
        }

        public static int InsertOrUpdatePessoa(string nome)
        {
            //string iniciais = GetInitials(nome);
            // string nomeCriptografado = EncryptName(nome);

            string textoSanitizado = PreprocessamentoTextoIA.SanitizaUPPERCASE(nome);

            string iniciais = GetInitials(nome);

            PreprocessamentoTextoIA preprocessamento = new PreprocessamentoTextoIA();
            string pseudonimo = preprocessamento.GerarPseudonimo(textoSanitizado);
            //pseudonimo = "Helena Rocha P�rez";
            //// Convertendo a string para UTF-8
            //byte[] bytes = Encoding.UTF8.GetBytes(pseudonimo);

            //// Opcional: Convertendo de volta para string para demonstração
            //string utf8String = Encoding.UTF8.GetString(bytes);

            //Debug.WriteLine(utf8String);

            //// Convertendo de ISO-8859-1 (Latin-1) para UTF-16 (C# interno)
            //byte[] latin1Bytes = Encoding.GetEncoding("Windows-1252").GetBytes(pseudonimo);
            //string utf16String = Encoding.UTF8.GetString(latin1Bytes);


            RemoverTextoInutil.Instance.AdicionarTextoEspecifico(nome, pseudonimo);

            RemoverTextoInutil.Instance.AdicionarTextoEspecifico(textoSanitizado, pseudonimo);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                INSERT INTO pessoa (nome, iniciais, pseudonimo)
                VALUES (@nome, @iniciais, @pseudo)
                ON DUPLICATE KEY UPDATE nome = VALUES(nome), iniciais = VALUES(iniciais);
                SELECT LAST_INSERT_ID();";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@nome", textoSanitizado);
                command.Parameters.AddWithValue("@iniciais", iniciais);
                command.Parameters.AddWithValue("@pseudo", pseudonimo);
                //command.Parameters.AddWithValue("@nome_criptografado", nomeCriptografado);

                int idPessoa = Convert.ToInt32(command.ExecuteScalar());

                return idPessoa;
            }
        }


        public static string GetInitials(string nome)
        {
            StringBuilder initials = new StringBuilder();
            bool newWord = true;

            for (int i = 0; i < nome.Length; i++)
            {
                if (newWord && char.IsLetter(nome[i]))
                {
                    initials.Append(nome[i]);
                    newWord = false;
                }
                else if (char.IsWhiteSpace(nome[i]))
                {
                    newWord = true;
                }
            }

            return initials.ToString();
        }


        public static void InsertOrUpdateParteDoProcesso(int idPessoa, int idElemento, int idPosicao)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                INSERT INTO partesdoprocesso (idParte, idElementos, idPosicao)
                VALUES (@idParte, @idElementos, @idPosicao)
                ON DUPLICATE KEY UPDATE
                idParte = VALUES(idParte),
                idposicao = VALUES(idPosicao),
                idElementos = VALUES(idElementos);";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@idParte", idPessoa);
                command.Parameters.AddWithValue("@idElementos", idElemento);
                command.Parameters.AddWithValue("@idPosicao", idPosicao);

                command.ExecuteNonQuery();
            }
        }



        public static void InsertParteDoProcesso(int idPessoa, int idElemento)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                        INSERT INTO partesdoprocesso (idPartes, idElementos)
                        VALUES (@idPartes, @idElementos);";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@idPartes", idPessoa);
                command.Parameters.AddWithValue("@idElementos", idElemento);

                command.ExecuteNonQuery();
            }
        }


        public static int InsertElementoProcesso(int idProcesso)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Verificar se já existe um registro com o idProcesso fornecido
                string checkQuery = @"
                            SELECT IdElemento FROM elementos
                            WHERE idprocesso = @idProcesso;";

                MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@idProcesso", idProcesso);

                object result = checkCommand.ExecuteScalar();
                if (result != null)
                {
                    // Se já existe um registro, retorna o IdElemento existente
                    return Convert.ToInt32(result);
                }

                // Se não existe um registro, insere um novo e retorna o IdElemento
                string insertQuery = @"
                            INSERT INTO elementos (idprocesso)
                            VALUES (@idProcesso);
                            SELECT LAST_INSERT_ID();";

                MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@idProcesso", idProcesso);

                int idElemento = Convert.ToInt32(insertCommand.ExecuteScalar());

                return idElemento;
            }
        }

        private static int GetProcessId(string numeroProcesso)
        {
            int idProcesso = 0;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT id_processo FROM processo WHERE numero_processo = @numeroProcesso;";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@numeroProcesso", numeroProcesso);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            idProcesso = reader.GetInt32(0);
                            reader.Close();

                            query = "SELECT id_processo_Judicial FROM processo_judicial WHERE idProcesso = @idProcesso;";

                            using (MySqlCommand comando = new MySqlCommand(query, connection))
                            {
                                comando.Parameters.AddWithValue("@idProcesso", idProcesso);

                                using (MySqlDataReader recuperaProcessoJudicial = comando.ExecuteReader())
                                {
                                    if (recuperaProcessoJudicial.Read())
                                    {

                                        idProcesso = recuperaProcessoJudicial.GetInt32(0);

                                    }
                                }
                            }
                        }
                    }
                }
                return idProcesso;
            }
        }

        private static void TruncateAtoProcessualTable()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string truncateQuery = "TRUNCATE TABLE atoprocessual;";

                using (MySqlCommand command = new MySqlCommand(truncateQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Desativar a verificação de chaves estrangeiras
                MySqlCommand disableForeignKeyCheck = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", connection);
                disableForeignKeyCheck.ExecuteNonQuery();


                truncateQuery = "TRUNCATE TABLE processo_judicial;";

                using (MySqlCommand command = new MySqlCommand(truncateQuery, connection))
                {
                    command.ExecuteNonQuery();

                    // Truncar a tabela 'elementos'
                    MySqlCommand truncateElementosTable = new MySqlCommand("TRUNCATE TABLE elementos;", connection);
                    truncateElementosTable.ExecuteNonQuery();

                    // Reativar a verificação de chaves estrangeiras
                    MySqlCommand enableForeignKeyCheck = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", connection);
                    enableForeignKeyCheck.ExecuteNonQuery();
                }

                // Desativar a verificação de chaves estrangeiras
                disableForeignKeyCheck = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", connection);
                disableForeignKeyCheck.ExecuteNonQuery();

                truncateQuery = "TRUNCATE TABLE processo;";

                using (MySqlCommand command = new MySqlCommand(truncateQuery, connection))
                {
                    command.ExecuteNonQuery();

                    MySqlCommand truncateElementosTable = new MySqlCommand("TRUNCATE TABLE processo_judicial;", connection);
                    truncateElementosTable.ExecuteNonQuery();


                }



            }
        }

        private static bool AtoProcessualExists(int idMovimento, string connectionString)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM atoprocessual WHERE idMovimento = @idMovimento";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idMovimento", idMovimento);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }


        public class CsvData
        {
            public string id { get; set; }
            public string data_hora { get; set; }
            public string tipo { get; set; }
            public string descricao { get; set; }
        }


        private static List<CsvData> ReadCsvFile(string filePath)
        {
            List<CsvData> csvRecords = new List<CsvData>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                // Pule o cabeçalho
                reader.ReadLine();
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    string[] values = reader.ReadLine().Split(',');

                    CsvData data = new CsvData
                    {
                        id = values[0],
                        data_hora = values[1],
                        tipo = values[2],
                        descricao = values[3]
                    };
                    csvRecords.Add(data);
                }
            }

            return csvRecords;
        }



        private static int GetOrCreateTipoAtoProcessualId(string tipoDocumento)
        {
            int tipoAtoProcessualId;

            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();

                // Verificar se o registro existe na tabela 'tipo_ato_processual'
                string query = "SELECT id_tipo_ato_processual FROM tipo_ato_processual WHERE tipo = @tipoDocumento";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@tipoDocumento", tipoDocumento);

                object result = command.ExecuteScalar();

                if (result != null)
                {
                    // Se o tipo de ato processual existir, obter o ID
                    tipoAtoProcessualId = Convert.ToInt32(result);
                }
                else
                {
                    // Se o tipo de ato processual não existir, inseri-lo e obter o ID gerado
                    query = "INSERT INTO tipo_ato_processual (tipo) VALUES (@tipoDocumento); SELECT LAST_INSERT_ID();";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@tipoDocumento", tipoDocumento);

                    tipoAtoProcessualId = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return tipoAtoProcessualId;
        }


        private static MyData ReadJsonFile(string filePath)
        {
            string jsonContent = File.ReadAllText(filePath);
            JObject jsonData = JsonConvert.DeserializeObject<JObject>(jsonContent);

            MyData data = new MyData
            {
                DataHoraConsulta = jsonData["data da consulta"].Value<string>(),
                NumeroProcesso = jsonData["Número"].Value<string>(),
                Classe = jsonData["Classe"].Value<string>(),
                OrgaoJulgador = jsonData["Órgão Julgador"].Value<string>(),
                ValorCausa = jsonData["Valor da Causa"].Value<double>(),
                SegredoDeJustica = jsonData["Segredo de Justiça"].Value<string>()
            };

            if (jsonData["Assuntos"] != null)
            {
                if (jsonData["Assuntos"] is JArray)
                {
                    data.Assuntos = string.Join(",", jsonData["Assuntos"].ToObject<List<string>>());
                }
                else
                {
                    data.Assuntos = jsonData["Assuntos"].Value<string>();
                }
            }
            return data;
        }

        private static void InsertAtoProcessual(CsvData csvData, int tipoAtoProcessualId, int processoJudicial)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO atoprocessual (idProcesso, idMovimento, dataInclusao, tipo, resumo)
                    VALUES (@id_processoJudicial, @idMovimento, @dataInclusao, @tipo, @resumo)";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id_processoJudicial", processoJudicial);
                command.Parameters.AddWithValue("@idMovimento", csvData.id);
                string[] dateFormats = { "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyyHH:mm:ss" };

                DateTime dataInclusao = DateTime.ParseExact(csvData.data_hora, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);

                command.Parameters.AddWithValue("@dataInclusao", dataInclusao);
                command.Parameters.AddWithValue("@tipo", tipoAtoProcessualId);
                command.Parameters.AddWithValue("@resumo", csvData.descricao);

                command.ExecuteNonQuery();
            }
        }

        private static MySqlConnection GetConnection()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            return connection;
        }

        public class MyData
        {
            public string DataHoraConsulta { get; set; }
            public string NumeroProcesso { get; set; }
            public string Classe { get; set; }
            public string OrgaoJulgador { get; set; }
            public double ValorCausa { get; set; }
            public string SegredoDeJustica { get; set; }
            public string Assuntos { get; set; }
        }






        private static MyData ReadJsonFile_old(string filePath)
        {
            string jsonContent = File.ReadAllText(filePath);
            JObject jsonData = JsonConvert.DeserializeObject<JObject>(jsonContent);

            // Processar o valor da causa
            string valorCausaStr = jsonData["ValorCausa"].Value<string>();
            valorCausaStr = valorCausaStr.Replace("R$", "").Replace(".", "").Trim();
            valorCausaStr = valorCausaStr.Replace(",", ".").Trim();
            double valorCausa = double.Parse(valorCausaStr, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

            MyData data = new MyData
            {
                DataHoraConsulta = jsonData["DataHoraConsulta"].Value<string>(),
                NumeroProcesso = jsonData["NumeroProcesso"].Value<string>(),
                Classe = jsonData["Classe"].Value<string>(),
                OrgaoJulgador = jsonData["OrgaoJulgador"].Value<string>(),
                ValorCausa = valorCausa,
                SegredoDeJustica = jsonData["SegredoDeJustica"].Value<string>()
            };

            if (jsonData["Assuntos"] != null)
            {
                if (jsonData["Assuntos"] is JArray)
                {
                    data.Assuntos = string.Join(",", jsonData["Assuntos"].ToObject<List<string>>());
                }
                else
                {
                    data.Assuntos = jsonData["Assuntos"].Value<string>();
                }
            }
            return data;
        }





        private static void InsertOrUpdateProcesso(MyData data)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                // Verificar se o registro existe e obter id_processo
                string queryCheckExists = "SELECT id_processo FROM processo WHERE numero_processo = @numero_processo";
                MySqlCommand commandCheckExists = new MySqlCommand(queryCheckExists, connection);
                commandCheckExists.Parameters.AddWithValue("@numero_processo", data.NumeroProcesso);
                object result = commandCheckExists.ExecuteScalar();
                long idProcesso;

                // Converter a data e hora no formato correto
                DateTime parsedDate = DateTime.ParseExact(data.DataHoraConsulta, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                string formattedDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");

                int orgaoJurisdicionalId = GetOrCreateOrgaoJurisdicionalId(data.OrgaoJulgador);


                if (result == null)
                {
                    // Inserir na tabela 'processo'
                    string queryInsertProcesso = @"
                            INSERT INTO processo (
                                numero_processo, Tipo, Ultimo_movimento, Classe, ClassePublicidade
                            ) VALUES (
                                @numero_processo, 1, @Data, 1, @Classe_Publicidade
                            )";

                    MySqlCommand commandInsertProcesso = new MySqlCommand(queryInsertProcesso, connection);
                    commandInsertProcesso.Parameters.AddWithValue("@numero_processo", data.NumeroProcesso);
                    commandInsertProcesso.Parameters.AddWithValue("@Data", formattedDate);
                    //commandInsertProcesso.Parameters.AddWithValue("@Classe", data.Classe);

                    bool segredoDeJustica = data.SegredoDeJustica == "true";
                    string classePublicidade = segredoDeJustica ? "SEGREDO DE JUSTIÇA" : "PÚBLICO";
                    commandInsertProcesso.Parameters.AddWithValue("@Classe_Publicidade", classePublicidade);

                    commandInsertProcesso.ExecuteNonQuery();

                    // Recuperar o id_processo inserido
                    idProcesso = commandInsertProcesso.LastInsertedId;
                }
                else
                {
                    // Atualizar o registro existente na tabela 'processo'
                    idProcesso = Convert.ToInt64(result);

                    string queryUpdateProcesso = @"
                            UPDATE processo SET
                                Tipo = 1,
                                Ultimo_movimento = @Data,
                                Classe = 1,
                                Classepublicidade = @Classe_Publicidade
                            WHERE
                                id_processo = @id_processo";

                    MySqlCommand commandUpdateProcesso = new MySqlCommand(queryUpdateProcesso, connection);
                    commandUpdateProcesso.Parameters.AddWithValue("@id_processo", idProcesso);
                    commandUpdateProcesso.Parameters.AddWithValue("@Data", formattedDate);
                    //commandUpdateProcesso.Parameters.AddWithValue("@Classe", data.Classe);
                    bool segredoDeJustica = data.SegredoDeJustica == "true";
                    string classePublicidade = segredoDeJustica ? "SEGREDO DE JUSTIÇA" : "PÚBLICO";
                    commandUpdateProcesso.Parameters.AddWithValue("@Classe_Publicidade", classePublicidade);

                    commandUpdateProcesso.ExecuteNonQuery();
                }

                // Verificar se o registro existe na tabela 'processo_judicial'
                string queryCheckExistsProcessoJudicial = "SELECT COUNT(*) FROM processo_judicial WHERE idprocesso = @id_processo";
                MySqlCommand commandCheckExistsProcessoJudicial = new MySqlCommand(queryCheckExistsProcessoJudicial, connection);
                commandCheckExistsProcessoJudicial.Parameters.AddWithValue("@id_processo", idProcesso);
                long countProcessoJudicial = (long)commandCheckExistsProcessoJudicial.ExecuteScalar();

                if (countProcessoJudicial == 0)
                {
                    // Inserir na tabela 'processo_judicial'
                    string queryInsertProcessoJudicial = @"
                            INSERT INTO processo_judicial (
                                idProcesso, valor_da_causa, unidade_jurisdicional
                            ) VALUES (
                                @id_processo, @valorCausa, @unidadeJurisdicional
                            )";

                    MySqlCommand commandInsertProcessoJudicial = new MySqlCommand(queryInsertProcessoJudicial, connection);
                    commandInsertProcessoJudicial.Parameters.AddWithValue("@id_processo", idProcesso);
                    commandInsertProcessoJudicial.Parameters.AddWithValue("@valorCausa", data.ValorCausa);
                    commandInsertProcessoJudicial.Parameters.AddWithValue("@unidadeJurisdicional", orgaoJurisdicionalId);

                    commandInsertProcessoJudicial.ExecuteNonQuery();
                }
                else
                {
                    // Atualizar o registro existente na tabela 'processo_judicial'
                    string queryUpdateProcessoJudicial = @"
                            UPDATE processo_judicial SET
                                valor_da_causa = @valorCausa,
                                unidade_jurisdicional = @unidadeJurisdicional
                            WHERE
                                idProcesso = @id_processo";

                    MySqlCommand commandUpdateProcessoJudicial = new MySqlCommand(queryUpdateProcessoJudicial, connection);
                    commandUpdateProcessoJudicial.Parameters.AddWithValue("@id_processo", idProcesso);
                    commandUpdateProcessoJudicial.Parameters.AddWithValue("@valorCausa", data.ValorCausa);
                    commandUpdateProcessoJudicial.Parameters.AddWithValue("@unidadeJurisdicional", orgaoJurisdicionalId);

                    commandUpdateProcessoJudicial.ExecuteNonQuery();
                }
            }
        }
        private static int GetOrCreateOrgaoJurisdicionalId(string orgaoJurisdicional)
        {
            int orgaoJurisdicionalId;

            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();

                // Verificar se o registro existe na tabela 'orgaojurisdicional'
                string query = "SELECT idorgao FROM orgaojurisdicional WHERE orgaonome = @orgaoJurisdicional";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@orgaoJurisdicional", orgaoJurisdicional);

                object result = command.ExecuteScalar();

                if (result != null)
                {
                    // Se o órgão jurisdicional existir, obter o ID
                    orgaoJurisdicionalId = Convert.ToInt32(result);
                }
                else
                {
                    // Se o órgão jurisdicional não existir, inseri-lo e obter o ID gerado
                    query = "INSERT INTO orgaojurisdicional (orgaonome) VALUES (@orgaoJurisdicional); SELECT LAST_INSERT_ID();";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@orgaoJurisdicional", orgaoJurisdicional);

                    orgaoJurisdicionalId = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return orgaoJurisdicionalId;
        }


        private static void InsertData(MyData data)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = @"
                        INSERT INTO processo (
                            numero_processo, Tipo, Ultimo_movimento, Classe, ClassePublicidade
                        ) VALUES (
                            @numero_processo, 1, @DataHoraConsulta, 1, @ClassePublicidade
                        )";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@numero_processo", data.NumeroProcesso);
                // Converter a data e hora no formato correto
                DateTime parsedDate = DateTime.ParseExact(data.DataHoraConsulta, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                string formattedDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");

                command.Parameters.AddWithValue("@DataHoraConsulta", formattedDate);
                command.Parameters.AddWithValue("@ClassePublicidade", data.SegredoDeJustica); // Atualizado para usar a string diretamente

                command.ExecuteNonQuery();
            }
        }
    }
}
