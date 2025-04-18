using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GabIA.ENT;
using GabIA.BLL;
using System.Diagnostics;
using System.Windows;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Text.RegularExpressions;
using static GabIA.WPF.APIRequest;


namespace GabIA.WPF
{
    public class PreprocessamentoTextoIA
    {

        private static List<string> nomes;
        private static List<string> sobrenomes1;
        private static List<string> sobrenomes2;
        // Constante para o tamanho máximo de cada "chunk"
        private const int MaxChunkSize = 32 * 1024; // 32KB
        private string diretorioTemplate = @"d:\PJe\Dados\Templates";


        public class Documento
        {
            public string tpo_ato { get; set; }
            public string nr_proc { get; set; }
            public string id { get; set; }
            public int parte { get; set; }
            public int tPartes { get; set; }
            public string maxTokens { get; set; }
            public string model { get; set; }
            public List<Linha> linhas { get; set; }
        }


        public class Linha
        {
            public string linha { get; set; }
        }

        

        public class Message
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        public class Choice
        {
            public int index { get; set; }
            public Message message { get; set; }
            public string finish_reason { get; set; }
        }

        public class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        public class Root
        {
            public string model { get; set; }
            public double temperature { get; set; }
            public List<Message> messages { get; set; }
            public string id { get; set; }
            public string @object { get; set; }
            public int created { get; set; }
            public List<Choice> choices { get; set; }
            public Usage usage { get; set; }
            public string t_id { get; set; }
            public string f { get; set; }
            public string t_ato { get; set; }
            public string pn { get; set; }

            public Metadata metadata { get; set; }
        }
        public class Metadata
        {
            public string f { get; set; }
        }


        public PreprocessamentoTextoIA()
        {
            // Inicializar as listas
            nomes = new List<string>();
            sobrenomes1 = new List<string>();
            sobrenomes2 = new List<string>();

            // Defina o caminho para o arquivo CSV
            string csvPath = @"d:\PJe\dados\pseudonimo.csv";

            // Usar StreamReader com a codificação correta (aqui usamos UTF-8)
            using (var reader = new StreamReader(csvPath, Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<NomeDTO>();
                foreach (var record in records)
                {
                    nomes.Add(record.Nome1);
                    sobrenomes1.Add(record.Sobrenome1);
                    sobrenomes2.Add(record.Sobrenome2);
                    // Supondo que nomesDoMeio será preenchido a partir de algum campo do seu csv
                    // nomesDoMeio.Add(record.NomeDoMeio);
                }
            }
        }

        

        public bool VerificarMembrosNulos(Documento documento)
        {
            // Verifica se as propriedades de nível superior são nulas
            if (documento == null ||
                documento.tpo_ato == null ||
                documento.nr_proc == null ||
                documento.id == null ||
                documento.maxTokens == null ||
                documento.model == null)
            {
                return true;
            }

            // Verifica se qualquer linha é nula ou tem a propriedade 'linha' nula
            foreach (var linha in documento.linhas)
            {
                if (linha == null || linha.linha == null)
                {
                    return true;
                }
            }

            return false;
        }
        public void DeserializeJsonFile(string inputFile, string outputFile)
        {
            // Verifique se o arquivo de entrada existe
            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException($"O arquivo {inputFile} não foi encontrado.");
            }


            string jsonContent = File.ReadAllText(inputFile);

            // Desserializa o conteúdo JSON
            var jsonObject = JsonConvert.DeserializeObject<List<JObject>>(jsonContent);

            // Percorre todos os objetos e imprime todas as chaves e valores
            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                // Percorre todos os objetos e grava o conteúdo de "choices[0].message.content" no arquivo de saída
                foreach (var obj in jsonObject)
                {
                    // Verifica se o objeto possui a propriedade 'choices'
                    if (obj.ContainsKey("choices"))
                    {
                        JArray choices = (JArray)obj["choices"];
                        // Verifica se o primeiro elemento em 'choices' possui a propriedade 'message'
                        if (choices.Count > 0 && choices[0] is JObject choice && choice.ContainsKey("message"))
                        {
                            JObject message = (JObject)choice["message"];
                            // Verifica se a 'message' possui a propriedade 'content'
                            if (message.ContainsKey("content"))
                            {
                                string content = (string)message["content"];
                                // Grava o conteúdo no arquivo de saída
                                sw.WriteLine(content);
                            }
                        }
                    }
                }
            }

        }

        // Método auxiliar para imprimir recursivamente as propriedades de um JObject
        public static void PrintProperties(JObject obj, string parentProperty = "")
        {
            foreach (var property in obj.Properties())
            {
                string propertyName = string.IsNullOrEmpty(parentProperty) ? property.Name : $"{parentProperty}.{property.Name}";
                if (property.Value is JObject nestedObj)
                {
                    // Se o valor é um objeto, recursivamente imprime suas propriedades
                    PrintProperties(nestedObj, propertyName);
                }
                else if (property.Value is JArray array)
                {
                    for (int i = 0; i < array.Count; i++)
                    {
                        if (array[i] is JObject arrayObj)
                        {
                            // Se o valor é uma matriz de objetos, recursivamente imprime as propriedades de cada objeto
                            PrintProperties(arrayObj, $"{propertyName}[{i}]");
                        }
                        else
                        {
                            // Imprime a chave e o valor para os elementos da matriz
                            Debug.WriteLine($"{propertyName}[{i}]: {array[i]}");
                        }
                    }
                }
                else
                {
                    // Imprime a chave e o valor para propriedades não aninhadas
                    Debug.WriteLine($"{propertyName}: {property.Value}");
                }
            }
        }


        public void separaLinhasJson(string arquivoJson, string diretorioDestino)
        {
            using (StreamReader sr = new StreamReader(arquivoJson))
            {
                string linha;
                while ((linha = sr.ReadLine()) != null)
                {
                    string filename = ExtractFileName(linha);
                    filename = filename + ".json";
                    string destino = Path.Combine(diretorioDestino, filename);
                    Combine(destino, linha);
                }
            }
        }

        private string ExtractFileName(string linha)
        {
            Debug.WriteLine(linha);
            string pattern = @"""f"": ""(\d+)""";
            Match match = Regex.Match(linha, pattern);
            if (match.Success)
            {
                string filename = match.Groups[1].Value;
                return filename;
            }
            else
            {
                throw new Exception("Padrão não encontrado na linha do JSON.");
            }
        }

        private void Combine(string arquivoDestino, string linha)
        {
            using (StreamWriter sw = new StreamWriter(arquivoDestino, true))
            {
                sw.WriteLine(linha);
            }
        }


        public static void DividirEGravarOTexto(string arquivoEntrada, string diretorioSaida, int tamanhoMaximo)
        {
            // Ler o texto do arquivo de entrada
            string texto = File.ReadAllText(arquivoEntrada);

            // Dividir o texto em partes
            List<string> partes = DividirTexto(texto, tamanhoMaximo);

            // Obter o nome base do arquivo de entrada (sem o diretório ou extensão)
            string nomeBaseArquivo = Path.GetFileNameWithoutExtension(arquivoEntrada);

            // Gravar cada parte em um arquivo separado
            for (int i = 0; i < partes.Count; i++)
            {
                // Se a parte é vazia, pula a iteração atual do loop
                if (string.IsNullOrWhiteSpace(partes[i]))
                {
                    continue;
                }

                // Criar o nome do arquivo de saída com o nome base, o sufixo e a extensão .txt
                string arquivoSaida = Path.Combine(diretorioSaida, $"{nomeBaseArquivo}_P{i + 1:000}.txt");

                // Gravar a parte no arquivo de saída
                File.WriteAllText(arquivoSaida, partes[i]);
            }
        }


        public static void DividirGravar(string arquivoEntrada, string diretorioSaida, int tamanhoMaximo)
        {
            // Ler o texto do arquivo de entrada
            string texto = File.ReadAllText(arquivoEntrada);

            // Dividir o texto em partes
            List<string> partes = DividirTexto(texto, tamanhoMaximo);

            // Gravar cada parte em um arquivo separado
            for (int i = 0; i < partes.Count; i++)
            {
                // Criar o nome do arquivo de saída
                string arquivoSaida = Path.Combine(diretorioSaida, $"parte_{i + 1}.txt");

                // Gravar a parte no arquivo de saída
                File.WriteAllText(arquivoSaida, partes[i]);
            }
        }

        public static string SanitizaUPPERCASE(string texto)
        {
            // Define a cultura para português
            CultureInfo cultura = new CultureInfo("pt-BR");

            // Converte para maiúsculas e remove os acentos
            string textoMaiusculoSemAcentos = new string(texto
                .ToUpper(cultura)
                .Normalize(NormalizationForm.FormD)
                .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            // Substituir "ç" por "c"
            textoMaiusculoSemAcentos = textoMaiusculoSemAcentos.Replace('Ç', 'C');

            //// Converter a string para uma sequência de bytes em UTF-8
            //byte[] bytesUtf8 = Encoding.UTF8.GetBytes(textoMaiusculoSemAcentos);

            //// Converter os bytes em UTF-8 de volta para uma string
            //string stringUtf8 = Encoding.UTF8.GetString(bytesUtf8);

            // Retorna o texto sanitizado
            return textoMaiusculoSemAcentos;
        }


        public static List<string> DividirTexto(string texto, int tamanhoMaximo)
        {
            List<string> partes = new List<string>();
            int posicaoAtual = 0;

            while (posicaoAtual < texto.Length)
            {
                int ultimaOcorrenciaPontuacao = texto.Substring(posicaoAtual, Math.Min(tamanhoMaximo, texto.Length - posicaoAtual))
                                            .LastIndexOfAny(new char[] { '.', '?', '!', ':', ';' });

                if (ultimaOcorrenciaPontuacao == -1)
                {
                    ultimaOcorrenciaPontuacao = Math.Min(tamanhoMaximo, texto.Length - posicaoAtual);
                }
                else
                {
                    ultimaOcorrenciaPontuacao += 1;
                }

                string parte = texto.Substring(posicaoAtual, ultimaOcorrenciaPontuacao);
                partes.Add(parte);
                posicaoAtual += ultimaOcorrenciaPontuacao;
            }

            return partes;
        }
        public string GerarPseudonimo(string nomePessoa)
        {
            Random random = new Random();

            string[] palavras = nomePessoa.Split(' ');

            int numPalavras = palavras.Length;

            string pseudonimo = "";

            if (numPalavras == 1)
            {
                string nome = nomes[random.Next(nomes.Count)];
                pseudonimo = nome;
            }
            else if (numPalavras == 2)
            {
                string nome = nomes[random.Next(nomes.Count)];
                string sobrenome = sobrenomes1[random.Next(sobrenomes1.Count)];
                pseudonimo = $"{nome} {sobrenome}";
            }
            else if (numPalavras > 2 && numPalavras <= 4)
            {


                string nome = nomes[random.Next(nomes.Count)];
                string sobrenome = sobrenomes1[random.Next(sobrenomes1.Count)];
                pseudonimo = $"{nome} {sobrenome}";
            }
            else if (numPalavras > 4)
            {
                string nome = nomes[random.Next(nomes.Count)];
                string preposicao = "de"; // Adicione outras preposições se desejar
                string sobrenome1 = sobrenomes1[random.Next(sobrenomes1.Count)];
                string sobrenome2 = sobrenomes2[random.Next(sobrenomes2.Count)];
                pseudonimo = $"{nome} {preposicao} {sobrenome1} {sobrenome2}";
            }

            return pseudonimo;
        }


        public class NomeDTO
        {
            public string Nome1 { get; set; }
            public string Sobrenome1 { get; set; }
            public string Sobrenome2 { get; set; }
        }




        private List<string> DevolveJsonl(string filecontent)
        {
            string[] linhas = File.ReadAllLines(filecontent);

            List<string> conteudo = new List<string>();
            foreach (string linha in linhas)
            {
                string linhaLimpa = PreprocessJsonString(linha);
                if (linhaLimpa.Length > 0)
                {
                    var objetoJson = new { linha = linhaLimpa };
                    string json = JsonConvert.SerializeObject(objetoJson);
                    conteudo.Add(json);
                }
            }
            return conteudo;
        }


        public string PreprocessJsonString(string input)
        {
            string output = input;

            // Substitui as aspas simples por aspas duplas
            output = output.Replace('\'', '\"');

            // Substitui as aspas especiais por aspas padrão
            output = output.Replace('“', '\"').Replace('”', '\"');

            // Escapa aspas duplas dentro de valores de string
            Regex regex = new Regex(@"(?<=:).*?(?=\,|\})");
            output = regex.Replace(output, m => m.Value.Replace("\"", "\\\""));

            // Remove caracteres de controle ASCII (exceto espaços em branco)
            for (int i = 0; i < 32; i++)
            {
                if (i != 9 && i != 10 && i != 13)  // exceto tabulação, nova linha e retorno de carro
                {
                    output = output.Replace(((char)i).ToString(), "");
                }
            }

            // Substitui alguns caracteres unicode problemáticos
            output = output.Replace('\u0085'.ToString(), "");  // Next Line (NEL)
            output = output.Replace('\u2028'.ToString(), "");  // Line Separator
            output = output.Replace('\u2029'.ToString(), "");  // Paragraph Separator

            return output;
        }




        public void ConverterParaJsonL(string arquivoTexto, string arquivoJsonL)
        {
            string[] linhas = File.ReadAllLines(arquivoTexto);

            using (StreamWriter writer = new StreamWriter(arquivoJsonL))
            {
                foreach (string linha in linhas)
                {
                    var objetoJson = new { linha };
                    string json = JsonConvert.SerializeObject(objetoJson);
                    writer.WriteLine(json);
                }
            }
        }


        private static List<string> SplitIntoChunks(string str)
        {
            var paragraphs = str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
            var chunks = new List<string>();
            var chunk = new StringBuilder();

            foreach (var paragraph in paragraphs)
            {
                if (Encoding.UTF8.GetByteCount(chunk.ToString() + paragraph) > MaxChunkSize)
                {
                    chunks.Add(chunk.ToString());
                    chunk.Clear();
                }

                chunk.AppendLine(paragraph);
            }

            if (chunk.Length > 0)
            {
                chunks.Add(chunk.ToString());
            }

            return chunks;
        }
        private string BuscarTipoPeca(string idMovimento, string numeroProcesso)
        {
            OperacoesBLL operacoes = new OperacoesBLL(/* coloque aqui as dependências necessárias */);
            List<AtoProcessualENT> atosDoProcesso = operacoes.BuscarTodosOsAtosDoProcesso(numeroProcesso);
            List<TipoAtoProcessualENT> tiposDeAtos = operacoes.BuscarTodosOsTiposDeAtosProcessuais();

            Debug.WriteLine(tiposDeAtos.Count());
            // Procura o AtoProcessualENT correspondente usando o idMovimento
            var atoProcessual = atosDoProcesso.FirstOrDefault(a => a.IdMovimento == idMovimento);
            var tipo = tiposDeAtos.FirstOrDefault(a => a.Id_Tipo_Ato_processual.ToString() == atoProcessual.Tipo.ToString());
            // Se encontrado, retorna o tipo
            if (atoProcessual != null && tipo != null)
            {
                return tipo.Tipo;
            }
            return null;
        }

        public void ProcessarDiretorioEmParalelo(string diretorio, string tipoProcessamento, string arquivoJason)
        {
            string destinationDirectory = Path.GetDirectoryName(diretorio);
            string diretorioSaida = destinationDirectory;

            // Obtendo todos os arquivos do diretório
            var arquivos = Directory.GetFiles(diretorio);

            int task_id = 1; // Inicializando o task_id

            // Criando um StringBuilder para armazenar todos os objetos JSON
            int icount = 0;

            StringBuilder configs = new StringBuilder();
            foreach (var arquivo in arquivos)
            {
                icount++;
                switch (tipoProcessamento)
                {
                    case "corrige":

                        if (File.Exists(arquivo))
                        {
                            // Carrega o template
                            string textoTemplate = "reescreva o texto na codificação UTF-8, eliminando cabeçalhos e rodapés inúteis, elimine quebras de linha indesejáveis; retire caracteres especiais e outros caracteres soltos e sem significado, tornando o texto fluído, sem eliminar qualquer trecho, e esteticamente bem formatado:";

                            string jsonlContent = File.ReadAllText(arquivo);

                            // Verifica se o JSON é válido antes de processá-lo
                            if (IsValidJson(jsonlContent))
                            {
                                //Debug.WriteLine(IsValidJson(jsonlContent));
                                Debug.WriteLine($"convertendo {arquivo} é {icount} de {arquivos.Count()} arquivos no total");

                                dynamic header = JsonConvert.DeserializeObject(jsonlContent);


                                StringBuilder sb = new StringBuilder();
                                sb.Append("<inicio do texto>");
 
                                foreach (var linha in header.linhas)
                                {
                                    dynamic linhaObj = JsonConvert.DeserializeObject(linha.ToString());
                                    sb.Append(linhaObj.linha.ToString() + "\n");  // Adiciona cada linha ao StringBuilder
                                }

                                sb.Append("</fim do texto>");

                                // Concatena o template, a tag "<inicio do texto>", o conteúdo do arquivo e a tag "</fim do texto>"
                                string texto = textoTemplate + sb.ToString();
                                Debug.WriteLine($"Tamanho do texto: {texto.Length}");
                                //string tipo_ato = "";
                                //MessageBox.Show(header.tpo_ato.ToString());
                                // Criando o JObject que representa o seu JSON
                                JObject config = new JObject(
                                    new JProperty("model", IdentificarModelo(arquivo)), // Chama função para identificar o modelo
                                    new JProperty("temperature", 0.2),
                                    new JProperty("messages",
                                        new JArray(
                                            new JObject(
                                                new JProperty("role", "user"),
                                                new JProperty("content", texto)
                                            )
                                        )
                                    ),
                                    new JProperty("metadata",
                                        new JObject(
                                            new JProperty("t_id", task_id++),
                                            new JProperty("f", Path.GetFileNameWithoutExtension(arquivo)),
                                            new JProperty("t_ato", header.tpo_ato.ToString()),
                                            new JProperty("pn", header.nr_pro.ToString()),
                                            new JProperty("id", header.id.ToString())))
                                );

                                configs.AppendLine(config.ToString(Formatting.None));
                            }
                            else
                            {
                                Debug.WriteLine("JSON inválido.");
                                continue; // Pula para o próximo arquivo ou interrompe o processamento
                            }
                        }
                        break;
         
                    case "sumariza":
                        Debug.WriteLine("sumariza");
                        // Carrega o template
                        string textoTemplateB = CarregarTemplate(IdentificarTipoResumo(arquivo) + ".txt");

                        // Lê e deserializa o cabeçalho do arquivo jsonl
                        string jsonlContentB = File.ReadAllText(arquivo);
                        string[] jsonlLinesB = jsonlContentB.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        var headerB = JsonConvert.DeserializeObject<dynamic>(jsonlLinesB[0]);

                        // Concatena o template, a tag "<inicio do texto>", o conteúdo do arquivo e a tag "</fim do texto>"
                        string textoB = textoTemplateB + "<inicio do texto>" + jsonlLinesB[1] + "</fim do texto>";

                        // Criando o JObject que representa o seu JSON
                        JObject configB = new JObject(
                            new JProperty("model", IdentificarModelo(arquivo)), // Chama função para identificar o modelo
                            new JProperty("temperature", 0.2),
                            new JProperty("messages",
                                new JArray(
                                    new JObject(
                                        new JProperty("role", "user"),
                                        new JProperty("content", textoB)
                                    )
                                )
                            ),
                            new JProperty("metadata",
                                new JObject(
                                    new JProperty("task_id", task_id++),
                                    new JProperty("filename", Path.GetFileNameWithoutExtension(arquivo)),
                                    new JProperty("tipo_ato_processual", headerB.tipo_ato_processual),
                                    new JProperty("processo_numero_processo", headerB.processo_numero_processo),
                                    new JProperty("id", headerB.id)))
                        );

                        configs.AppendLine(configB.ToString(Formatting.None));
                        break;
                }
            }

            // Salvar o StringBuilder no arquivo de saída
            File.WriteAllText(Path.Combine(diretorioSaida, arquivoJason), configs.ToString());
        }



        public static void CompareDirectoriesAndLogDifferences(string verifierDir, string verifiedDir, string logFile)
        {
            // Lista os arquivos nos diretórios
            var verifierFiles = Directory.GetFiles(verifierDir, "P*.txt")
                                         .Select(Path.GetFileNameWithoutExtension)
                                         .ToList();
            var verifiedFiles = Directory.GetFiles(verifiedDir, "P*.json")
                                         .Select(f => Regex.Match(Path.GetFileNameWithoutExtension(f), @"P\d+").Value)
                                         .Distinct()
                                         .ToList();

            // Encontra arquivos que estão no verificador, mas não no verificado
            var missingFiles = verifierFiles.Except(verifiedFiles).ToList();

            // Escreve as diferenças no arquivo de log
            using (StreamWriter sw = new StreamWriter(logFile))
            {
                foreach (var file in missingFiles)
                {
                    sw.WriteLine(file + " está presente no diretório verificador, mas não no verificado.");
                }
            }
        }


    public bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //Para objeto único
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //Para coleções de objetos
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    // Exceção encontrada, que significa que o string de entrada não é um JSON válido
                    return false;
                }
                catch (Exception ex) //Alguma outra exceção
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        private string IdentificarTipoResumo(string arquivo)
        {
            // Lista de palavras-chave e a correspondência de retorno
            var categorias = new Dictionary<List<string>, string> {
                    { new List<string> { "excelentíssimo", "propor", "ação", "pedido" }, "inicial" },
                    { new List<string> { "tutela", "urgência", "pedido" }, "inicial" },
                    { new List<string> { "ação", "fatos", "pedido" }, "inicial" },
                    { new List<string> { "fatos", "direito", "causa" }, "inicial" },
                    { new List<string> { "parecer", "promoto", "promotoria" }, "parecer" },
                    { new List<string> { "contestação" }, "contestacao" },
                    { new List<string> { "réplica" }, "replica" },
                    { new List<string> { "replica" }, "replica" },
                    { new List<string> { "certidão", "nascimento" }, "nascimento" },
                    { new List<string> { "certidão", "casamento"  }, "casamento" },
                    { new List<string> { "certidão", "óbito"  }, "obito" },
                    { new List<string> { "certidão" }, "certidao" },
                    { new List<string> { "despacho" }, "despacho" },
                    { new List<string> { "mandado" }, "mandado" },
                    // adicione mais categorias conforme necessário
                };

            // Lê todo o texto do arquivo
            var textoDoArquivo = File.ReadAllText(arquivo);

            // Transforma o texto em minúsculo para garantir que a correspondência não seja sensível a maiúsculas e minúsculas
            textoDoArquivo = textoDoArquivo.ToLower();

            // Percorre cada categoria para encontrar correspondências
            foreach (var categoria in categorias)
            {
                bool todasAsPalavrasEncontradas = categoria.Key.All(palavra => textoDoArquivo.Contains(palavra));

                // Retorna o valor da categoria se todas as palavras forem encontradas
                if (todasAsPalavrasEncontradas)
                {
                    return categoria.Value;
                }
            }
            // Retorna "desconhecido" ou qualquer outra string de sua preferência se nenhuma correspondência for encontrada
            return "texto";
        }

        private string IdentificarModelo(string arquivo)
        {
            // Obtendo o tamanho do arquivo em bytes
            FileInfo info = new FileInfo(arquivo);
            long tamanhoDoArquivo = info.Length;

            // Retornando o modelo baseado no tamanho do arquivo
            if (tamanhoDoArquivo < 7000)
            {
                return "gpt-3.5-turbo-1106";
            }
            else
            {
                return "gpt-4-1106-preview";
            }
        }

        public static void RegularizaArquivos(string diretorio, int tamanhoMaximo)
        {
            string[] arquivos = Directory.GetFiles(Path.GetDirectoryName(diretorio));

            int iaFile = 0;

            foreach (string arquivo in arquivos)
            {
                iaFile++;

                string texto = File.ReadAllText(arquivo);

                List<string> partes = DividirTexto(texto, tamanhoMaximo);

                // determine the base file name
                string nomeArquivo = Path.GetFileNameWithoutExtension(arquivo);

                for (int i = 0; i < partes.Count; i++)
                {
                    // Check if the last portion has less than 10 lines
                    if (i == partes.Count - 1 && partes[i].Split('\n').Length < 10 && i > 0)
                    {
                        // Append this portion to the previous file
                        string previousFile = Path.Combine(diretorio, $"{nomeArquivo}{i:000}.txt");
                        string previousContent = File.ReadAllText(previousFile);
                        File.WriteAllText(previousFile, previousContent + "\n" + partes[i]);
                        Debug.WriteLine($" Gravado: {iaFile} !");
                        continue;
                    }

                    // Construct the new file name, using a format of "name001", "name002", etc.
                    string novoArquivo = Path.Combine(diretorio, $"{nomeArquivo}{i + 1:000}.txt");

                    // Escreve o arquivo
                    File.WriteAllText(novoArquivo, partes[i]);
                    Debug.WriteLine($" Gravado: {iaFile} !");
                }
            }
        }
        private string CarregarTemplate(string nomeArquivo)
        {
            return File.ReadAllText(Path.Combine(diretorioTemplate, nomeArquivo), Encoding.UTF8);
        }
        
        public void CriaArquivosDeDescricao()
        {
            var csvPath = @"d:\PJe\Dados\tipos de atos.csv";
            var outputPath = @"d:\PJe\Dados\Templates";

            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<dynamic>().ToList();

                foreach (var record in records)
                {
                    var descricao = record.descricao as string;
                    if (!string.IsNullOrEmpty(descricao))
                    {
                        string fileName = Path.Combine(outputPath, $"{descricao}.txt");
                        File.WriteAllText(fileName, JsonConvert.SerializeObject(record));
                    }
                }
            }
        }
    }
}
