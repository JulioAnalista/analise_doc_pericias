using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using GabIA.BLL;
using GabIA.ENT;
using System.Windows;
using static GabIA.WPF.APIRequest;
using Microsoft.ML;
using GabIA.DAL;

namespace GabIA.WPF
{
    public class OrganizaPecas
    {
        //ia_
        public void RegularizaArquivosPorTamanhoToJson(string diretorio, string destinationDirectory, int tamanhoMaximo, List<AtoProcessualENT> atosDoProcesso)
        {
            // Criando instâncias de cada DAL
            ProcessoDAL processoDAL = new ProcessoDAL();
            ProcessoJudicialDAL processoJudicialDAL = new ProcessoJudicialDAL();
            AtoProcessualDAL atoProcessualDAL = new AtoProcessualDAL();
            ElementosDAL elementosDAL = new ElementosDAL();
            PartesDoProcessoDAL partesdoProcessoDAL = new PartesDoProcessoDAL();
            CausaDePedirDAL causasDePedirDAL = new CausaDePedirDAL();
            PedidoDAL pedidosDAL = new PedidoDAL();

            // Criando uma instância de ProcessoBLL com as instâncias DAL
            ProcessoBLL process = new ProcessoBLL(processoDAL, processoJudicialDAL, atoProcessualDAL,
                                                  elementosDAL, partesdoProcessoDAL, causasDePedirDAL,
                                                  pedidosDAL);

            string[] arquivos = Directory.GetFiles(diretorio);
            string idPeticaoInicial = "";
            bool ignore = false;


            // Ordenar os arquivos pelo ID
            var arquivosOrdenados = arquivos.Select(arquivo => new
            {
                NomeArquivo = arquivo,
                ID = ExtractId(arquivo)
            })
            .OrderBy(arquivo => arquivo.ID)
            .Select(arquivo => arquivo.NomeArquivo)
            .ToArray();



            int iaFile = 0;

            OperacoesBLL operacoes = new OperacoesBLL();
            List<TipoAtoProcessualENT> tiposDeAtos = operacoes.BuscarTodosOsTiposDeAtosProcessuais();
            string dirPecas = Path.GetDirectoryName(diretorio);
            string dirPc = Path.GetDirectoryName(dirPecas);
            string dirProc = Path.GetFileName(dirPc);

            ProcessoCompletoENT processo = process.ObterProcessoCompletoPorNumero(Path.GetFileName(dirProc));

            Debug.WriteLine(processo.IdPJ);

            List<AtoProcessualENT> todosOsAtos = operacoes.BuscarTodosOsAtosDoProcesso(Path.GetFileName(dirPc));


            foreach (string arquivo in arquivosOrdenados)
            {

                iaFile++;
                ignore = false;
                // Determine the base file name
                string nomeArquivo = Path.GetFileNameWithoutExtension(arquivo);

                string idMovimento = "";

                // Extrair a parte numérica
                idMovimento = ExtractNumericPart(nomeArquivo);


                string texto = File.ReadAllText(arquivo, Encoding.UTF8);

                if (texto.Length < 50)
                {
                    //idPeticaoInicial = "132934818";
                    if (texto.StartsWith("vide ID:"))
                    {
                        ignore = true;
                    }
                    else
                    {
                        ignore = true;
                        // Primeiro, crie uma instância da classe OperacoesBLL
                        OperacoesBLL operacoesBLL = new OperacoesBLL();
                        MessageBox.Show("Aqui tenho que corrigir o problema das petições em anexo");
                        // Em seguida, use essa instância para chamar o método
                        //int proxID = operacoesBLL.ObterProximoIdMovimentoSeTipo83(processo.IdPJ, idMovimento);

                        string pattern = @"(anexo|segue|juntada|petição)";
                        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                        Match match = regex.Match(texto);

                        int proxID = 0;
                        if (proxID > 0 && match.Success)
                        {
                            //AnexarConteudoPeticaoInicial(string idPeticaoInicial, string idProximaPeticao, string diretorio)
                            AnexarConteudoPeticaoInicial(idMovimento, proxID.ToString(), diretorio);
                            idPeticaoInicial = idMovimento;
                            ignore = false;
                        }
                        else
                        {
                            ignore = true;
                        }
                    }
                }
                if (!ignore)
                {
                    ignore = false;
                    texto = File.ReadAllText(arquivo, Encoding.UTF8);


                    if(texto.Length > 128)
                    {
                        int maximoSuperposicao = 256;
                        int limiteSaldo = 350;

                        List<string> partes = OrganizaPecas.DividirTextoComSuperposicaoCaracteres(texto, tamanhoMaximo, maximoSuperposicao, limiteSaldo);
                        //
                        //     string texto, int maximoCaracteres, int nCaracteresSuperposicao, int limiteFinal = 350
                        var atoProcessual = todosOsAtos.Find(a => a.IdMovimento == idMovimento);
                        if (atoProcessual == null)
                        {
                            Debug.WriteLine($"não encontrei resumo para idMovimento: {idMovimento}");
                            continue;
                        }
                        else
                        {
                            if (atoProcessual == null)
                            {
                                Debug.WriteLine($"não encontrei resumo para idMovimento: {idMovimento}");
                                continue;
                            }
                            else
                            {
                                var tipo = tiposDeAtos.FirstOrDefault(a => a.Id_Tipo_Ato_processual.ToString() == atoProcessual.Tipo.ToString());
                                //tipo = tiposDeAtos.FirstOrDefault(a => a.Id_Tipo_Ato_processual == atoProcessual.Tipo);

                                //Corrigir isso
                                // Se encontrou o tipo, atribui a descrição ao tipoAto do atoProcessual
                                if (tipo != null)
                                {
                                    atoProcessual.Tipo = tipo.Tipo;
                                }
                                else
                                {
                                    // Se não encontrou, define um valor padrão, por exemplo, "genérico"
                                    atoProcessual.Tipo = "genérico";
                                }
                            }


                            string numeroProcesso = Path.GetDirectoryName(diretorio);
                            numeroProcesso = Path.GetDirectoryName(numeroProcesso);
                            numeroProcesso = Path.GetFileName(numeroProcesso);

                            for (int i = 0; i < partes.Count; i++)
                            {

                                // Check if the last portion has less than 10 lines
                                if (i == partes.Count - 1 && partes[i].Split('\n').Length < 10 && i > 0)
                                {
                                    // Append this portion to the previous file
                                    string previousFile = Path.Combine(destinationDirectory, $"{nomeArquivo}[{i:000}].Json");
                                    string parteFaltando = partes[i];

                                    // Cria um novo objeto Linha com a string parteFaltando
                                    Linha novaLinha = new Linha { linha = parteFaltando };

                                    AppendToJsonFile(previousFile, novaLinha);
                                    Console.WriteLine($" Gravado: {iaFile} !");
                                    continue;
                                }

                                if (!Directory.Exists(destinationDirectory))
                                {
                                    Directory.CreateDirectory(destinationDirectory);
                                }

                                int tamanhoTotal = 0;
                                // Construct the new file name, using a format of "name001", "name002", etc.
                                string novoArquivo = Path.Combine(destinationDirectory, $"{nomeArquivo}[{i + 1:000}].json");

                                // Here we split the text by '\n' to create lines based on line breaks.
                                List<string> lines = partes[i].Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(line => line.Trim())
                                      .ToList();

                                List<dynamic> linhas = lines.Select(line => new { linha = line } as dynamic).ToList();

                                int totalPalavras = linhas.Sum(linha => linha.linha.Split(' ').Length);

                                int tamanho = totalPalavras * 2;
                                string modelo = "gpt-3.5-turbo16k";

                                var jsonData = new
                                {
                                    tpo_ato = atoProcessual.TipoAto,
                                    nr_proc = numeroProcesso,
                                    id = idMovimento,
                                    model = modelo,
                                    linhas = linhas
                                };

                                // Then serialize jsonData as before
                                string jsonString = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                                File.WriteAllText(novoArquivo, jsonString, Encoding.UTF8);

                                Debug.WriteLine($" Gravado: {iaFile} de {arquivos.Length}!");
                            }
                        }
                    }
                    
                }
            }
        }

        static int ExtractId(string arquivo)
        {
            string nome = Path.GetFileNameWithoutExtension(arquivo);
            if (nome.StartsWith("P") && int.TryParse(nome.Substring(1), out int id))
            {
                return id;
            }
            return -1; // Retornar -1 para nomes que não seguem o padrão esperado
        }

        public void AnexarConteudoPeticaoInicial(string idPeticaoInicial, string idProximaPeticao, string diretorio)
        {
            // Construindo os caminhos dos arquivos com a nova convenção de nomenclatura
            string arquivoPeticaoInicial = Path.Combine(diretorio, $"P{idPeticaoInicial}.txt");
            string arquivoProximaPeticao = Path.Combine(diretorio, $"P{idProximaPeticao}.txt");

            // Verificando se ambos os arquivos existem
            if (File.Exists(arquivoPeticaoInicial) && File.Exists(arquivoProximaPeticao))
            {
                // Ler o conteúdo da próxima petição
                string conteudoProximaPeticao = File.ReadAllText(arquivoProximaPeticao);

                // Verificar se o conteúdo da próxima petição é suficientemente grande
                if (conteudoProximaPeticao.Length > 100)
                {
                    // Anexar o conteúdo da próxima petição ao arquivo da petição inicial
                    File.AppendAllText(arquivoPeticaoInicial, conteudoProximaPeticao);

                    // Marcar a próxima petição
                    string marca = $"Petição inicial juntada no ID:{idPeticaoInicial}";
                    File.WriteAllText(arquivoProximaPeticao, marca);

                    Debug.WriteLine($"Conteúdo da petição {idProximaPeticao} anexado e marcado no arquivo {idPeticaoInicial}.");
                }
                else
                {
                    Debug.WriteLine($"A próxima petição {idProximaPeticao} não tem conteúdo suficiente para ser anexada.");
                }
            }
            else
            {
                Console.WriteLine($"Um ou ambos os arquivos das petições não foram encontrados.");
            }
        }




        static string ExtractNumericPart(string fileName)
        {
            // Define a expressão regular para encontrar a parte numérica
            Regex regex = new Regex(@"\d+");

            // Encontra a primeira ocorrência da parte numérica no nome do arquivo
            Match match = regex.Match(fileName);

            if (match.Success)
            {
                return match.Value;
            }

            // Caso não encontre nenhuma parte numérica, você pode retornar um valor padrão ou lançar uma exceção, conforme necessário.
            return string.Empty;
        }

        public static async Task AppendToJsonFile(string filepath, Linha newContent)
        {
            string fileContent = File.ReadAllText(filepath);

            // Desserialização
            JsonData jsonData = JsonConvert.DeserializeObject<JsonData>(fileContent);

            // Adiciona novo conteúdo
            jsonData.Linhas.Add(newContent); // Aqui corrigimos para usar 'Linhas' em vez de 'linhas'

            // Serialização
            string updatedContent = JsonConvert.SerializeObject(jsonData);

            // Escreve de volta ao arquivo
            await File.WriteAllTextAsync(filepath, updatedContent);
        }




        /*
        public static void RegularizaArquivosPorTamanho(string diretorio,string destinationDirectory, int tamanhoMaximo)
        {
            string[] arquivos = Directory.GetFiles(diretorio);

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

                    if (!Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    // Construct the new file name, using a format of "name001", "name002", etc.
                    string novoArquivo = Path.Combine(destinationDirectory, $"{nomeArquivo}({i + 1:000}).txt");

                    // Escreve o arquivo
                    File.WriteAllText(novoArquivo, partes[i]);
                    Debug.WriteLine($" Gravado: {iaFile} de {arquivos.Count()}!");
                }
            }
        }
        */
        public static void ProcessarArquivosReg(string diretorio)
        {
            string subDiretorio = Path.Combine(Path.GetDirectoryName(diretorio), "Fase_III");

            Directory.CreateDirectory(subDiretorio);

            // get list of unique base file names
            string[] arquivos = Directory.GetFiles(diretorio, "*.rtf");
            var baseFileNames = new HashSet<string>();
            int count = 0;
            foreach (string arquivo in arquivos)
            {

                string nomeArquivo = Path.GetFileNameWithoutExtension(arquivo);
                nomeArquivo = nomeArquivo.Substring(0, nomeArquivo.LastIndexOf('_')); // remove the .reg and the part number

                baseFileNames.Add(nomeArquivo);
            }

            // process each unique base file name
            foreach (string baseFileName in baseFileNames)
            {
                var partFiles = Directory.GetFiles(diretorio, $"{baseFileName}*.rtf");

                StringBuilder sb = new StringBuilder();

                // sort partFiles based on the numeric suffix to ensure they are processed in the correct order
                Array.Sort(partFiles, (x, y) => StringComparer.Create(new CultureInfo("pt-BR"), true).Compare(x, y));

                foreach (string partFile in partFiles)
                {
                    string texto = File.ReadAllText(partFile);
                    sb.Append(texto);
                }

                // Expressão Regular
                string pattern = @"P\d+";

                // Crie uma instância Regex com o padrão
                Regex regex = new Regex(pattern);

                // Busque as correspondências no arquivo
                Match match = regex.Match(baseFileName);

                string novoArquivo = Path.Combine(subDiretorio, $"{match}_R.txt");

                // Escreve o arquivo
                File.WriteAllText(novoArquivo, sb.ToString());
                Debug.WriteLine($" Processado: {novoArquivo} !");
                count++;
                Debug.WriteLine($"Processando {count} de {arquivos.Count()}");
            }
            //converteParaRTF(subDiretorio, Path.Combine(Path.GetDirectoryName(diretorio), "Fase_IV"));
        }

        public static void converteParaRTF(string sourceDirectory, string destinationDirectory)
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            // Loop over the .rtf files in the source directory
            foreach (string filePath in Directory.EnumerateFiles(sourceDirectory, "*.rtf"))
            {
                // Load the file content
                string fileContent = File.ReadAllText(filePath, Encoding.UTF8); // lendo com a codificação UTF-8

                // Split the content into lines
                string[] lines = fileContent.Split('\n');

                // Process each line
                for (int i = 0; i < lines.Length; i++)
                {
                    // Inserindo uma linha entre parágrafos
                    lines[i] = "\\pard\\sa200\\f1\\fs26 " + lines[i] + "\\par";  // O comando \pard redefine para os parâmetros de parágrafo padrão. 
                                                                                 // O comando \sa200 adiciona espaço após o parágrafo.
                                                                                 // O comando \f1 define a fonte (supondo que Biome Light seja a fonte 1).
                                                                                 // O comando \fs26 define o tamanho da fonte como 13 (o tamanho em RTF é duas vezes o tamanho desejado em pontos).
                }

                // Join the lines back together
                string newContent = string.Join('\n', lines);

                // Add the UTF-8 directive to the start
                newContent = "{\\rtfi\\ansi\\ansicpg65001\\deff0{\\fonttbl{\\f0\\fnil\\fcharset0 Arial;}{\\f1\\fnil\\fcharset0 Biome Light;}}\n" + newContent + "}";
                // O comando \rtfi começa o arquivo RTF.

                string destFile = Path.Combine(destinationDirectory, Path.GetFileName(filePath));

                // Overwrite the original file with the new content
                File.WriteAllText(destFile, newContent, Encoding.UTF8); // escrevendo com a codificação UTF-8
            }
        }

        //public static List<string> DividirTextoComSuperposicao(string texto, int maximoPalavras, int palavrasSuperposicao, int limiteFinal = 350)
        //{
        //    List<string> partes = new List<string>();
        //    string[] paragrafos = texto.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        //    List<string> todasPalavras = new List<string>();


        //    Debug.WriteLine(paragrafos.Length);

        //    foreach (string paragrafo in paragrafos)
        //    {
        //        todasPalavras.AddRange(paragrafo.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        //        todasPalavras.Add("\n"); // Marcador de fim de parágrafo
        //    }


        //    Debug.Write(todasPalavras.ToString());

        //    int inicioAtual = 0;
        //    while (inicioAtual < todasPalavras.Count)
        //    {
        //        int fim = Math.Min(inicioAtual + maximoPalavras, todasPalavras.Count);

        //        // Verifica se o restante do texto é menor do que o limiteFinal
        //        if (todasPalavras.Count - inicioAtual <= limiteFinal)
        //        {
        //            fim = todasPalavras.Count; // Pega todo o restante do texto
        //        }

        //        string parte = String.Join(" ", todasPalavras.GetRange(inicioAtual, fim - inicioAtual));
        //        partes.Add(parte.Trim());

        //        if (fim == todasPalavras.Count)
        //        {
        //            break; // Todo o texto foi processado
        //        }

        //        // Calcula o início da próxima parte com superposição
        //        int inicioProximaParte = fim - palavrasSuperposicao;
        //        while (inicioProximaParte > 0 && todasPalavras[inicioProximaParte] != "\n")
        //        {
        //            inicioProximaParte--; // Retrocede até o início do parágrafo
        //        }

        //        inicioAtual = inicioProximaParte;
        //    }

        //    return partes;
        //}
        public static List<string> DividirTextoComSuperposicaoCaracteres(string texto, int maximoCaracteres, int nCaracteresSuperposicao, int limiteFinal)
        {
            List<string> partes = new List<string>();

            int inicioAtual = 0;
            while (inicioAtual < texto.Length)
            {
                int fim = inicioAtual + maximoCaracteres;
                fim = Math.Min(fim, texto.Length);

                // Ajuste para encontrar um final de sentença ou final de linha
                fim = EncontrarPontoDeCorte(texto, inicioAtual, fim);

                string parte = texto.Substring(inicioAtual, fim - inicioAtual);
                partes.Add(parte);

                if (fim == texto.Length)
                {
                    break; // Todo o texto foi processado
                }

                // Prepara o início da próxima parte considerando a superposição
                inicioAtual = fim;
                inicioAtual -= Math.Min(nCaracteresSuperposicao, texto.Length - inicioAtual);
            }

            return partes;
        }

        private static int EncontrarPontoDeCorte(string texto, int inicio, int fim)
        {
            if (fim >= texto.Length)
            {
                return fim;
            }

            // Retroceder para o último ponto final, ponto de exclamação, ponto de interrogação ou final de linha
            for (int i = fim; i > inicio; i--)
            {
                if (texto[i] == '.' || texto[i] == '!' || texto[i] == '?' || texto[i] == '\n')
                {
                    return i + 1; // Incluir o sinal de pontuação ou a quebra de linha
                }
            }

            // Se não encontrar, retorna o fim original
            return fim;
        }



        public static List<string> DividirTexto(string texto, int maximoPalavras)
        {
            // Separar o texto em frases
            string[] frases = texto.Split(new[] { '.', '?', '!', ';' }, StringSplitOptions.RemoveEmptyEntries);

            List<string> partes = new List<string>();
            StringBuilder parteAtual = new StringBuilder();

            foreach (var frase in frases)
            {
                // Contar as palavras na frase e na parte atual
                int numeroPalavrasFrase = frase.Split().Length;
                int numeroPalavrasParteAtual = parteAtual.ToString().Split().Length;

                // Adicionar a frase à parte atual, se houver espaço suficiente
                if (numeroPalavrasParteAtual + numeroPalavrasFrase <= maximoPalavras)
                {
                    parteAtual.Append(frase.Trim() + " ");  // Adicione um espaço ao final
                }
                else
                {
                    // Se a frase não couber, adicionar a parte atual à lista de partes
                    partes.Add(parteAtual.ToString().Trim());

                    // Começar uma nova parte com a frase atual
                    parteAtual.Clear();
                    parteAtual.Append(frase.Trim() + " ");  // Adicione um espaço ao final
                }
            }

            // Adicionar a última parte à lista de partes
            if (parteAtual.Length > 0)
            {
                partes.Add(parteAtual.ToString().Trim());
            }

            return partes;
        }





    }
}
