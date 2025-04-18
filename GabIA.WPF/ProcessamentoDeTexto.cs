using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel;
using iText.Kernel.Utils;
using iText.Kernel.Pdf;
using System.Diagnostics;
using Tesseract;
using System.Threading.Tasks;
using System.Xml.Linq;
using GabIA.BLL;
using GabIA.ENT;
using GabIA.DAL;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Printing;
using System.Threading;
using static GabIA.WPF.ProcessamentoDeTexto;

namespace GabIA.WPF
{
    public class ProcessamentoDeTexto
    {
        List<Tuple<string, int, int>> fileToIdAndPage = new List<Tuple<string, int, int>>();


        public ProcessamentoDeTexto()
        {
        }


        public class ElementosDaAcaoCivel
        {
            public string Juizo { get; set; }

            public List<Parte> Partes { get; set; }

            // Restante das propriedades...
        }

        public class Parte
        {
            public string Nomes { get; set; }

            public string EstadoCivil { get; set; }

            public bool UniaoEstavel { get; set; }

            public string Profissao { get; set; }

            public string CPF { get; set; }

            public string Email { get; set; }

            public string Domicilio { get; set; }
        }

        public class JsonRoot
        {
            public Response response { get; set; }
            public Metadata metadata { get; set; }
        }

        public class ExternalJsonRoot
        {
            [JsonProperty("response")]
            public string ResponseJson { get; set; }

            [JsonProperty("metadata")]
            public Metadata Metadata { get; set; }
        }
        public class Response
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("created")]
            public long Created { get; set; }

            [JsonProperty("model")]
            public string Model { get; set; }

            [JsonProperty("choices")]
            public Choice[] Choices { get; set; }

            [JsonProperty("usage")]
            public Usage Usage { get; set; }

            [JsonProperty("system_fingerprint")]
            public string SystemFingerprint { get; set; }

            [JsonProperty("object")]
            public string ObjectType { get; set; }
        }


        public class Choice
        {
            public int Index { get; set; }
            public Message Message { get; set; }
            public string FinishReason { get; set; }
        }

        public class Message
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }

        public class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        public class Metadata
        {
            public string TpoAto { get; set; }
            public string Id { get; set; }
            public string NrProc { get; set; }

            public string DataInclusao { get; set; }
            public string Tipo { get; set; }
            public string Resumo { get; set; }
            public string IdProcesso { get; set; }
        }



        public class CabecalhoJson
        {
            public JsonRoot Root { get; set; }
            public List<LinhaJson> Linhas { get; set; }
            // Outras propriedades conforme necessário
        }

        public class LinhaJson
        {
            public string Linha { get; set; }
            public DateTime? DataInclusao { get; set; }
            public string Tipo { get; set; }
            public string Resumo { get; set; }
            public int? IdProcesso { get; set; }
            // Outras propriedades conforme necessário
        }




        public void MoveConteudoIDCorreto(string diretorio)
        {
            if (Directory.Exists(diretorio))
            {
                var arquivos = Directory.GetFiles(diretorio, "*.txt")
                    .Select(Path.GetFileNameWithoutExtension)
                    .OrderBy(x => int.Parse(Regex.Match(x, @"\d+").Value))
                    .ToList();

                for (int i = 0; i < arquivos.Count; i++)
                {
                    string caminhoArquivoAtual = Path.Combine(diretorio, arquivos[i] + ".txt");
                    string texto = File.ReadAllText(caminhoArquivoAtual);
                    texto = texto.ToLower();
                    // Se o arquivo já contém "vide ID:<id>", então o processo é pulado para o próximo arquivo
                    if (Regex.IsMatch(texto, @"vide id:p\d+")) continue;

                    if ((texto.Length < 24 || (texto.Length < 64 && (texto.Contains("pdf") || (texto.Contains("petição") || (texto.Contains("documentos") || texto.Contains("anexo")))) && i < arquivos.Count - 1)))
                    {
                        string caminhoArquivoProximo = Path.Combine(diretorio, arquivos[i + 1] + ".txt");
                        string textoProximo = File.ReadAllText(caminhoArquivoProximo);

                        // Verificando se o próximo arquivo também não foi substituído antes
                        if (!Regex.IsMatch(textoProximo, @"vide ID:\d+"))
                        {
                            // Substitui o conteúdo do arquivo atual pelo conteúdo do próximo arquivo
                            File.WriteAllText(caminhoArquivoAtual, textoProximo);

                            // Substitui o conteúdo do próximo arquivo pela string "vide ID:<indice>"
                            File.WriteAllText(caminhoArquivoProximo, "vide ID:" + arquivos[i]);

                            // Caminho do arquivo PDF correspondente ao arquivo TXT atual
                            string caminhoPdfAtual = Path.Combine(diretorio, "pecasPDF", arquivos[i] + ".pdf");

                            // Verifique se o arquivo PDF existe
                            if (File.Exists(caminhoPdfAtual))
                            {
                                // Caminho do arquivo PDF que corresponderá ao próximo arquivo TXT
                                string caminhoPdfProximo = Path.Combine(diretorio, "pecasPDF", arquivos[i + 1] + ".pdf");

                                // Verifique se o próximo arquivo PDF não existe para evitar a substituição
                                if (!File.Exists(caminhoPdfProximo))
                                {
                                    // Mover o arquivo PDF para corresponder ao próximo arquivo TXT
                                    File.Move(caminhoPdfAtual, caminhoPdfProximo);
                                }
                            }

                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Diretório não encontrado: " + diretorio);
            }
        }

        public string ExtrairConteudoJson(string jsonText)
        {
            try
            {
                var jsonRoot = JsonConvert.DeserializeObject<ExternalJsonRoot>(jsonText);

                if (jsonRoot?.ResponseJson == null)
                {
                    throw new InvalidOperationException("JSON inválido ou ausente.");
                }

                var jsonResponse = JsonConvert.DeserializeObject<Response>(jsonRoot.ResponseJson);

                if (jsonResponse?.Choices == null || jsonResponse.Choices.Length == 0)
                {
                    throw new InvalidOperationException("Nenhum conteúdo encontrado em 'choices'.");
                }

                return jsonResponse.Choices[0]?.Message?.Content ?? string.Empty;
            }
            catch (JsonException e)
            {
                Console.WriteLine("Erro ao processar JSON: " + e.Message);
                return string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro: " + e.Message);
                return string.Empty;
            }
        }


        public void ProcessDirtXml(string inputDirectory, string outputDirectory, string defaultName, string tipo)
        {
            // Preparando o nome do arquivo de saída
            var outputFileName = Path.Combine(outputDirectory, defaultName, "Doc", $"{defaultName}.html");

            // Criando o documento HTML e o elemento root
            var htmlDoc = new StringBuilder();

            // Adicionando o cabeçalho e estilos
            htmlDoc.AppendLine("<!DOCTYPE html>");
            htmlDoc.AppendLine("<html>");
            htmlDoc.AppendLine("<head>");
            htmlDoc.AppendLine("<style>");
            htmlDoc.AppendLine("body { font-family: Arial; color: black; font-size: 16px; }");
            htmlDoc.AppendLine("</style>");
            htmlDoc.AppendLine("</head>");
            htmlDoc.AppendLine("<body>");

            // Carregando os atos do banco de dados
            OperacoesBLL operacoes = new OperacoesBLL();
            List<AtoProcessualENT> atosDoProcesso = operacoes.BuscarTodosOsAtosDoProcesso(defaultName);

            var files = new DirectoryInfo(inputDirectory).GetFiles();

            // Lista predefinida de tipos de atos para o relatório "pf"
            List<string> tiposDeAtosPf = new List<string> { "inicial", "emenda", "decisão", "despacho", "diligência", "mandado", "parecer", "contestação", "replica", "saneamento", "alegações" };

            // Converte a lista para minúsculas para a comparação ser case-insensitive
            List<string> tiposDeAtosPfLowerCase = tiposDeAtosPf.Select(t => t.ToLower()).ToList();

            // Lista para armazenar os atos do relatório "pf"
            List<AtoProcessualENT> atosDoRelatorioPf = new List<AtoProcessualENT>();

            // Para cada ato processual, verifica se o tipo de ato faz parte da lista predefinida
            foreach (var ato in atosDoProcesso)
            {
                if (ato.TipoAto != null && tiposDeAtosPfLowerCase.Contains(ato.TipoAto.ToLower()))
                {
                    atosDoRelatorioPf.Add(ato);
                }
            }

            // Usando a função OrderBy para ordenar os arquivos baseado no ID extraído do nome do arquivo
            var orderedFiles = files.OrderBy(file =>
            {
                var regex = new Regex(@"P(\d+)_");
                var match = regex.Match(file.Name);
                return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
            }).ToList();

            // Iterando sobre os arquivos ordenados
            foreach (var file in orderedFiles)
            {
                // Usando uma expressão regular para encontrar o ID
                var regex = new Regex(@"P(\d+)_");
                var match = regex.Match(file.Name);
                if (match.Success)
                {
                    var id = int.Parse(match.Groups[1].Value);

                    // Obtendo o AtoProcessual correspondente
                    var ato = atosDoProcesso.FirstOrDefault(a => int.Parse(a.IdMovimento) == id);
                    if (ato == null)
                        continue;

                    // Lendo o conteúdo do arquivo
                    var content = File.ReadAllText(file.FullName);

                    // Se o tamanho do conteúdo for menor que 64 caracteres, ignora
                    if (content.Length < 64)
                        continue;

                    // Preparando o hyperlink
                    var hyperlink = $"file:///d:/PJe/Processos/{defaultName}/PecasProcessuais/PecasPDF/P{id}.pdf";

                    // Adicionando o hyperlink, o resumo do ato, a data de cadastramento e o conteúdo ao documento HTML
                    htmlDoc.AppendLine($"<p><a href=\"{hyperlink}\">{id} - Resumo: {ato.Resumo} - Data da Inclusão: {ato.DataInclusao}</a></p>");
                    htmlDoc.AppendLine($"<p>{content}</p>");

                    // Adicionando espaçamento
                    htmlDoc.AppendLine("<br/><br/><br/>");
                }
            }

            // Finalizando o documento HTML
            htmlDoc.AppendLine("</body>");
            htmlDoc.AppendLine("</html>");

            // Salvando o documento HTML
            File.WriteAllText(outputFileName, htmlDoc.ToString(), Encoding.UTF8);

            // Salvando o documento HTML no formato de texto UTF-8
            using (var writer = File.CreateText(Path.Combine(Path.GetDirectoryName(outputFileName), Path.GetFileNameWithoutExtension(outputFileName) + ".txt")))
            {
                writer.Write(htmlDoc.ToString());
            }
        }


        public string SubstituirSequenciaNumerica(string texto)
        {
            // Defina as expressões regulares (regex) para cada tipo
            string patternNProc = @"\d{7}-\d{2}.\d{4}.\d.\d{2}.\d{4}"; // ajuste conforme necessário
            string patternCpf = @"\d{3}.\d{3}.\d{3}-\d{2}"; // ajuste conforme necessário
            string patternCep = @"\d{5}-\d{3}"; // ajuste conforme necessário
            string patternIdt = @"\d{10}"; // ajuste conforme necessário
            string patternTelefone = @"\(\d{2}\) \d{4}-\d{4}"; // ajuste conforme necessário
            string ident = @"\d{1}.\d{3}.\d{3}";
            // Defina as strings de substituição para cada tipo
            string replacementNProc = "0000000-00.0000.0.00.0000";
            string replacementCpf = "000.000.000-00";
            string replacementCep = "00000-000";
            string replacementIdt = "0000000000";
            string replacementTelefone = "(00) 0000-0000";
            string replacementID = "0.000.000";

            // Substitua as ocorrências de cada padrão pela string de substituição correspondente
            texto = Regex.Replace(texto, patternNProc, replacementNProc);
            texto = Regex.Replace(texto, patternCpf, replacementCpf);
            texto = Regex.Replace(texto, patternCep, replacementCep);
            texto = Regex.Replace(texto, patternIdt, replacementIdt);
            texto = Regex.Replace(texto, patternTelefone, replacementTelefone);
            texto = Regex.Replace(texto, ident, replacementID);
            return texto;
        }


        public void ProcessaArquivos(string directoryPath, string pdfDirectoryPath)
        {
            var textFiles = Directory.GetFiles(directoryPath, "*.txt").OrderBy(f => f).ToList();

            var regex = new Regex(@"Número processo PJe:\s*(?<pjeNumber>\d+-\d+\.\d+\.\d+\.\d+\.\d+)\s*ID\.\s*(?<id>\d+)\s*Pág\.\s*(?<page>\d+)", RegexOptions.Multiline);

            string dirPDF = Path.Combine(Path.GetDirectoryName(directoryPath), "PaginasPDF");
            string dirPNG = Path.Combine(Path.GetDirectoryName(directoryPath), "PNG");

            // Conta o número de arquivos em cada diretório
            int countPDF = Directory.GetFiles(dirPDF, "*.pdf").Length;
            int countPNG = Directory.GetFiles(dirPNG, "*.png").Length;

            // Executa PerformOcrOnImages se a contagem for diferente
            if (countPDF != countPNG)
            {
                PerformOcrOnImages(pdfDirectoryPath, dirPDF);
            }
            else
            {
                return;
            }

            int currentId = -1;
            string combinedText = "";
            int contPageID = 0;
            foreach (var file in textFiles)
            {

                string ocrTXT = Path.GetDirectoryName(file);
                ocrTXT = Path.GetDirectoryName(ocrTXT);
                ocrTXT = Path.Combine(ocrTXT, "OCR", Path.GetFileNameWithoutExtension(file) + "OCR.txt");

                var fileContent = ObterTextoMaior(file, ocrTXT);

                var match = regex.Match(fileContent);
                Debug.WriteLine(fileContent.Length);
                if (match.Success)
                {
                    var regexII = new Regex(@"ID\.\s*(?<id>\d+)\s*Pág\.\s*(?<page>\d+)", RegexOptions.Singleline);

                    match = regexII.Match(fileContent);
                    
                    if (match.Success) { } else { }

                    int id = int.Parse(match.Groups["id"].Value);
                    int page = int.Parse(match.Groups["page"].Value);
                    string filename = Path.GetFileNameWithoutExtension(file);

                    // Adicione o nome do arquivo (sem a extensão .txt) à lista de páginas para a peça atual.
                    string nomePdfCorrespondente = Path.GetFileNameWithoutExtension(file);

                    Debug.WriteLine(id);

                    string eventualOCR = nomePdfCorrespondente + "OCR.txt";
                    nomePdfCorrespondente = nomePdfCorrespondente + ".Pdf";

                    fileToIdAndPage.Add(Tuple.Create(nomePdfCorrespondente, id, page));

                    if (currentId == -1)
                    {
                        currentId = id;
                    }

                    bool fizOCR = false;
                    var cleanedContent = regex.Replace(fileContent, "").Trim();

                    if (currentId == id)
                    {
                        combinedText += cleanedContent + Environment.NewLine;
                        contPageID++;
                    }
                    else
                    {
                        if(!fizOCR)
                        {

                        } 
                        string combinedDir = Path.GetDirectoryName(directoryPath);
                        combinedDir =  Path.Combine(combinedDir, "PecasTxt");
                        SaveCombinedFile(currentId, combinedText, combinedDir);
                        combinedText = regex.Replace(fileContent, "").Trim() + Environment.NewLine;
                        currentId = id;
                    }
                }
                else
                {
                    if(fileContent.Length == 0)
                    {
                        Debug.WriteLine("arquivo vazio!");

                    }
                    else
                    {
                        Debug.WriteLine("erro match ?");

                    }
                }
            }

            if (!string.IsNullOrEmpty(combinedText))
            {
                string combinedDir = Path.GetDirectoryName(directoryPath);
                //combinedDir = Path.GetDirectoryName(combinedDir);
                combinedDir = Path.Combine(combinedDir, "PecasTxt");

                SaveCombinedFile(currentId, combinedText, combinedDir);
            }

            string dirPecasPDF = Path.GetDirectoryName(pdfDirectoryPath);
            dirPecasPDF = Path.Combine(dirPecasPDF, "PecasPDF");
            MergePdfFiles(dirPecasPDF, fileToIdAndPage);
        }

        static string ObterTextoMaior(string arquivo1, string arquivo2)
        {
            try
            {
                string texto1 = File.ReadAllText(arquivo1);
                string texto2 = File.ReadAllText(arquivo2);

                if (texto1.Length >= texto2.Length)
                {
                    return texto1;
                }
                else
                {
                    return texto2;
                }
            }
            catch (Exception ex)
            {
                // Tratamento de exceção
                Console.WriteLine("Ocorreu um erro ao ler os arquivos: " + ex.Message);
                return string.Empty;
            }
        }

        // Extrai o código de processamento para um método separado.
        public void PerformOcrOnImages(string directoryPath, string pdfDirectoryPath)
        {
            Stopwatch stopwatch = Stopwatch.StartNew(); // Inicia o cronômetro

            var regex = new Regex(@"Número processo PJe:\s*(?<pjeNumber>\d+-\d+\.\d+\.\d+\.\d+\.\d+)\s*ID\.\s*(?<id>\d+)\s*Pág\.\s*(?<page>\d+)", RegexOptions.Singleline);

            var textFiles = Directory.GetFiles(directoryPath, "*.pdf").OrderBy(f => f).ToList();
            foreach (var file in textFiles)
            {
                try
                {
                    var fileContent = File.ReadAllText(file, Encoding.UTF8);
                    var cleanedContent = regex.Replace(fileContent, "").Trim();
                    if (cleanedContent.Length > 1)
                    {
                        string dirPdf = Path.GetDirectoryName(file);
                        string dirPNG = Path.Combine(Path.GetDirectoryName(dirPdf), "PNG");
                        Debug.WriteLine(Path.GetFullPath(file));
                        string nomePdfCorrespondente = Path.GetFileNameWithoutExtension(file) + ".Pdf";
                        dirPdf = Path.GetDirectoryName(dirPdf);
                        dirPdf = Path.Combine(dirPdf, "PaginasPDF", nomePdfCorrespondente);

                        if (File.Exists(dirPdf))
                        {
                            string pdfPath = Path.Combine(pdfDirectoryPath, nomePdfCorrespondente);
                            PDFToPNGConverter convert = new PDFToPNGConverter();

                            CancellationTokenSource cts = new CancellationTokenSource();
                            Task conversionTask = Task.Run(() =>
                            {
                                convert.ConvertPdfPageToPng(pdfPath, dirPNG, 200, 600, 1);
                            }, cts.Token);

                            bool isConversionCompleted = conversionTask.Wait(TimeSpan.FromSeconds(10));
                            if (!isConversionCompleted)
                            {
                                throw new TimeoutException("A conversão para PNG excedeu o tempo limite.");
                            }

                            string pngPath = Path.Combine(dirPNG, Path.GetFileNameWithoutExtension(pdfPath) + "L.png");
                            if (File.Exists(pngPath))
                            {
                                string eventualOCR = Path.GetFileNameWithoutExtension(file) + "OCR.txt";
                                string ocrFilePath = Path.GetDirectoryName(file);
                                ocrFilePath = Path.GetDirectoryName(ocrFilePath);
                                ocrFilePath = Path.Combine(ocrFilePath, "OCR", eventualOCR);

                                if (!File.Exists(ocrFilePath))
                                {
                                    Task ocrTask = Task.Run(() =>
                                    {
                                        string ocrContent = PerformOcr(pngPath);
                                        if (ocrContent.Length > 20)
                                        {
                                            File.WriteAllText(ocrFilePath, ocrContent);
                                        } else
                                        {
                                            // Chama o script Python aqui
                                            string pythonScriptPath = "d:\\PJe\\App_cpp\\ocr.py";
                                            string pythonExecutablePath = "c:\\Python311\\python.exe";
                                            string dirTXT = Path.GetDirectoryName(pngPath);
                                            dirTXT = Path.Combine(dirTXT, Path.GetFileNameWithoutExtension(pngPath) +"T.txt");

                                            string ProcName1 = "Mon" + CriaNomeUnico();

                                            var args = new List<string> {
                                                pythonExecutablePath,
                                                pythonScriptPath,
                                                pngPath,
                                                dirTXT,
                                                "d:\\PJe\\Monitor\\" + Path.GetFileNameWithoutExtension(pngPath) +"T.txt"};

                                            // Criar uma instância da classe MonitoraExcecucaoAssincrona
                                            MonitoraExcecucaoAssincrona monitor = new MonitoraExcecucaoAssincrona("caminho", "arquivo", 10000);


                                            var process = new Process()
                                            {
                                                StartInfo = new ProcessStartInfo
                                                {
                                                    FileName = pythonExecutablePath,
                                                    Arguments = $"{pythonScriptPath} {pngPath} {"d:\\PJe\\Dados"} {"d:\\PJe\\Dados"}",
                                                    RedirectStandardOutput = true,
                                                    UseShellExecute = false,
                                                    CreateNoWindow = true
                                                }
                                            };

                                            process.Start();
                                            string output = process.StandardOutput.ReadToEnd();
                                            process.WaitForExit();

                                            // Você pode processar a saída do Python aqui se necessário

                                        }
                                    }, cts.Token);

                                    bool isOcrCompleted = ocrTask.Wait(TimeSpan.FromSeconds(10));
                                    if (!isOcrCompleted)
                                    {
                                        throw new TimeoutException("O processo OCR excedeu o tempo limite.");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText("path_do_seu_arquivo_log.txt", $"Erro no arquivo {file}: {ex.Message}\n");
                    continue;
                }
            }

            stopwatch.Stop(); // Para o cronômetro
            Debug.WriteLine($"Tempo total para executar normal: {stopwatch.Elapsed.TotalSeconds} segundos");
        }
        public static string CriaNomeUnico()
        {
            // Obtém o identicador da thread atual
            //int threadId = Thread.CurrentThread.ManagedThreadId;

            // Obtém a data e hora atuais formatadas
            //string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            // Gera um GUID
            string guid = Guid.NewGuid().ToString();

            // Combina tudo para criar um nome de arquivo único
            string fileName = $"_{guid}.mon";

            return fileName;
        }

        public string PerformOcr(string imagePath)
        {
            string tessDataPath = @"C:\Arquivos de Programas\Tesseract-OCR\tessdata"; // Caminho para o diretório tessdata
            string ocrText = "";

            // Verifica se o arquivo PNG existe e não é vazio
            if (!File.Exists(imagePath) || new FileInfo(imagePath).Length == 0)
            {
                Debug.WriteLine($"O arquivo PNG '{imagePath}' está vazio ou não existe.");
                return ocrText;
            }

            try
            {
                using (var engine = new TesseractEngine(tessDataPath, "por", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(imagePath))
                    {
                        using (var page = engine.Process(img))
                        {
                            ocrText = page.GetText();
                        }
                    }
                }
            }
            catch (System.IO.IOException ex)
            {
                // Tratamento de erro de I/O
                Debug.WriteLine($"Erro de I/O no arquivo '{imagePath}': {ex.Message}");
            }
            catch (Exception e)
            {
                // Tratamento de outros erros
                Debug.WriteLine($"Erro durante o processamento OCR do arquivo '{imagePath}': {e.Message}");
            }

            return ocrText;
        }

        public static List<string> DividirTexto(string texto, int tamanhoMaximo)
        {
            List<string> partes = new List<string>();

            // Mantenha o controle da posição atual no texto
            int posicaoAtual = 0;

            while (posicaoAtual < texto.Length)
            {
                // Encontre a última ocorrência de qualquer caracter de pontuação final antes do limite
                int ultimaOcorrenciaPontuacao = texto.Substring(posicaoAtual, Math.Min(tamanhoMaximo, texto.Length - posicaoAtual))
                                                .LastIndexOfAny(new char[] { '.', '?', '!', ':', ';' });

                // Se não encontramos um caracter de pontuação, corte no tamanho máximo
                if (ultimaOcorrenciaPontuacao == -1)
                {
                    ultimaOcorrenciaPontuacao = Math.Min(tamanhoMaximo, texto.Length - posicaoAtual);
                }
                else
                {
                    // Se encontramos um caracter de pontuação, movemos uma posição à frente para incluí-lo na parte
                    ultimaOcorrenciaPontuacao += 1;
                }

                // Corte a parte do texto
                string parte = texto.Substring(posicaoAtual, ultimaOcorrenciaPontuacao);

                // Adicione a parte à lista
                partes.Add(parte);

                // Atualize a posição atual
                posicaoAtual += ultimaOcorrenciaPontuacao;
            }

            return partes;
        }


        //public string PerformOcr(string pngFilePath)
        //{
        //    string tessDataPath = @"C:\Arquivos de Programas\Tesseract-OCR\tessdata"; // Caminho para o diretório tessdata
        //    string ocrText = "";

        //    // Verifica se o arquivo PDF não é vazio
        //    if (new FileInfo(pngFilePath).Length == 0)
        //    {
        //        Debug.WriteLine("O arquivo PDF está vazio.");
        //        return ocrText;
        //    }

        //    try
        //    {
        //        using (var engine = new TesseractEngine(tessDataPath, "por", EngineMode.Default))
        //        {
        //            using (var img = Pix.LoadFromFile(pngFilePath))
        //            {
        //                using (var page = engine.Process(img))
        //                {
        //                    ocrText = page.GetText();
        //                }
        //            }
        //        }
        //    }
        //    catch (System.IO.IOException ex)
        //    {
        //        Debug.WriteLine("An I/O error occurred during OCR processing: " + ex.Message);
        //        // Realize ações apropriadas para lidar com a exceção de E/S
        //        // Por exemplo, exiba uma mensagem de erro para o usuário ou registre o erro em um arquivo de log
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine("An error occurred during OCR processing: " + e.Message);
        //        // Realize ações apropriadas para lidar com a exceção
        //    }

        //    return ocrText;
        //}


        private void SaveCombinedFile(int id, string content, string diretorioTexto)
        {
            var outputFileName = $"P{id}.txt";
            string outPutFile = Path.Combine(diretorioTexto, outputFileName);
            //var outputFileNameB = $"P{id}_L.txt";
            //string dirModeloDeLinguagem = Path.GetDirectoryName(diretorioTexto);
            //dirModeloDeLinguagem = Path.GetDirectoryName(dirModeloDeLinguagem);
            //dirModeloDeLinguagem = Path.Combine(dirModeloDeLinguagem, "ModeloDeLinguagem");
            //string CleanOutPutFile = Path.Combine(dirModeloDeLinguagem, outputFileNameB);
            File.WriteAllText(outPutFile, content);
            //string contentLimpo = SubstituirSequenciaNumerica(content);
            //File.WriteAllText(CleanOutPutFile, RemoverTextoInutil.Instance.Remover_E_OU_Substituir_Textos(contentLimpo));
        }

        private void MergePdfFiles(string directoryPath, List<Tuple<string, int, int>> fileToIdAndPage)
        {
            int currentId = -1;
            PdfMerger merger = null;
            PdfDocument pdfDoc = null;

            foreach (var tuple in fileToIdAndPage.OrderBy(t => t.Item2).ThenBy(t => t.Item3))
            {
                string filename = tuple.Item1;
                int id = tuple.Item2;

                if (currentId != id)
                {
                    if (merger != null)
                    {
                        merger.Close();
                        pdfDoc.Close();
                    }

                    var outputFileName = $"P{id}.pdf";
                    outputFileName = Path.Combine(directoryPath, outputFileName);
                    pdfDoc = new PdfDocument(new PdfWriter(outputFileName));
                    merger = new PdfMerger(pdfDoc);

                    currentId = id;
                }

                string paginas = Path.GetDirectoryName(directoryPath);

                paginas = Path.Combine(paginas, "PaginasPDF");

                var inputFileName = Path.Combine(paginas, $"{filename}");

                using (PdfDocument sourcePdf = new PdfDocument(new PdfReader(inputFileName)))
                {
                    merger.Merge(sourcePdf, 1, sourcePdf.GetNumberOfPages());
                }
                Debug.WriteLine(filename);
            }

            if (merger != null)
            {
                merger.Close();
                pdfDoc.Close();
            }
        }

        public class OrganizaPecas
        {
            public static void RegularizaArquivos(string diretorio, int tamanhoMaximo)
            {
                string subDiretorio = Path.Combine(diretorio, "Fase_I");
                Directory.CreateDirectory(subDiretorio);

                string[] arquivos = Directory.GetFiles(diretorio);

                foreach (string arquivo in arquivos)
                {
                    string texto = File.ReadAllText(arquivo);

                    List<string> partes = DividirTexto(texto, tamanhoMaximo);

                    // determine the base file name
                    string nomeArquivo = Path.GetFileNameWithoutExtension(arquivo);

                    for (int i = 0; i < partes.Count; i++)
                    {
                        // Construct the new file name, using a format of "name001", "name002", etc.
                        string novoArquivo = Path.Combine(subDiretorio, $"{nomeArquivo}{i + 1:000}.txt");

                        // Escreve o arquivo
                        File.WriteAllText(novoArquivo, partes[i]);
                    }
                }
            }
            public static List<string> DividirTexto(string texto, int tamanhoMaximo)
            {
                // Separar o texto em frases
                string[] frases = texto.Split(new[] { '.', '?', '!', ';' }, StringSplitOptions.RemoveEmptyEntries);

                List<string> partes = new List<string>();
                StringBuilder parteAtual = new StringBuilder();

                foreach (var frase in frases)
                {
                    // Adicionar a frase à parte atual, se houver espaço suficiente
                    if (parteAtual.Length + frase.Length <= tamanhoMaximo)
                    {
                        parteAtual.Append(frase.Trim() + ". ");
                    }
                    else
                    {
                        // Se a frase não couber, adicionar a parte atual à lista de partes
                        partes.Add(parteAtual.ToString().Trim());

                        // Começar uma nova parte com a frase atual
                        parteAtual.Clear();
                        parteAtual.Append(frase.Trim() + ". ");
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


        // Método para mesclar os conteúdos de dois arquivos JSON
        public void MergeJsonContent(string inputDir, string responseDir, string outputDir)
        {
            var folder = new DirectoryInfo(outputDir);
            int fileCount = folder.Exists ? folder.GetFileSystemInfos().Length : 0;

            if (fileCount > 0) return;

            string[] inputFiles = Directory.GetFiles(inputDir, "*.json");

            foreach (string inputFile in inputFiles)
            {
                string fileName = Path.GetFileName(inputFile);
                string baseName = fileName.Split('[')[0]; // Isso retorna "P132934818"
                string finalFileName = baseName + ".json"; // Adiciona a extensão ".json" resultando em "P132934818.json"


                string responseFile = Path.Combine(responseDir, finalFileName);
                string outputFile = Path.Combine(outputDir, fileName);

                if (File.Exists(responseFile))
                {
                    try
                    {
                        JObject inputJson = JObject.Parse(File.ReadAllText(inputFile));
                        JObject responseJson = JObject.Parse(File.ReadAllText(responseFile));

                        string content = ExtractContentFromResponse(responseJson);
                        // Expressão regular para dividir o conteúdo em sentenças
                        string[] sentences = Regex.Split(content, @"(?<=[\.])\s+");

                        JArray linhasArray = new JArray();
                        foreach (string sentence in sentences)
                        {
                            if (!string.IsNullOrWhiteSpace(sentence))
                            {
                                linhasArray.Add(new JObject(new JProperty("linha", sentence)));
                            }
                        }

                        inputJson["linhas"] = linhasArray;

                        File.WriteAllText(outputFile, inputJson.ToString());
                        Debug.WriteLine($"Arquivo processado e salvo: {outputFile}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar o arquivo {fileName}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Arquivo de resposta não encontrado para {fileName}");
                }
            }
        }


        private static string ExtractContentFromResponse(JObject responseJson)
        {
            // Primeiro, obtém a string JSON do campo 'response'
            string responseString = responseJson["response"].ToString();

            // Deserializa a string em um novo objeto JObject
            JObject responseObject = JObject.Parse(responseString);

            // Agora, extrai o conteúdo do objeto deserializado
            JToken choicesToken = responseObject["choices"];
            if (choicesToken is JArray choicesArray && choicesArray.Count > 0)
            {
                JToken messageToken = choicesArray[0]["message"];
                if (messageToken is JObject messageObject)
                {
                    JToken contentToken = messageObject["content"];
                    if (contentToken is JValue contentValue)
                    {
                        return contentValue.ToString();
                    }
                }
            }

            throw new InvalidOperationException("Invalid JSON structure for response.");
        }

        public void UnificarEAtualizarArquivosJson(string directoryPath)
        {


            var jsonFiles = Directory.GetFiles(directoryPath, "P*.json");

            var groupedFiles = jsonFiles.GroupBy(
                file => Path.GetFileNameWithoutExtension(file).Split('[')[0]
            );

            foreach (var group in groupedFiles)
            {
                StringBuilder contentBuilder = new StringBuilder();
                string finalJsonFilePath = Path.Combine(directoryPath, group.Key + ".json");

                // Se o grupo contém apenas um arquivo, apenas renomeie
                if (group.Count() == 1)
                {
                    string singleFilePath = group.First();
                    if (singleFilePath != finalJsonFilePath)
                    {
                        File.Move(singleFilePath, finalJsonFilePath);
                    }
                    continue;
                }

                foreach (var filePath in group.OrderBy(f => f))
                {
                    string jsonContent = File.ReadAllText(filePath);
                    var responseObj = JObject.Parse(jsonContent)["response"].ToString();
                    var responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseObj);

                    if (responseDict["choices"] is JArray choices)
                    {
                        foreach (var choice in choices)
                        {
                            if (choice["message"]?["content"] != null)
                            {
                                contentBuilder.AppendLine(choice["message"]["content"].ToString());
                            }
                        }
                    }

                    // Excluir arquivo atual
                    File.Delete(filePath);
                }

                var newResponse = new Dictionary<string, object>
                {
                    ["response"] = new Dictionary<string, object>
                    {
                        ["choices"] = new JArray
                {
                    new JObject
                    {
                        ["message"] = new JObject
                        {
                            ["content"] = contentBuilder.ToString()
                        }
                    }
                }
                    }
                };

                string finalJson = JsonConvert.SerializeObject(newResponse, Formatting.Indented);
                File.WriteAllText(finalJsonFilePath, finalJson);
            }
        }

        ////public void ComplementarArquivosJsonEDataBase(string directoryPath, string outputFilePath, string numeroProcesso)
        ////{
        ////    OperacoesBLL operacoes = new OperacoesBLL(/* coloque aqui as dependências necessárias */);
        ////    List<AtoProcessualENT> atosDoProcesso = operacoes.BuscarTodosOsAtosDoProcesso(numeroProcesso);
        ////    List<TipoAtoProcessualENT> TiposAtoProcessuais = operacoes.BuscarTodosOsTiposDeAtosProcessuais();

        ////    var jsonFiles = Directory.GetFiles(directoryPath, "P*.json");

        ////    // Ordenar os arquivos baseado no ID numérico
        ////    var orderedFiles = jsonFiles.OrderBy(f =>
        ////    {
        ////        var fileName = Path.GetFileNameWithoutExtension(f);
        ////        var idPart = fileName.Split('[')[0].Substring(1); // Remove 'P' e divide em '[' pegando a primeira parte
        ////        return int.TryParse(idPart, out int id) ? id : int.MaxValue; // Usar int.MaxValue para arquivos não numéricos
        ////    });

        ////    using (StreamWriter sw = new StreamWriter(outputFilePath, false))
        ////    {
        ////        foreach (var filePath in orderedFiles)
        ////        {
        ////            string jsonContent = File.ReadAllText(filePath);
        ////            var jsonRoot = JsonConvert.DeserializeObject<JsonRoot>(jsonContent);

        ////            // Processamento do conteúdo de cada arquivo JSON
        ////            StringBuilder contentBuilder = new StringBuilder();

        ////            foreach (var choice in jsonRoot.response.choices)
        ////            {
        ////                if (!string.IsNullOrEmpty(choice.message?.content))
        ////                {
        ////                    contentBuilder.AppendLine(choice.message.content);
        ////                }
        ////            }

        ////            string idMovimentoDesejado = jsonRoot.response.id;
        ////            AtoProcessualENT atoEspecifico = operacoes.BuscarAtoPorIdMovimento(atosDoProcesso, idMovimentoDesejado);

        ////            if (atoEspecifico != null)
        ////            {
        ////                jsonRoot.metadata.dataInclusao = atoEspecifico.DataInclusao?.ToString();
        ////                var tipoAtoCorrespondente = TiposAtoProcessuais.FirstOrDefault(t => t.Id_Tipo_Ato_processual == atoEspecifico.Tipo);
        ////                if (tipoAtoCorrespondente != null)
        ////                {
        ////                    jsonRoot.metadata.tipo = tipoAtoCorrespondente.Tipo;
        ////                }
        ////                jsonRoot.metadata.resumo = atoEspecifico.resumo;
        ////                jsonRoot.metadata.idProcesso = atoEspecifico.IdProcesso?.ToString();
        ////            }

        ////            // Atualiza a resposta no jsonRoot
        ////            var newResponse = new Response
        ////            {
        ////                choices = new Choice[]
        ////                {
        ////            new Choice
        ////            {
        ////                message = new Message { content = contentBuilder.ToString() }
        ////            }
        ////                }
        ////            };

        ////            jsonRoot.response = newResponse;

        ////            // Serializar o jsonRoot atualizado e escrever no arquivo JSONL
        ////            string finalJson = JsonConvert.SerializeObject(jsonRoot, Formatting.Indented);
        ////            sw.WriteLine(finalJson);

        ////            // Excluir arquivo atual (se desejado)
        ////            // File.Delete(filePath);
        ////        }
        ////    }
        ////}


        public void ComplementarArquivosJsonEDataBase(string directoryPath, string outputFilePath, string numeroProcesso)
        {
            OperacoesBLL operacoes = new OperacoesBLL(/* coloque aqui as dependências necessárias */);
            List<AtoProcessualENT> atosDoProcesso = operacoes.BuscarTodosOsAtosDoProcesso(numeroProcesso);
            List<TipoAtoProcessualENT> TiposAtoProcessuais = operacoes.BuscarTodosOsTiposDeAtosProcessuais();

            var jsonFiles = Directory.GetFiles(directoryPath, "P*.json");

            var orderedFiles = jsonFiles.OrderBy(f =>
            {
                var fileName = Path.GetFileNameWithoutExtension(f);
                var idPart = fileName.Split('[')[0].Substring(1);
                return int.TryParse(idPart, out int id) ? id : int.MaxValue;
            });

            using (StreamWriter sw = new StreamWriter(outputFilePath, false))
            {
                foreach (var filePath in orderedFiles)
                {
                    string jsonContent = File.ReadAllText(filePath);
                    var externalJsonRoot = JsonConvert.DeserializeObject<ExternalJsonRoot>(jsonContent);

                    // Desserializar a string JSON interna para obter o objeto Response
                    var response = JsonConvert.DeserializeObject<Response>(externalJsonRoot.ResponseJson);

                    StringBuilder contentBuilder = new StringBuilder();
                    foreach (var choice in response.Choices)
                    {
                        if (!string.IsNullOrEmpty(choice.Message?.Content))
                        {
                            contentBuilder.AppendLine(choice.Message.Content);
                        }
                    }

                    string idMovimentoDesejado = externalJsonRoot.Metadata.Id;
                    //idMovimentoDesejado = "132934821";
                    AtoProcessualENT atoEspecifico = operacoes.BuscarAtoPorIdMovimento(atosDoProcesso, idMovimentoDesejado);

                    if (atoEspecifico != null)
                    {
                        string tipoAtoS = "";
                        externalJsonRoot.Metadata.DataInclusao = atoEspecifico.DataInclusao?.ToString();
                        foreach (var tipoAto in TiposAtoProcessuais)
                        {
                            if(tipoAto.Id_Tipo_Ato_processual.ToString() == atoEspecifico.Tipo.ToString())
                            {
                                tipoAtoS = tipoAto.Tipo;
                                break;
                            }
                            Debug.WriteLine($"Id_Tipo_Ato_processual: {tipoAto.Tipo}");

                        }


                        if (tipoAtoS != "")
                        {
                            externalJsonRoot.Metadata.Tipo = tipoAtoS;
                        }
                        externalJsonRoot.Metadata.Resumo = atoEspecifico.Resumo;
                        externalJsonRoot.Metadata.IdProcesso = atoEspecifico.IdProcesso?.ToString();
                    }

                    // Atualizar a resposta no externalJsonRoot
                    var newResponse = JsonConvert.SerializeObject(new Response
                        {
                            Choices = new Choice[]
                            {
                        new Choice
                        {
                            Message = new Message { Content = contentBuilder.ToString() }
                        }
                            }
                        });

                    externalJsonRoot.ResponseJson = newResponse;

                    // Serializar o externalJsonRoot atualizado e escrever no arquivo JSONL
                    string finalJson = JsonConvert.SerializeObject(externalJsonRoot, Formatting.Indented);
                    sw.WriteLine(finalJson);
                }
            }
        }

        public void ProcessarJsonl(string caminhoArquivo)
        {
            var caminhoArquivoSaida = Path.Combine(Path.GetDirectoryName(caminhoArquivo), "processado.jsonl");

            using (var leitor = new StreamReader(caminhoArquivo))
            using (var escritor = new StreamWriter(caminhoArquivoSaida, false))
            {
                var sb = new StringBuilder();
                string linha;
                while ((linha = leitor.ReadLine()) != null)
                {
                    sb.Append(linha);

                    try
                    {
                        // Tenta parsear o conteúdo acumulado como JSON
                        var objetoJson = JToken.Parse(sb.ToString());

                        // Escreve o JSON formatado no novo arquivo e limpa o StringBuilder
                        escritor.WriteLine(objetoJson.ToString(Formatting.None));
                        sb.Clear();
                    }
                    catch (JsonReaderException)
                    {
                        // Se o conteúdo ainda não formar um JSON válido, continua acumulando
                        // Pode adicionar lógica aqui para lidar com casos específicos de erro
                    }
                }

                // Tenta processar qualquer conteúdo remanescente após a leitura do arquivo
                if (sb.Length > 0)
                {
                    try
                    {
                        var objetoJson = JToken.Parse(sb.ToString());
                        escritor.WriteLine(objetoJson.ToString(Formatting.None));
                    }
                    catch (JsonReaderException)
                    {
                        // Lida com o caso de o conteúdo remanescente ainda ser um JSON inválido
                    }
                }
            }
        }


        public void UnificarArquivosJson(string dirOrigem, string dirCorrespondente, string dirDestino)
        {
            // Regex para identificar e agrupar arquivos
            var regex = new Regex(@"P(\d+)\[\d+\]\.json");
            var gruposArquivos = Directory.GetFiles(dirOrigem, "P*.json")
                .Where(path => regex.IsMatch(Path.GetFileName(path)))
                .GroupBy(path => regex.Match(Path.GetFileName(path)).Groups[1].Value)
                .Where(group => group.Count() > 1);

            foreach (var grupo in gruposArquivos)
            {
                var idComum = grupo.Key;
                var arquivosGrupo = grupo.OrderBy(n => n).ToList();

                // Ler e extrair o cabeçalho do primeiro arquivo
                var cabecalhoJson = JObject.Parse(File.ReadAllText(arquivosGrupo.First()));

                // Preparar a lista de linhas para o conteúdo unificado
                var linhas = new JArray();

                // Processar cada arquivo correspondente no outro diretório
                foreach (var arquivo in arquivosGrupo)
                {
                    var nomeArquivoCorrespondente = Path.GetFileName(arquivo);
                    var caminhoArquivoCorrespondente = Path.Combine(dirCorrespondente, nomeArquivoCorrespondente);

                    if (File.Exists(caminhoArquivoCorrespondente))
                    {
                        var conteudoArquivo = File.ReadAllText(caminhoArquivoCorrespondente);
                        var jsonArquivo = JObject.Parse(conteudoArquivo);
                        var conteudo = ExtractContentFromResponse(jsonArquivo);

                        // Adicionar o conteúdo ao array de linhas
                        linhas.Add(new JObject(new JProperty("linha", conteudo)));
                    }
                }

                // Substituir as linhas no json de cabeçalho e salvar
                cabecalhoJson["linhas"] = linhas;
                var caminhoDestino = Path.Combine(dirDestino, $"P{idComum}.json");
                File.WriteAllText(caminhoDestino, cabecalhoJson.ToString());
            }
        }


    }
}
