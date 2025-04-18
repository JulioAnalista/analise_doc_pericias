using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using PdfPigDocument = UglyToad.PdfPig.PdfDocument;
using System.Windows;
using SautinSoft;
using SkiaSharp;
using iText.Kernel;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using DocumentFormat.OpenXml.Wordprocessing;
using Tesseract;
using PdfSharp.Pdf;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using IOPath = System.IO.Path;
using iTextPath = iTextSharp.text.pdf.parser.Path;
using iText7PdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfWriter = iTextSharp.text.pdf.PdfWriter;
using PdfReader = iTextSharp.text.pdf.PdfReader;
using iTextDocument = iTextSharp.text.Document;
using iTextPdfWriter = iTextSharp.text.pdf.PdfWriter;
using SimpleTextExtractionStrategy = iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy;
using PdfTextExtractor = iTextSharp.text.pdf.parser.PdfTextExtractor;
using iText7PdfWriter = iText.Kernel.Pdf.PdfWriter;
using iText7PdfReader = iText.Kernel.Pdf.PdfReader;
using iText7PdfPage = iText.Kernel.Pdf.PdfPage;
using iText7PdfDictionary = iText.Kernel.Pdf.PdfDictionary;
using iText7PdfName = iText.Kernel.Pdf.PdfName;
using iText7PdfStream = iText.Kernel.Pdf.PdfStream;
using System.Windows.Documents;
using System.Linq;
using System.Collections.Concurrent;
using GabIA.BLL;
using GabIA.ENT;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Threading;
using System.Runtime.Intrinsics.X86;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore.Metadata;


namespace GabIA.WPF.Views
{
    /// <summary>
    /// Interaction logic for NeoGabView.xaml
    /// </summary>
    public partial class NeoGabView : UserControl
    {
        public delegate void ArquivoProcessadoEventHandler(object sender, EventArgs e);

        private int numeroPaginasCabecalho = 0;

        private DataManager _dataManager;

        public event ArquivoProcessadoEventHandler ArquivoProcessado;

        public event EventHandler ProcessItemSelected;
        //private void LoadPdfFromResources()
        //{
        //    string resourceName = "D:\\Pje\\Dados\\Blank.pdf";

        //    // Obtenha o caminho do arquivo PDF no diretório de saída
        //    //string pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resourceName);

        //    string pdfPath = resourceName;

        //    if (!File.Exists(pdfPath))
        //    {
        //        throw new FileNotFoundException($"O arquivo {resourceName} não foi encontrado.");
        //    }
        //    // Carregue o arquivo PDF no WebBrowser
        //    // Supondo que você tenha uma instância do WebBrowser chamada pdfWebBrowser
        //    pdfWebBrowser.Navigate(pdfPath);
        //}

        public void OnProcessItemSelected()
        {
            ProcessItemSelected?.Invoke(this, EventArgs.Empty);
        }

        public void CriarDiretorios(string arquivoPdf)
        {
            string diretorioBase = IOPath.Combine("D:\\PJe\\Processos", IOPath.GetFileNameWithoutExtension(arquivoPdf));
            string diretorioPecas = IOPath.Combine(diretorioBase, "PecasProcessuais");
            string diretorioPdf = IOPath.Combine(diretorioBase, "PDF");
            string diretorioDoc = IOPath.Combine(diretorioBase, "Doc");
            string diretorioCsv = IOPath.Combine(diretorioBase, "Csv");
            string diretorioJson = IOPath.Combine(diretorioBase, "Json");
            string diretorioPaginasPdf = IOPath.Combine(diretorioPecas, "PaginasPDF");
            string diretorioPecasPdf = IOPath.Combine(diretorioPecas, "PecasPDF");
            string diretorioPecasTexto = IOPath.Combine(diretorioPecas, "PaginasTexto");
            string diretorioPecasOCR = IOPath.Combine(diretorioPecas, "OCR");
            string diretorioPecasPNG = IOPath.Combine(diretorioPecas, "PNG");
            string diretorioPecasTextoLimpo = IOPath.Combine(diretorioPecas, "PecasTxt");
            string diretorioPecasTokenizadas = IOPath.Combine(diretorioPecas, "PecasTok");
            string diretorioPecasTextoModeloDeLinguagem = IOPath.Combine(diretorioBase, "ModeloDeLinguagem");
            string diretorioPecasTextoModeloDeLinguagemF1 = IOPath.Combine(diretorioBase, "ModeloDeLinguagem", "JsonlOriginal");
            string diretorioPecasTextoModeloDeLinguagemF2 = IOPath.Combine(diretorioBase, "ModeloDeLinguagem", "JsonToSummarize");
            string diretorioPecasTextoModeloDeLinguagemF3 = IOPath.Combine(diretorioBase, "ModeloDeLinguagem", "JsonlReceived");
            string diretorioPecasTextoModeloDeLinguagemF4 = IOPath.Combine(diretorioBase, "ModeloDeLinguagem", "JsonSummarized");
            string diretorioPecasTextoModeloDeLinguagemF5 = IOPath.Combine(diretorioBase, "ModeloDeLinguagem", "JsonMerged");
            string diretorioPecasTextoModeloDeLinguagemF6 = IOPath.Combine(diretorioBase, "ModeloDeLinguagem", "JsonSintetico");

            Directory.CreateDirectory(diretorioBase);
            Directory.CreateDirectory(diretorioPecas);
            Directory.CreateDirectory(diretorioPdf);
            Directory.CreateDirectory(diretorioDoc);
            Directory.CreateDirectory(diretorioCsv);
            Directory.CreateDirectory(diretorioJson);
            Directory.CreateDirectory(diretorioPaginasPdf);
            Directory.CreateDirectory(diretorioPecasPdf);
            Directory.CreateDirectory(diretorioPecasTexto);
            Directory.CreateDirectory(diretorioPecasOCR);
            Directory.CreateDirectory(diretorioPecasPNG);
            Directory.CreateDirectory(diretorioPecasTextoLimpo); //diretorioPecasTokenizadas 
            Directory.CreateDirectory(diretorioPecasTokenizadas); // 
            Directory.CreateDirectory(diretorioPecasTextoModeloDeLinguagem);
            Directory.CreateDirectory(diretorioPecasTextoModeloDeLinguagemF1);
            Directory.CreateDirectory(diretorioPecasTextoModeloDeLinguagemF2);
            Directory.CreateDirectory(diretorioPecasTextoModeloDeLinguagemF3);
            Directory.CreateDirectory(diretorioPecasTextoModeloDeLinguagemF4);
            Directory.CreateDirectory(diretorioPecasTextoModeloDeLinguagemF5);
            Directory.CreateDirectory(diretorioPecasTextoModeloDeLinguagemF6);
        }

        public async void ExecutarAplicacaoConsole(string argumento1, string argumento2)
        {
            string caminhoAplicacao = @"D:\ExportDOCX\ExportPDFToDocx.exe";
            string argumentos = $"{argumento1} {argumento2}";

            ProcessStartInfo startInfo = new ProcessStartInfo(caminhoAplicacao, argumentos)
            {
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            try
            {
                using Process processo = new Process { StartInfo = startInfo };
                processo.Start();

                // Insira o tempo de espera desejado em milissegundos
                //int tempoDeEspera = 5000; // 5 segundos, por exemplo
                // await Task.Delay(tempoDeEspera);

                // Se você quiser garantir que o processo termine antes de continuar, descomente a linha abaixo
                // processo.WaitForExit();
            }
            catch (Exception ex)
            {
                // Faça algo com a exceção, como exibir uma mensagem de erro
                MessageBox.Show($"Ocorreu um erro ao executar a aplicação console: {ex.Message}");
            }
        }

        private bool ContemInformacoesPlanilha(string texto)
        {
            return texto.Contains("PJe:") && texto.Contains("ID.") && texto.Contains("Pág.");
        }


        public string ExtractText(string path)
        {
            using (var reader = new PdfReader(path))
            {
                var text = string.Empty;
                for (var page = 1; page <= reader.NumberOfPages; page++)
                {
                    text += PdfTextExtractor.GetTextFromPage(reader, page, new SimpleTextExtractionStrategy());
                }
                return text;
            }
        }

        public string ExtractText_old(PdfSharp.Pdf.PdfPage page, string arquivoPdf, int pageNumber)
        {
            try
            {
                string tempFilePath = IOPath.GetTempFileName();
                using (iText.Kernel.Pdf.PdfReader pdfReader = new iText.Kernel.Pdf.PdfReader(arquivoPdf))
                {
                    using (iText.Kernel.Pdf.PdfDocument sourceDoc = new iText.Kernel.Pdf.PdfDocument(pdfReader))
                    {
                        using (iText.Kernel.Pdf.PdfWriter pdfWriter = new iText.Kernel.Pdf.PdfWriter(tempFilePath))
                        {
                            using (iText.Kernel.Pdf.PdfDocument tempDoc = new iText.Kernel.Pdf.PdfDocument(pdfWriter))
                            {
                                iText.Kernel.Pdf.PdfPage sourcePage = sourceDoc.GetPage(pageNumber);
                                sourceDoc.CopyPagesTo(pageNumber, pageNumber, tempDoc);
                                tempDoc.Close();
                            }
                        }
                    }
                }

                using (PdfPigDocument document = PdfPigDocument.Open(tempFilePath))
                {
                    if (document.NumberOfPages == 0)
                    {
                        return string.Empty;
                    }

                    UglyToad.PdfPig.Content.Page pigPage = document.GetPage(1);
                    return pigPage.Text;
                }
            }

            catch (UglyToad.PdfPig.Fonts.InvalidFontFormatException ex)
            {

                MessageBox.Show("// Registre a exceção ou exiba uma mensagem de aviso");
                return "-";
            }
        }

        public string RemoverCadeiaDeCaracteresDesnecessarios(string nomeArquivo)
        {
            string cadeiaRemover = " (TJDFT - PJe1)";
            int index = nomeArquivo.IndexOf(cadeiaRemover, StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                string novoNomeArquivo = nomeArquivo.Remove(index, cadeiaRemover.Length);
                return novoNomeArquivo;
            }

            return nomeArquivo;
        }

        public List<string> ListarArquivosPdf(string diretorio)
        {
            string[] arquivos = Directory.GetFiles(diretorio, "*.pdf");
            List<string> arquivosRenomeados = new List<string>();

            foreach (string arquivo in arquivos)
            {
                string novoNomeArquivo = RemoverCadeiaDeCaracteresDesnecessarios(arquivo);
                if (arquivo != novoNomeArquivo)
                {
                    File.Move(arquivo, novoNomeArquivo);
                }
                arquivosRenomeados.Add(novoNomeArquivo);
            }

            return arquivosRenomeados;
        }

        public string ObterIdentificadorDoRodape(string texto)
        {
            string padrao = @"Número processo PJe:.*?ID\.\s(\d+)\sPág";
            Match match = Regex.Match(texto, padrao);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        public async Task salvarPaginas (string arquivoPdf, string diretorioBase)
        {
            string numeroProcesso = IOPath.GetFileName(diretorioBase);
            try
            {
                using (iText.Kernel.Pdf.PdfReader reader = new iText.Kernel.Pdf.PdfReader(arquivoPdf))
                {
                    using (iText.Kernel.Pdf.PdfDocument pdfDoc = new iText.Kernel.Pdf.PdfDocument(reader))
                    {
                        int totalPaginas = pdfDoc.GetNumberOfPages();
                        string diretorioPecas = IOPath.Combine(diretorioBase, "PecasProcessuais");

                        for (int i = numeroPaginasCabecalho + 1; i <= totalPaginas; i++)
                        {
                            try
                            {
                                string nomeArquivoPagina = IOPath.Combine(diretorioPecas, "PDF", numeroProcesso + "_P" + (i - numeroPaginasCabecalho).ToString("D4") + ".pdf");
                                bool hasLargeImage = false;
                                iText.Kernel.Pdf.PdfPage page = pdfDoc.GetPage(i);

                                // rest of the code

                                // Salvando o texto extraído
                                ////try
                                ////{
                                ////    string txtFilePath = IOPath.Combine(diretorioPecas, "Texto", numeroProcesso + "_imgP" + (i - numeroPaginasCabecalho).ToString("D4") + ".txt");
                                ////    File.WriteAllText(txtFilePath, ocrText);
                                ////    Debug.WriteLine($" TXT {i}/{totalPaginas} ");
                                ////}
                                ////catch (Exception e)
                                ////{
                                ////    Console.WriteLine("Error writing OCR text to file: " + e.Message);
                                ////}
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error processing page " + i + ": " + e.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error opening PDF file: " + e.Message);
            }
        }



        public async Task SalvarPaginas_old(string arquivoPdf, string diretorioBase)
        {

            string numeroProcesso = IOPath.GetFileName(diretorioBase);

            using (iText.Kernel.Pdf.PdfReader reader = new iText.Kernel.Pdf.PdfReader(arquivoPdf))
            {
                using (iText.Kernel.Pdf.PdfDocument pdfDoc = new iText.Kernel.Pdf.PdfDocument(reader))
                {
                    int totalPaginas = pdfDoc.GetNumberOfPages();
                    string diretorioPecas = IOPath.Combine(diretorioBase, "PecasProcessuais");

                    for (int i = numeroPaginasCabecalho + 1; i <= totalPaginas; i++)
                    {
                        string nomeArquivoPagina = IOPath.Combine(diretorioPecas, "PDF", numeroProcesso + "_P" + (i - numeroPaginasCabecalho).ToString("D4") + ".pdf");

                        bool hasLargeImage = false;
                        iText.Kernel.Pdf.PdfPage page = pdfDoc.GetPage(i);

                        iText.Kernel.Pdf.PdfDictionary pageDict = page.GetPdfObject();
                        iText.Kernel.Pdf.PdfDictionary resources = pageDict.GetAsDictionary(iText.Kernel.Pdf.PdfName.Resources);
                        iText.Kernel.Pdf.PdfDictionary xobjects = resources.GetAsDictionary(iText.Kernel.Pdf.PdfName.XObject);

                        if (xobjects != null)
                        {
                            foreach (iText.Kernel.Pdf.PdfName imgRef in xobjects.KeySet())
                            {
                                iText.Kernel.Pdf.PdfStream imgStream = xobjects.GetAsStream(imgRef);

                                if (!imgStream.ContainsKey(iText.Kernel.Pdf.PdfName.Subtype) || !imgStream.Get(iText.Kernel.Pdf.PdfName.Subtype).Equals(iText.Kernel.Pdf.PdfName.Image))
                                    continue;

                                iText.Kernel.Pdf.Xobject.PdfImageXObject pdfImageXObject = new iText.Kernel.Pdf.Xobject.PdfImageXObject((iText.Kernel.Pdf.PdfStream)imgStream);
                                byte[] imgBytes = pdfImageXObject.GetImageBytes();

                                if (imgBytes.Length > 20 * 1024)
                                {
                                    hasLargeImage = true;
                                    // Salvando a página com o sufixo 'img'
                                    string imgPdfPageName = IOPath.Combine(diretorioPecas, "PDF", numeroProcesso + "_imgP" + (i - numeroPaginasCabecalho).ToString("D4") + ".pdf");
                                    using (iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(imgPdfPageName))
                                    {
                                        using (iText.Kernel.Pdf.PdfDocument newPdf = new iText.Kernel.Pdf.PdfDocument(writer))
                                        {
                                            pdfDoc.CopyPagesTo(i, i, newPdf);
                                            Debug.WriteLine(imgPdfPageName + " Extraída");
                                        }
                                    }
                                    // Chamando o método ConvertPdfToPng_cpp
                                    //ConvertPdfToPng_cpp(imgPdfPageName, IOPath.Combine(diretorioPecas, "Texto"), "600");

                                    string pngFilePath = IOPath.Combine(diretorioPecas, "Texto", numeroProcesso + "_imgP" + (i - numeroPaginasCabecalho).ToString("D4") + "_1.png");

                                    /* Instanciando a classe ProcessamentoDeTexto e realizando OCR
                                    ProcessamentoDeTexto procTexto = new ProcessamentoDeTexto();
                                    string ocrText = procTexto.PerformOcr(pngFilePath);  

                                    // Salvando o texto extraído
                                    string txtFilePath = IOPath.Combine(diretorioPecas, "Texto", numeroProcesso + "_imgP" + (i - numeroPaginasCabecalho).ToString("D4") + ".txt");
                                    File.WriteAllText(txtFilePath, ocrText);
                                    Debug.WriteLine($" TXT {i}/{totalPaginas} "); */

                                    break;

                                }
                            }
                        }

                        if (!hasLargeImage)
                        {

                            using (iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(nomeArquivoPagina))
                            {
                                using (iText.Kernel.Pdf.PdfDocument newPdf = new iText.Kernel.Pdf.PdfDocument(writer))
                                {
                                    pdfDoc.CopyPagesTo(i, i, newPdf);
                                    Debug.WriteLine($" TXT {i}/{totalPaginas} ");
                                }
                            }
                            /* Salvar a primeira página
                            RodaAppJava myClass = new RodaAppJava();
                            Task taskTable = myClass.RunProcessAsync("java", "d:\\Java\\projetos\\gabia.pdf\\pdfutils-app\\target\\pdfutils-app-1.0-SNAPSHOT.jar",
                                "d:\\PJe\\App_cpp\\extractAllText.py",
                                nomeArquivoPagina,
                                IOPath.Combine(diretorioBase, "PecasProcessuais","Texto")); */

                            //ExtractTextFromPdfcpp(nomeArquivoPagina, IOPath.Combine(diretorioPecas, "Texto"), "1");
                        } 
                    }
                }
            }
        }


        public bool CheckTextExistence(string arquivoPdf, int pageNumber, string targetText)
        {
            using (iText.Kernel.Pdf.PdfReader pdfReader = new iText.Kernel.Pdf.PdfReader(arquivoPdf))
            {
                using (iText.Kernel.Pdf.PdfDocument iText7PdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader))
                {
                    iText.Kernel.Pdf.PdfPage page = iText7PdfDocument.GetPage(pageNumber);
                    iText.Kernel.Pdf.Canvas.Parser.Listener.LocationTextExtractionStrategy extractionStrategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.LocationTextExtractionStrategy();

                    string pageText = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page, extractionStrategy);

                    return pageText.Contains(targetText);
                }
            }
        }

        public async Task SalvarCabecalho(string arquivoPdf, string diretorioBase)
        {
            string numeroProcesso = "";
            numeroProcesso = IOPath.GetFileName(diretorioBase);
            string nomeArquivoCabecalho = IOPath.Combine(diretorioBase, "PDF", numeroProcesso + "cab.pdf");
            if(File.Exists(nomeArquivoCabecalho)) { 
            
                return; 
            }

            using (PdfSharp.Pdf.PdfDocument cabecalho = new PdfSharp.Pdf.PdfDocument())
            {
                using (PdfSharp.Pdf.PdfDocument documento = PdfSharp.Pdf.IO.PdfReader.Open(arquivoPdf, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import))
                {
                    int i = 0;
                    while (i < documento.PageCount)
                    {
                        PdfSharp.Pdf.PdfPage pagina = documento.Pages[i];
                        string nomeArquivoPaginaIndividual = IOPath.Combine(diretorioBase, "PDF", "tabelas", numeroProcesso + "_P" + (i + 1).ToString("D2") + ".pdf");

                        if(!Directory.Exists(IOPath.Combine(diretorioBase, "PDF", "tabelas")))
                        {
                            Directory.CreateDirectory(IOPath.Combine(diretorioBase, "PDF", "tabelas"));
                        }

                        if (CheckTextExistence(arquivoPdf, i + 1, "Número processo PJe:"))
                        {
                            numeroPaginasCabecalho = i;
                            break;
                        }
                        else
                        {
                            using (PdfSharp.Pdf.PdfDocument documentoPaginaIndividual = new PdfSharp.Pdf.PdfDocument())
                            {
                                documentoPaginaIndividual.AddPage(pagina);
                                documentoPaginaIndividual.Save(nomeArquivoPaginaIndividual);
                            }
                            cabecalho.AddPage(pagina);
                            i++;
                        }
                    }

                    if (cabecalho.PageCount > 0)
                    {
                        Debug.WriteLine(nomeArquivoCabecalho + " Gravando Cabeçalho");

                        if (!File.Exists(nomeArquivoCabecalho))
                            cabecalho.Save(nomeArquivoCabecalho);
                    }
                }
            }
            
            if (File.Exists(nomeArquivoCabecalho))
            {
                using (PdfSharp.Pdf.PdfDocument cabecalhoSalvo = PdfSharp.Pdf.IO.PdfReader.Open(nomeArquivoCabecalho, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import))
                {
                    if (cabecalhoSalvo.PageCount > 0)
                    {
                        // Salvar a primeira página
                        //RodaAplicacaoConsoleJava myClass = new RodaAplicacaoConsoleJava();

                        Debug.WriteLine("Extraindo a primeira página (PDF)");
                        string nomeArquivoPagina1 = IOPath.Combine(diretorioBase, "PDF", numeroProcesso + "_P0001.pdf");


                        using (PdfSharp.Pdf.PdfDocument documentoPagina1 = new PdfSharp.Pdf.PdfDocument())
                        {
                            documentoPagina1.AddPage(cabecalhoSalvo.Pages[0]);
                            documentoPagina1.Save(nomeArquivoPagina1);
                        }
                        Debug.WriteLine(nomeArquivoPagina1 + " Dados do Processo (TXT)");

                        string ProcName = "Mon" + CriaNomeUnico();

                        var args = new List<string> {
                            "c:\\Python311\\python.exe",
                            "d:\\PJe\\App_cpp\\extractTextoProc.py",
                            nomeArquivoPagina1,
                            IOPath.Combine(diretorioBase, "Json"),
                            "d:\\PJe\\Monitor\\" + ProcName};

                        // Criar uma instância da classe MonitoraExcecucaoAssincrona
                        MonitoraExcecucaoAssincrona monitor = new MonitoraExcecucaoAssincrona("caminho", "arquivo", 10000);

                        monitor.RodaProcessoCMD(args.ToArray());

                        string ProcName1 = "Mon" + CriaNomeUnico();

                        var args1 = new List<string> {
                            "c:\\Python311\\python.exe",
                            "d:\\PJe\\App_cpp\\extractTableMovimentos.py",
                            IOPath.Combine(diretorioBase, "PDF", "tabelas"),
                            IOPath.Combine(diretorioBase, "Json"),
                            "d:\\PJe\\Monitor\\" + ProcName1};

                        // Criar uma instância da classe MonitoraExcecucaoAssincrona
                        MonitoraExcecucaoAssincrona monitor1 = new MonitoraExcecucaoAssincrona(IOPath.Combine(diretorioBase, "Json"), "arquivo", 10000);

                        monitor1.RodaProcessoCMD(args1.ToArray());

                        Debug.WriteLine(nomeArquivoCabecalho + " Extraindo Tabelas");
                        //var argsMov = new List<string> {
                        //    "d:\\PJe\\App_cpp\\extractTableMovimentos.py",
                        //    IOPath.Combine(diretorioBase, "PDF", "tabelas"),
                        //    IOPath.Combine(diretorioBase, "Json")};

                        //var rodaMov = new RodaAplicacaoConsoleJava(
                        //    "C:\\Arquivos de Programas\\Java\\jdk-20\\bin\\java.exe",
                        //    "d:\\Java\\projetos\\gabia.pdf\\pdfutils-app\\target\\RodaAppPythonJson.jar",
                        //    argsMov 
                        //);

                        //rodaMov.Run();

                        string ProcName2 = "Mon" + CriaNomeUnico();


                        var args2 = new List<string> {
                            "c:\\Python311\\python.exe",
                            "d:\\PJe\\App_cpp\\extractTablePolos.py",
                            nomeArquivoCabecalho,
                            IOPath.Combine(diretorioBase, "Json"),
                            "d:\\PJe\\Monitor\\" + ProcName2};


                        // Criar uma instância da classe MonitoraExcecucaoAssincrona
                        MonitoraExcecucaoAssincrona monitor2 = new MonitoraExcecucaoAssincrona("caminho", "arquivo", 10000);

                        monitor2.RodaProcessoCMD(args2.ToArray());
                    }
                }
            }
        }


        protected virtual void OnArquivoProcessado()
        {
            ArquivoProcessado?.Invoke(this, EventArgs.Empty);
        }

        public async Task RunJavaProcessAsync(string javaApplication, string jarFile, string pythonScript, string pdfFile, string outputDirectory)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(javaApplication)
            {
                Arguments = $"-jar {jarFile} {pythonScript} {pdfFile} {outputDirectory}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = await process.StandardOutput.ReadLineAsync();
                    System.Console.WriteLine(line);
                }
                await Task.Run(() => process.WaitForExit());
            }
        }

        public async void RegularizaPecas(string processoNR)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string dirModeloDeLinguagem = IOPath.Combine("d:\\PJe\\Processos\\" + processoNR + "\\ModeloDeLinguagem");
            dirModeloDeLinguagem = IOPath.Combine(dirModeloDeLinguagem, "JsonlOriginal");
            string origem = @"d:\PJe\Processos\" + processoNR + @"\PecasProcessuais\PecasTxt";

            OperacoesBLL operacoes = new OperacoesBLL();
            List<AtoProcessualENT> atosDoProcesso = operacoes.BuscarTodosOsAtosDoProcesso(processoNR);

            OrganizaPecas toJson = new OrganizaPecas();
            toJson.RegularizaArquivosPorTamanhoToJson(origem, dirModeloDeLinguagem, 6000, atosDoProcesso);
            stopwatch.Stop();
            TimeSpan tempoDecorrido = stopwatch.Elapsed;
            MessageBox.Show($"Processado: {processoNR}, em {tempoDecorrido} ");
        }
        public async void separaPecasPDF()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int arquivosPDF_noDiretorio = 0;

            // Defina o diretório onde estão os arquivos PDF
            string diretorio = "d:\\PJe\\Baixados";
            
            // Liste e renomeie os arquivos PDF no diretório
            List<string> arquivosPdfRenomeados = ListarArquivosPdf(diretorio);

            //arquivosPDF_noDiretorio = arquivosPdfRenomeados.Count;

            // Crie os diretórios e subdiretórios necessários para cada arquivo PDF renomeado
            foreach (string arquivoPdf in arquivosPdfRenomeados)
            {
                arquivosPDF_noDiretorio++;
                //MainWindow.processados++;
                //ArquivoProcessado?.Invoke(this, EventArgs.Empty);
                string numeroProcesso = IOPath.GetFileNameWithoutExtension(arquivoPdf);
                Application.Current.Properties["PJ"] = numeroProcesso;


                string diretorioBase = IOPath.Combine("D:\\PJe\\Processos", IOPath.GetFileNameWithoutExtension(arquivoPdf));
                string diretorioPecas = IOPath.Combine(diretorioBase, "PecasProcessuais");

                string arquivoNoDiretorioProcesso = IOPath.Combine(diretorioBase, "pdf", numeroProcesso + ".PDF");
                if (File.Exists(arquivoNoDiretorioProcesso))
                {
                    File.Delete(arquivoNoDiretorioProcesso);
                }

                Debug.WriteLine("criando diretórios");
                CriarDiretorios(arquivoPdf);

                Debug.WriteLine("Separando cabeçalho do processo");

                SalvarCabecalho(arquivoPdf, diretorioBase);
                // Cria uma instância de OperacoesBLL
                OperacoesBLL operacoes = new OperacoesBLL(/* coloque aqui as dependências necessárias */);
                // Obtem o objeto ProcessoCompletoENT atualizado
                List<AtoProcessualENT> atosDoProcesso = operacoes.BuscarTodosOsAtosDoProcesso(numeroProcesso);

                if (File.Exists(@"d:\PJe\Baixados\" + numeroProcesso + ".PDF"))
                {
                    string destino = IOPath.GetDirectoryName(diretorioBase);

                    //agora vamos atualizar o banco de dados
                    Debug.WriteLine("Atualizando a base de dados");
                    _dataManager = new DataManager();
                    // Executa o método para carregar os dados
                    await _dataManager.CarregarDadosProcessoAsync(diretorioBase, numeroProcesso);

                    // Cria uma instância de OperacoesBLL

                    Debug.WriteLine("Separando Páginas do processo");

                    Debug.WriteLine("Separando Páginas do processo - PDF");
                    // neste ponto, vou chamar a função java
                    //RodaAplicacaoConsoleJava myClass = new RodaAplicacaoConsoleJava();
                    // Verifica se o diretório está vazio

                    var folder = new DirectoryInfo(IOPath.Combine(diretorioPecas, "PaginasPDF"));
                    int filesCount = 0;
                    if (folder.Exists)
                    {
                        filesCount = folder.GetFileSystemInfos().Length;
                    }

                    if (filesCount == 0)
                    {

                        string ProcName = "Mon" + CriaNomeUnico();
                        string logDir = IOPath.Combine("D:\\PJe", "Arquivos_Log");
                        int nrPgsInt = numeroPaginasCabecalho + 1;
                        string nrPag = nrPgsInt.ToString();

                        var args = new List<string> {
                                "c:\\Python311\\python.exe",
                                "d:\\PJe\\App_cpp\\extractPagePDF.py",
                                arquivoPdf,
                                IOPath.Combine(diretorioPecas, "PaginasPDF"),
                                logDir,
                                nrPag,
                                "d:\\PJe\\Monitor\\" + ProcName};

                        // Criar uma instância da classe MonitoraExcecucaoAssincrona
                        MonitoraExcecucaoAssincrona monitor = new MonitoraExcecucaoAssincrona("caminho", "arquivo", 10000);

                        monitor.RodaProcessoCMD(args.ToArray());
                    }


                    folder = new DirectoryInfo(IOPath.Combine(diretorioPecas, "PaginasTexto"));
                    filesCount = 0;
                    if (folder.Exists)
                    {
                        filesCount = folder.GetFileSystemInfos().Length;
                    }

                    if (filesCount == 0)
                    {
                        Debug.WriteLine("Separando Páginas do processo - Texto");

                        string ProcName = "Mon" + CriaNomeUnico();
                        string logDir = IOPath.Combine("D:\\PJe", "Arquivos_Log");
                        int nrPgsInt = numeroPaginasCabecalho + 1;
                        string nrPag = nrPgsInt.ToString();

                        ProcName = "Mon" + CriaNomeUnico();
                        logDir = IOPath.Combine("D:\\PJe", "Arquivos_Log");
                        nrPgsInt = numeroPaginasCabecalho + 1;
                        nrPag = nrPgsInt.ToString();

                        //marca
                        var argsTxt = new List<string> {
                                "c:\\Python311\\python.exe",
                                "d:\\PJe\\App_cpp\\extractPageText.py",
                                arquivoPdf,
                                IOPath.Combine(diretorioPecas, "PaginasTexto"),
                                logDir,
                                nrPag,
                                "d:\\PJe\\Monitor\\" + ProcName};

                        // Criar uma instância da classe MonitoraExcecucaoAssincrona
                        MonitoraExcecucaoAssincrona monitor = new MonitoraExcecucaoAssincrona("caminho", "arquivo", 10000);
                        monitor.RodaProcessoCMD(argsTxt.ToArray());
                    }
                        

                    var processador = new ProcessamentoDeTexto();
                    processador.ProcessaArquivos(IOPath.Combine(destino, numeroProcesso, "PecasProcessuais", "PaginasTexto"), IOPath.Combine(destino, numeroProcesso, "PecasProcessuais", "PaginasPDF"));
                    

                    destino = IOPath.GetDirectoryName(diretorioBase);
                    string dirModeloDeLinguagem = IOPath.Combine(destino, numeroProcesso, "ModeloDeLinguagem");
                    dirModeloDeLinguagem = IOPath.Combine(dirModeloDeLinguagem, "JsonlOriginal");
                    //string origem = IOPath.Combine(dirModeloDeLinguagem, "JsonlOriginal"); @"d:\PJe\Processos\0704628-29.2022.8.07.0008\PecasProcessuais\PecasTxt";

                    // este método resolve o problema das petições de encaminhamento "segue petição anexa"
                    ProcessamentoDeTexto limpeza = new ProcessamentoDeTexto();
                    limpeza.MoveConteudoIDCorreto(IOPath.Combine("d:\\PJe\\Processos", numeroProcesso, "PecasProcessuais", "PecasTxt"));


                    folder = new DirectoryInfo(dirModeloDeLinguagem);
                    filesCount = 0;
                    if (folder.Exists)
                    {
                        filesCount = folder.GetFileSystemInfos().Length;
                    }

                    if (filesCount == 0)
                    {
                        string origem = IOPath.Combine("d:\\PJe\\Processos", numeroProcesso, "PecasProcessuais", "PecasTxt");
                        OrganizaPecas toJson = new OrganizaPecas();
                        toJson.RegularizaArquivosPorTamanhoToJson(origem, dirModeloDeLinguagem, 10000, atosDoProcesso);
                    }


                    //primeira etapa faz a limpeza dos arquivos e escreve em jsonlreceived

                    APIRequest processa_IA = new APIRequest();
                    string promptFilePath = "d:\\PJe\\Dados\\Templates\\Corrige.txt";
                    string systemContent = processa_IA.ReadPromptFromFile(promptFilePath);
                    string inputDirectory = dirModeloDeLinguagem;
                    string outputDirectory = IOPath.Combine(destino, numeroProcesso, "ModeloDeLinguagem", "JsonlReceived");
                    string apiKey = "d:\\PJe\\App_cpp\\Senha.py";

                    //await APIRequest.ProcessDirectory_Para(inputDirectory, outputDirectory, apiKey, systemContent);
                    //await APIRequest.ProcessDirectory(inputDirectory, outputDirectory, apiKey, systemContent);
                    //string pathJson = "D:\\PJe\\Processos\\0704628-29.2022.8.07.0008\\ModeloDeLinguagem\\0704628-29.2022.8.07.0008.jsonl";
                    var Unifica = new ProcessamentoDeTexto();
                    //Unifica.ProcessarJsonl(pathJson);

                    await APIRequest.ProcessDirectory_Azure(inputDirectory, outputDirectory, apiKey, systemContent);
                    string outputToSummarize = IOPath.Combine(destino, numeroProcesso, "ModeloDeLinguagem", "JsonToSummarize");



                }
                OnArquivoProcessado();
            }
            stopwatch.Stop();
            TimeSpan tempoDecorrido = stopwatch.Elapsed;
            MessageBox.Show($"Processados: {arquivosPDF_noDiretorio}, em {tempoDecorrido} ");
        }

        
        public static void ProcessFilesInDirectory(string sourceDirectory, string destinationFilePath, List<AtoProcessualENT> atosDoProcesso)
        {
            // Ensure the destination directory exists
            string destinationDirectory = IOPath.GetDirectoryName(destinationFilePath);
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            // Loop over the files in the source directory
            foreach (string filePath in Directory.EnumerateFiles(sourceDirectory))
            {
                // Extract the ID from the file name
                string fileName = IOPath.GetFileNameWithoutExtension(filePath);
                string idMovimento = fileName.Substring(fileName.IndexOf('P') + 1, fileName.IndexOf('_') - fileName.IndexOf('P') - 1);

                // Find the corresponding 'resumo' from the process list
                var atoProcessual = atosDoProcesso.Find(a => a.IdMovimento == idMovimento);
                if (atoProcessual == null)
                {
                    Console.WriteLine($"Could not find resumo for idMovimento: {idMovimento}");
                    continue;
                }
                string resumo = atoProcessual.Resumo;

                // Load the file content
                string fileContent = File.ReadAllText(filePath);

                // Create new content with file name, idMovimento, resumo and tags
                string newContent = $"<inicio> - {idMovimento}\nResumo - {resumo}\n{fileContent}\n<fim>\n";

                // Append the new content to the destination file
                File.AppendAllText(destinationFilePath, newContent);
            }
        }




        public static void RenomearArquivos(string pasta)
        {
            string[] arquivos = Directory.GetFiles(pasta, "*.txt.reg");

            foreach (string arquivo in arquivos)
            {
                string fName = IOPath.GetFileNameWithoutExtension(arquivo);

                string novoNome = IOPath.ChangeExtension(fName, ".rtf");
                Debug.WriteLine(IOPath.GetDirectoryName(arquivo));
                novoNome = IOPath.Combine(IOPath.GetDirectoryName(arquivo), novoNome);
                // Verifica se o novo nome já existe
                if (File.Exists(novoNome))
                {
                    Console.WriteLine($"Não foi possível renomear o arquivo {arquivo}. Já existe um arquivo com o nome {novoNome}.");
                    continue;
                }

                // Renomeia o arquivo
                try
                {
                    File.Move(arquivo, novoNome);
                    Console.WriteLine($"O arquivo {arquivo} foi renomeado para {novoNome}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ocorreu um erro ao renomear o arquivo {arquivo}: {ex.Message}");
                }
            }
        }

        public async Task ProcessarArquivos_Par(string diretorio, string diretorioPecas, string logDir, int nrPag)
        {
            // Carregar todos os arquivos no diretório
            var arquivos = Directory.GetFiles(diretorio);
            string diretorioPecasSumarizadas = IOPath.GetDirectoryName(diretorio);

            Debug.WriteLine("Processando API OpenAI");

            // Ordenar os arquivos por data de criação
            var arquivosOrdenados = arquivos.OrderBy(f => File.GetCreationTime(f)).ToList();

            // Criação de uma fila para gerenciar os arquivos a serem processados
            var filaArquivos = new ConcurrentQueue<string>(arquivosOrdenados);

            // Criar um conjunto de tarefas (uma para cada thread)
            var tarefas = Enumerable.Range(0, 8).Select(async _ =>
            {
                while (filaArquivos.TryDequeue(out var arquivo))
                {
                    string content = File.ReadAllText(arquivo);

                    if (content.Length < 200 && content.Split('\n').Length < 8)
                    {
                        // Renomeie o arquivo e mova para o diretório de destino.
                        string novoNomeArquivo = IOPath.ChangeExtension(arquivo, ".rtf");
                        string novoCaminhoArquivo = IOPath.Combine(diretorioPecasSumarizadas, novoNomeArquivo);

                        File.Move(arquivo, novoCaminhoArquivo);
                    }
                    else
                    {
                        // Preparar argumentos para o script Python
                        Debug.WriteLine("Processando: " + IOPath.GetFileNameWithoutExtension(arquivo));
                        var argsAI = new List<string> {
                            "d:\\PJe\\App_cpp\\openAI_organiza.py",
                            arquivo,
                        IOPath.Combine(diretorioPecasSumarizadas, "Fase_II")};

                        // Executar a aplicação Java com o script Python e argumentos
                        var rodaAI = new RodaAplicacaoConsoleJava(
                            "C:\\Arquivos de Programas\\Java\\jdk-20\\bin\\java.exe",
                            "d:\\Java\\projetos\\gabia.pdf\\pdfutils-app\\target\\RodaAppPythonJson.jar",
                            argsAI
                        );

                        await rodaAI.RunAsync(); // aqui mudamos de Run() para RunAsync()
                    }
                }
            });

            // Aguardar todas as tarefas concluírem
            await Task.WhenAll(tarefas);
        }

        public void SumarizaPecasProcessuais(string diretorio, string diretorioPecas, string logDir, int nrPag)
        {
            // Carregar todos os arquivos no diretório
            var arquivos = Directory.GetFiles(diretorio);
            string diretorioPecasSumarizadas = IOPath.Combine(IOPath.GetDirectoryName(diretorio), "Resumos");

            Directory.CreateDirectory(diretorioPecasSumarizadas);

            // Criar um diretório temporário para guardar os arquivos divididos
            string tempDirectory = IOPath.Combine(IOPath.GetDirectoryName(diretorio), "Temp");
            Directory.CreateDirectory(tempDirectory);

            // Ordenar os arquivos por data de criação
            var arquivosOrdenados = arquivos.OrderBy(f => File.GetCreationTime(f)).ToList();

            // Iterar em todos os arquivos
            foreach (var arquivo in arquivosOrdenados)
            {
                Debug.WriteLine(arquivo);
                if (File.Exists(arquivo))
                {
                    var nomeArquivo = IOPath.GetFileNameWithoutExtension(arquivo);
                    var idMovimento = ExtraiIdDoArquivo(nomeArquivo);
                    var tipoPeca = BuscarTipoPeca(idMovimento, logDir);

                    if (tipoPeca.ToLower() == "petição inicial")
                    {
                        tipoPeca = "petição";
                    }

                    tipoPeca = ClassificarTextoDoArquivo(arquivo);

                    // Carregar o conteúdo do arquivo
                    var fileContent = File.ReadAllText(arquivo);

                    // Dividir o conteúdo do arquivo em partes de até 38 KB
                    var chunks = SplitTextIntoChunks(fileContent, 32 * 1024);
                    int chunkId = 1;

                    // Processar cada parte do arquivo
                    foreach (var chunk in chunks)
                    {
                        // Criar um arquivo temporário para cada parte
                        Directory.CreateDirectory(tempDirectory);
                        string tempFilePath = IOPath.Combine(tempDirectory, $"P{idMovimento}_P{chunkId.ToString("D3")}.txt");
                        File.WriteAllText(tempFilePath, chunk);
                        chunkId++;

                        var chunkSize = Encoding.UTF8.GetByteCount(chunk);

                        if (chunkSize <= 8192) // até 8kb
                        {
                            ChamaScriptPython("d:\\PJe\\App_cpp\\openAI_sumarizaPar.py",
                                tempFilePath,
                                tempDirectory,
                                tipoPeca.ToLower(),
                                "gpt-3.5-turbo");
                        }
                        else if (chunkSize <= 36768) // entre 8kb e 32kb
                        {
                            ChamaScriptPython("d:\\PJe\\App_cpp\\openAI_sumarizaPar.py",
                                tempFilePath,
                                tempDirectory,
                                tipoPeca.ToLower(),
                                "gpt-3.5-turbo-16k");
                        }
                        else
                        {
                            MessageBox.Show("Cheguei em um arquivo gigantesco!!");
                        }
                    }

                    // Consolidar os resultados
                    var resultFilePath = IOPath.Combine(diretorioPecasSumarizadas, nomeArquivo + ".rtf");
                    var tempFiles = Directory.GetFiles(tempDirectory).OrderBy(f => f);
                    foreach (var tempFile in tempFiles)
                    {
                        var tempContent = File.ReadAllText(tempFile);
                        File.AppendAllText(resultFilePath, tempContent);
                    }

                    // Limpar os arquivos temporários
                    Directory.Delete(tempDirectory, true);
                }
            }
        }

        private static List<string> SplitTextIntoChunks(string text, int chunkSize)
        {
            var paragraphs = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var chunks = new List<string>();
            var chunk = new StringBuilder(chunkSize);

            foreach (var paragraph in paragraphs)
            {
                var paragraphSize = Encoding.UTF8.GetByteCount(paragraph);
                if (chunk.Length + paragraphSize > chunkSize)
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



        public string ClassificarTextoDoArquivo(string caminhoDoArquivo)
        {
            // Lista de palavras-chave e a correspondência de retorno
            var categorias = new Dictionary<List<string>, string> {
                    { new List<string> { "excelentíssimo", "propor", "ação", "pedido" }, "inicial" },
                    { new List<string> { "tutela", "urgência", "pedido" }, "inicial" },
                    { new List<string> { "fatos", "direito", "causa" }, "inicial" },
                    { new List<string> { "contestação" }, "contestação" },
                    { new List<string> { "réplica" }, "réplica" },
                    { new List<string> { "replica" }, "réplica" },
                    // adicione mais categorias conforme necessário
                };

            // Lê todo o texto do arquivo
            var textoDoArquivo = File.ReadAllText(caminhoDoArquivo);

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

        private string BuscarTipoPeca(string idMovimento, string numeroProcesso)
        {
            OperacoesBLL operacoes = new OperacoesBLL(/* coloque aqui as dependências necessárias */);
            List<AtoProcessualENT> atosDoProcesso = operacoes.BuscarTodosOsAtosDoProcesso(numeroProcesso);
            List<TipoAtoProcessualENT> tiposDeAtos = operacoes.BuscarTodosOsTiposDeAtosProcessuais();

            Debug.WriteLine( tiposDeAtos.Count());
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



        private string ExtraiIdDoArquivo(string nomeArquivo)
        {
            var match = Regex.Match(nomeArquivo, @"P(\d+)_");
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        private void ChamaScriptPython(string scriptPython, string arquivo, string diretorioPecasSumarizadas, string tipoResumo, string modelo)
        {
            // Preparar argumentos para o script Python
            Debug.WriteLine(arquivo);
            Debug.WriteLine(diretorioPecasSumarizadas);
            Debug.WriteLine(tipoResumo);
            Debug.WriteLine(modelo);
            var argsAI = new List<string> {
                    scriptPython,
                    arquivo,
                    diretorioPecasSumarizadas,
                    tipoResumo,
                    modelo};

            // Executar a aplicação Java com o script Python e argumentos
            var rodaAI = new RodaAplicacaoConsoleJava(
                "C:\\Arquivos de Programas\\Java\\jdk-20\\bin\\java.exe",
                "d:\\Java\\projetos\\gabia.pdf\\pdfutils-app\\target\\AppPyOpenAI_Sumariza.jar",
                argsAI
            );
            rodaAI.Run();
        }


        private void ChamaScriptPython(string scriptPython, string arquivo, string diretorioPecasSumarizadas)
        {
            // Preparar argumentos para o script Python
            var argsAI = new List<string> {
                scriptPython,
                arquivo,
                IOPath.Combine(diretorioPecasSumarizadas, "Resumos")};

                    // Executar a aplicação Java com o script Python e argumentos
                    var rodaAI = new RodaAplicacaoConsoleJava(
                        "C:\\Arquivos de Programas\\Java\\jdk-20\\bin\\java.exe",
                        "d:\\Java\\projetos\\gabia.pdf\\pdfutils-app\\target\\RodaAppPythonJson.jar",
                        argsAI
                    );
            rodaAI.Run();
        }

        public void ProcessarArquivosRegulariza(string diretorio, string diretorioPecas, string logDir, int nrPag)
        {
            // Carregar todos os arquivos no diretório
            var arquivos = Directory.GetFiles(diretorio);
            string diretorioPecasSumarizadas = IOPath.GetDirectoryName(diretorio);

            // Ordenar os arquivos por data de criação
            var arquivosOrdenados = arquivos.OrderBy(f => File.GetCreationTime(f)).ToList();

            int count = 0;
            string feitos = "";
            // Iterar em todos os arquivos
            foreach (var arquivo in arquivosOrdenados)
            {
                // Obter o tamanho do arquivo
                count++;

                feitos = $"Regularizados {count} de {arquivosOrdenados.Count()}";
                Debug.WriteLine(feitos);
                Debug.WriteLine(arquivo);
                if (File.Exists(arquivo))
                {
                    long tamanhoArquivo = new FileInfo(arquivo).Length;

                    // Exibir uma mensagem com o tamanho do arquivo
                    if (tamanhoArquivo < 200)
                    {
                        // Renomeie o arquivo e mova para o diretório de destino.
                        string novoNomeArquivo = IOPath.ChangeExtension(arquivo, ".rtf");
                        novoNomeArquivo = IOPath.GetFileName(novoNomeArquivo);
                        string dS = IOPath.Combine(diretorioPecasSumarizadas, "Fase_II");

                        string novoCaminhoArquivo = IOPath.Combine(dS, novoNomeArquivo);
                        if(!File.Exists(novoCaminhoArquivo)) File.Move(arquivo, novoCaminhoArquivo);
                    }
                    else if(tamanhoArquivo < 8000)
                    {
                        // Preparar argumentos para o script Python
                        var argsAI = new List<string> {
                            "d:\\PJe\\App_cpp\\openAI_organiza.py",
                            arquivo,
                            IOPath.Combine(diretorioPecasSumarizadas, "Fase_II")};

                        // Executar a aplicação Java com o script Python e argumentos
                        var rodaAI = new RodaAplicacaoConsoleJava(
                            "C:\\Arquivos de Programas\\Java\\jdk-20\\bin\\java.exe",
                            "d:\\Java\\projetos\\gabia.pdf\\pdfutils-app\\target\\RodaAppPythonJson.jar",
                            argsAI
                        );
                        rodaAI.Run();
                    }
                    else
                    {
                        ChamaScriptPython("d:\\PJe\\App_cpp\\openAI_sumarizaPar.py",
                                arquivo,
                                IOPath.Combine(diretorioPecasSumarizadas, "Fase_II"),
                                "corrige",
                                "gpt-3.5-turbo-16k");

                        /*/ Preparar argumentos para o script Python
                        var argsAI = new List<string> {
                            "d:\\PJe\\App_cpp\\openAI_organiza16.py",
                            arquivo,
                            IOPath.Combine(diretorioPecasSumarizadas, "Fase_II")};

                        // Executar a aplicação Java com o script Python e argumentos
                        var rodaAI = new RodaAplicacaoConsoleJava(
                            "C:\\Arquivos de Programas\\Java\\jdk-20\\bin\\java.exe",
                            "d:\\Java\\projetos\\gabia.pdf\\pdfutils-app\\target\\RodaAppPythonJson.jar",
                            argsAI
                        );
                        rodaAI.Run();
                        */
                    }
                }
            }
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
        public NeoGabView()
        {
            InitializeComponent();
            //LoadPdfFromResources();
        }

        private bool IsPageTextValid(string pageText)
        {
            //MessageBox.Show(pageText.Length + " Caracteres!" );
            // Implemente a lógica para verificar se o texto da página é válido (não contém apenas rodapé)
            // Exemplo: verifique o tamanho do texto, palavras-chave específicas ou padrões de rodapé
            return !string.IsNullOrEmpty(pageText) && pageText.Length > 50;
        }
    }
}

