using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using OpenQA.Selenium;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf;
using PdfSharpDocument = PdfSharp.Pdf.PdfDocument;
using PdfPigDocument = UglyToad.PdfPig.PdfDocument;
using Autofac;
using GabIA.ENT;
using GabIA.BLL;
using System.Text.RegularExpressions;
using OpenQA.Selenium.DevTools;
using PdfPigPage = UglyToad.PdfPig.Content.Page;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Geometry;
using System.Globalization;
using GabIA.WPF.ViewModels;
using log4net.Repository;
using log4net.Config;
using log4net;
using System.Reflection;
using System.Windows.Markup;
using GabIA.WPF.Views;
using GabIA.WPF.Models;
using Newtonsoft.Json;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Diagnostics;
using System.Security.Policy;
using Org.BouncyCastle.Tls;
using Avalonia.Media.TextFormatting.Unicode;
using UglyToad.PdfPig.Logging;
using Log4netILog = log4net.ILog;
using Tesseract;
using OpenQA.Selenium.DevTools.V109.Network;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Net.Sockets;
using NetMQ;
using NetMQ.Sockets;
using MySqlX.XDevAPI;



namespace GabIA.WPF
{
    public partial class MainWindow : Window
    {
        private int listViewIndex = 0;
        public string urlProcesso = "www.mpdft";
        private readonly ILifetimeScope? _scope;
        private readonly ProcessoBLL? _processoBLL;
        private static readonly Log4netILog log = LogManager.GetLogger(typeof(MainWindow));


        private PythonApiService _pythonApiService;

        public int processados = 0;

        private bool isNavigationTriggeredByCode = false;

        private NeoGabView _webViewControl1;
        private WebViewControl2 _webViewControl2;
        private WebViewControl3 _webViewControl3;
        private WebViewControl1 _webViewControl4;

        private bool shouldUpdateProcessCSV = false;

        private bool isMonitoringSuspended;

        private AtoProcessualConsultaBLL _atoProcessualBLL;

        private NeoGabView currentNeoGabView;
        private bool enteredIntranet = false;
        public string NumeroProcesso = "";

        private OperacoesBLL _operacoesBLL;

        public ProcessosView processosView { get; private set; }

        public static TextBox DadosDoProcessoBox { get; set; }


        public MainWindow(ILifetimeScope? scope, ProcessoBLL? processoBLL)
        {
            InitializeComponent();

            DataContext = new MainViewModel();

            _scope = scope;

            _processoBLL = processoBLL;

            _webViewControl1 = new NeoGabView();
            _webViewControl2 = new WebViewControl2();
            _webViewControl3 = new WebViewControl3();
            _webViewControl4 = new WebViewControl1();

            // Definindo os DataContexts
            _webViewControl1.DataContext = new NeoGabModel();
            _webViewControl2.DataContext = new WebViewControl2ViewModel();
            _webViewControl3.DataContext = new WebViewControl3ViewModel();
            _webViewControl4.DataContext = new WebViewControl1ViewModel();

            WebViewContentControl.Content = _webViewControl3;
            WebViewContentControlJusBrasil.Content = _webViewControl2;
            WebViewContentControlMigalhas.Content = _webViewControl4;

            // Cria uma instância do controle ProcessosView e define o DataContext
            processosView = new ProcessosView(this);
            processosView.ProcessoSelected += ProcessosView_ProcessoSelected;


            // Adicione ProcessosView ao ContentControl
            ProcessosContentControl.Content = processosView;

            // Adicionar essa linha para assegurar que o DataContext está sendo definido corretamente
            processosView.DataContext = new ProcessosViewModel();

            // Certificar-se de que o controle ProcessosView está sendo adicionado ao layout do MainWindow
            ProcessosContentControl.Content = processosView;


            InitializeAsync();

            DadosDoProcessoBox = DadosDoProcesso;

            pdfUserControl.ArquivoProcessado += PdfUserControl_ArquivoProcessado;

            ConectarAoServidorDeLogs();

            _pythonApiService = new PythonApiService();

            BuscarLogsDoServidorPythonAsync();


        }

        public MainWindow()
        {
            InitializeComponent();
        }


        private void LogRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Lógica para lidar com a mudança de texto
            // Por exemplo, você pode fazer algo a cada vez que o texto mudar
        }

        private async Task BuscarLogsDoServidorPythonAsync()
        {
            while (true)
            {
                if (!ServidorPythonAtivo()) // Verifica se o servidor está ativo
                {
                    // Tratamento caso o servidor não esteja ativo.
                    break;
                }

                try
                {
                    var logs = await _pythonApiService.ObterLogsAsync();
                    Dispatcher.Invoke(() =>
                    {
                        logRichTextBox.AppendText(logs + "\n");
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        logRichTextBox.AppendText($"Erro: {ex.Message}");
                    });
                }

                await Task.Delay(1000); // Aguarda um segundo antes de buscar novos logs
            }
        }

        private async Task BuscarLogsDoServidorPythonAsyncold()
        {
            while (true)
            {
                try
                {
                    var logs = await _pythonApiService.ObterLogsAsync(); // Uso da instância
                    Dispatcher.Invoke(() =>
                    {
                        logRichTextBox.AppendText(logs);
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Tratar exceções
                        logRichTextBox.AppendText($"Erro: {ex.Message}");
                    });
                }

                await Task.Delay(1000); // Aguarda um segundo antes de buscar novos logs
            }
        }

        


        private void ConectarAoServidorDeLogs()
        {
            Task.Run(() =>
            {
                try
                {
                    TcpClient client = new TcpClient("127.0.0.1", 65432);
                    using NetworkStream stream = client.GetStream();
                    byte[] data = new byte[256];
                    StringBuilder response = new StringBuilder();

                    while (client.Connected)
                    {
                        int bytes = stream.Read(data, 0, data.Length);
                        response.Clear();
                        response.Append(Encoding.ASCII.GetString(data, 0, bytes));
                        Dispatcher.Invoke(() =>
                        {
                            // Atualiza o RichTextBox com o log
                            logRichTextBox.AppendText(response.ToString());
                        });
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Tratar exceções
                        logRichTextBox.AppendText($"Erro: {ex.Message}");
                    });
                }
            });
        }


        private bool ServidorPythonAtivo()
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                // Tenta conectar ao servidor Python na porta 8000
                tcpClient.Connect("127.0.0.1", 8000);

                // Se a conexão for bem-sucedida, o servidor está ativo
                return true;
            }
            catch (Exception)
            {
                // Se ocorrer um erro ao tentar se conectar, o servidor provavelmente não está ativo
                return false;
            }
            finally
            {
                // Fecha a conexão TCP
                tcpClient.Close();
            }
        }

        private async void ProcessosView_ProcessoSelected(object sender, EventArgs e)
        {

            // Este código será executado quando um processo for selecionado em ProcessosView
            ProcessoCSV processoAtivo = Singleton.Instance.ProcessoAtivo;
            // agora você pode usar processoAtivo
            await DownloadPDF();
        }


        private void PdfUserControl_ArquivoProcessado(object sender, EventArgs e)
        {
            // Incremente a variável processados quando o evento ArquivoProcessado for disparado
            processados++;
        }

        private async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            //webView.CoreWebView2.NewWindowRequested += WebView_NewWindowRequested;
        }
        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (isNavigationTriggeredByCode)
            {
                isNavigationTriggeredByCode = false;
                return;
            }

            string currentUrl = webView.Source.AbsoluteUri;

            if (currentUrl == "https://intranet.mpdft.mp.br/portal/")
            {
                isNavigationTriggeredByCode = true;
                webView.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes");
            }
            else if (currentUrl == "https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes" && shouldUpdateProcessCSV)
            {
                await atualizaProcessCSV_New();
                shouldUpdateProcessCSV = false; // Reset the flag after calling the method
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Restante do código...
        }



        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            // Cancela as tarefas de monitoramento
            //_cts.Cancel();
        }

        public void EnviarMensagemParaPython(string mensagem)
        {
            using (var client = new RequestSocket(">tcp://localhost:65432"))
            {
                client.SendFrame(mensagem);
                string resposta = client.ReceiveFrameString();
                Console.WriteLine("Resposta recebida: " + resposta);
            }
        }

        private void Carrega_Processos_Button(object sender, RoutedEventArgs e)
        {
            var processosViewModel = processosView.DataContext as ProcessosViewModel;
            if (processosViewModel != null)
            {
                processosViewModel.LoadProcessosCSVFromDB();
                processosViewModel.Processos_DB = new ObservableCollection<ProcessoCSV>(processosViewModel.Processos_DB); // Aqui usamos os dados já carregados
            }
            else
            {
                // Trate o erro - DataContext não é um ProcessosViewModel
            }

            TogglePdfAndWebView(false, true, false, false);
        }



        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
                    // Se você deseja navegar para uma URL específica após a inicialização, você pode fazer isso aqui:
                    webView.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/portal/");
                }
            }
            else
            {
                // Trate o caso em que a inicialização do CoreWebView2 falhou
                MessageBox.Show("Falha ao inicializar o CoreWebView2. Verifique sua conexão com a internet e tente novamente.");
            }
        }

        private void TogglePdfAndWebView(bool showPdf, bool showProc, bool showGridDividido, bool showRichTextBox)
        {
            if (showPdf)
            {
                gridUserControl.Visibility = Visibility.Visible;
                processosView.Visibility = Visibility.Collapsed;
                gridIntranet.Visibility = Visibility.Collapsed;
                gridProcesso.Visibility = Visibility.Collapsed;
                gridRichTextBox.Visibility = Visibility.Collapsed;
            }
            else if (showProc)
            {
                gridUserControl.Visibility = Visibility.Collapsed;
                gridIntranet.Visibility = Visibility.Collapsed;
                processosView.Visibility = Visibility.Visible;
                gridProcesso.Visibility = Visibility.Collapsed;
                gridRichTextBox.Visibility = Visibility.Collapsed;
            }
            else if (showGridDividido)
            {
                gridUserControl.Visibility = Visibility.Collapsed;
                gridIntranet.Visibility = Visibility.Collapsed;
                processosView.Visibility = Visibility.Collapsed;
                gridProcesso.Visibility = Visibility.Visible;
                gridRichTextBox.Visibility = Visibility.Collapsed;
            }
            else if (showRichTextBox)
            {
                gridUserControl.Visibility = Visibility.Collapsed;
                gridIntranet.Visibility = Visibility.Collapsed;
                processosView.Visibility = Visibility.Collapsed;
                gridProcesso.Visibility = Visibility.Collapsed;
                gridRichTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                gridUserControl.Visibility = Visibility.Collapsed;
                gridIntranet.Visibility = Visibility.Visible;
                processosView.Visibility = Visibility.Collapsed;
                gridProcesso.Visibility = Visibility.Collapsed;
                gridRichTextBox.Visibility = Visibility.Collapsed;
            }
        }


        private void BlueView_Clicked(object sender, RoutedEventArgs e)
        {
            DataContext = new NeoGabModel();
        }

        private void OrangeView_Clicked(object sender, RoutedEventArgs e)
        {

        }

        public void Dispose()
        {
            _scope?.Dispose();
        }

        private void CarregarProcessos()
        {
            // Carregar os dados dos processos no DataGrid
            if (_processoBLL != null)
            {
                //dataGrid.ItemsSource = _processoBLL.ListarProcessos();
            }
        }

        private void AdicionarProcessoButton_Click(object sender, RoutedEventArgs e)
        {
            // Adicionar um novo processo
        }

        private void EditarProcessoButton_Click(object sender, RoutedEventArgs e)
        {
            // Editar o processo selecionado
        }

        private void ExcluirProcessoButton_Click(object sender, RoutedEventArgs e)
        {
            // Excluir o processo selecionado
        }

        private void MaxBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            // Criar uma instância da janela AboutWindow
            AboutWindow aboutWindow = new AboutWindow();

            // Configurar a janela para ser uma janela de diálogo modal
            aboutWindow.Owner = this;
            aboutWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Exibir a janela AboutWindow
            aboutWindow.ShowDialog();
        }
 

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
        }
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Chat_Button_Click(object sender, RoutedEventArgs e)
        {
 
        }

        private void ProcessListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProcessListView.SelectedItem != null)
            {
                urlProcesso = FormatURLWithProcessNumber(ProcessListView.SelectedItem.ToString());
            }
        }


        private async void Res3_Button_Click(object sender, RoutedEventArgs e)
        {
            //await ExpandAllPanelsAsync();


            //await ClickMenusAsync();


            //await ExtractPanelsAsync(_webViewControl1.WebViewCore);


            //await ExpandTableAndNavigatePages();

        }

        public async Task ExtractPanelsAsync(CoreWebView2 webView)
        {
            try
            {
                // Crie o diretório se não existir
                string directoryPath = @"f:\classes\pagina\painel";
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                for (int i = 1; i < 50; i++)
                {
                    try
                    {
                        // Obtenha o conteúdo do painel
                        string panelHeader = await webView.ExecuteScriptAsync($"document.querySelector('#mat-expansion-panel-header-{i}')?.innerText");
                        string panelContent = await webView.ExecuteScriptAsync($"document.querySelector('#cdk-accordion-child-{i} > div > div')?.innerText");

                        if (panelHeader != "null")
                        {
                            // Crie o nome do arquivo e o caminho
                            string fileName = $"painel_{i}.txt";
                            string filePath = Path.Combine(directoryPath, fileName);

                            // Verifique se o painel não está vazio
                            if (panelContent != "null" && !string.IsNullOrEmpty(panelContent))
                            {
                                // Grave o conteúdo do painel no arquivo
                                File.WriteAllText(filePath, $"{panelHeader}:\n{panelContent}");
                            }
                            else
                            {
                                // Grave a sinalização de painel vazio no arquivo
                                File.WriteAllText(filePath, $"{panelHeader}:\n[Painel vazio]");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Registre outras exceções e continue com o próximo painel
                        MessageBox.Show($"Erro inesperado ao processar o painel {i}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Registre a exceção e termine a função
                MessageBox.Show($"Erro inesperado ao extrair painéis: {ex.Message}");
            }
        }


 

        //private void btnSite1_Click(object sender, RoutedEventArgs e)
        //{
        //    DataContext = new WebViewControl1ViewModel();

        //}
        private void btnSite1_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new WebViewControl1ViewModel();
            DataContext = viewModel;

            // Definir a visibilidade do WebView2
            azureView.Visibility = Visibility.Visible;

            // Carregar a URL no WebView2
            azureView.Source = new Uri("https://www.conjur.com.br");
        }

        private void btnSite2_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new WebViewControl2ViewModel();
            DataContext = viewModel;

            // Definir a visibilidade do WebView2
            azureView.Visibility = Visibility.Visible;

            // Carregar a URL no WebView2
            azureView.Source = new Uri("https://www.jusbrasil.com.br");
        }

        private void btnSite3_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new WebViewControl3ViewModel();
            DataContext = viewModel;

            // Definir a visibilidade do WebView2
            azureView.Visibility = Visibility.Visible;

            // Carregar a URL no WebView2
            azureView.Source = new Uri("https://www.Migalhas.com.br");
        }

        private async void Process_Button_Click(object sender, RoutedEventArgs e)
        {


            //// Solicita o download de todos os processos
            //currentNeoGabView.RequestDownloadProcessosConsulta();
        }

        private void Home_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NeoGabView_Clicked(object sender, RoutedEventArgs e)
        {
            TogglePdfAndWebView(false,false,false,false);
            if(!enteredIntranet)
            {
                webView.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes");
                enteredIntranet = true;
            }
        }

        private async void atualizaProcessCSV(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            // Verifique se a URL atual é "intranet.mpdft.mp.br/novoGabinete"
            string currentUrl = webView.Source.AbsoluteUri;
            if (currentUrl != "https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes")
            {
                webView.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes");
                MessageBox.Show("Aguarde NeoGab->Intimações","Carregar Processos com Vista");
                return;
            }

            if (currentUrl == "https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes")
            {

                //aguarda 15 segundos antes de começar 
                int cont = 0;
                int totalRows = 0;

                do
                {
                    //await Task.Delay(5000);
                    totalRows = await GetTotalRowsAsync();
                    cont++;

                } while (totalRows <= 0 && cont < 12);

                if (cont > 11 || totalRows <= 0)
                {
                    return;
                }

                List<List<string>> allData = new List<List<string>>();
                int rowsPerPage = 5;
                int totalPages = (int)Math.Ceiling(totalRows / (double)rowsPerPage);

                for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
                {
                    // Extrair dados e adicionar a allData
                    allData.AddRange(await ExtractDataFromTable());

                    // Navegue para a próxima página clicando no botão, se não for a última página
                    if (pageIndex < totalPages - 1)
                    {
                        string clickScript = "document.querySelector('#mat-tab-content-0-0 > div > agrupamento-intimacoes > article > " +
                            "div:nth-child(3) > mat-paginator > div > div > div.mat-paginator-range-actions > " +
                            "button.mat-focus-indicator.mat-tooltip-trigger.mat-paginator-navigation-next.mat-icon-button.mat-button-base').click();";

                        await webView.ExecuteScriptAsync(clickScript);
                        for(int contTemp = 1; contTemp < 1000000; contTemp++)
                        {
                            int tempVar = 1;
                            tempVar = tempVar + contTemp;
                            tempVar = tempVar - contTemp;
                            double duplo = 0.10;
                            duplo = tempVar / contTemp;
                        }
                        //await Task.Delay(2000); // Aguarde um pouco para a próxima página ser carregada
                    }
                }

                string folderPath = @"D:\PJe\Dados";
                string fileName = GetNextFileName(folderPath);
                WriteDataToCSV(allData, fileName, totalRows);

                //}

            }
        }


        private async Task<List<List<string>>> ExtractDataFromTable()
        {
            string script = @"
                            (() => {
                                try {
                                    const tbody = document.querySelector('#mat-tab-content-0-0 > div > agrupamento-intimacoes > article > div:nth-child(2) > table > tbody');
                                    if (tbody) {
                                        return Array.from(tbody.querySelectorAll('tr')).map(row => {
                                            return [
                                                row.children[2] ? row.children[2].innerText : '',
                                                row.children[3] ? row.children[3].innerText : '',
                                                row.children[4] ? row.children[4].innerText : '',
                                                row.children[5] ? row.children[5].innerText : '',
                                                row.children[6] ? row.children[6].innerText : '',
                                                row.children[7] ? row.children[7].innerText : '',
                                                row.children[8] ? row.children[8].innerText : '',
                                                row.children[9] ? row.children[9].innerText : ''
                                            ];
                                        });
                                    } else {
                                        return [];
                                    }
                                } catch (error) {
                                    return { error: error.message };
                                }
                            })();";

            string resultJson = await webView.ExecuteScriptAsync(script);
            var result = JsonConvert.DeserializeObject(resultJson);

            if (result is JObject jObj && jObj.ContainsKey("error"))
            {
                //MessageBox.Show($"Ocorreu um erro ao executar o script JavaScript: {jObj["error"]}");
                return new List<List<string>>();
            }
            else
            {
                List<List<string>> rowsData = JsonConvert.DeserializeObject<List<List<string>>>(resultJson);
                return rowsData ?? new List<List<string>>();
            }
        }


        private async Task<int> GetTotalRowsAsync()
        {
            // Aguarde um pouco para garantir que a página esteja totalmente carregada
            //await Task.Delay(2000);

            string script = @"
                            (() => {
                                const element = document.querySelector('.mat-tab-label-content');
                                return element ? element.innerText : null;
                            })();";

            string totalRowsText = await webView.ExecuteScriptAsync(script);

            if (!string.IsNullOrEmpty(totalRowsText))
            {
                Match match = Regex.Match(totalRowsText, @"\d+");
                if (match.Success)
                {
                    return int.Parse(match.Value);
                }
            }
            return 0;
        }

        // Método de manipulação de evento
        private async void WebView_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            // Impedir que o WebView2 padrão abra a janela pop-up
            e.Handled = true;

            // Criar um novo WebView2 e configurá-lo conforme necessário
            var newWebView = new Microsoft.Web.WebView2.Wpf.WebView2();
            newWebView.Source = new Uri(e.Uri); // Definir a URL da janela pop-up

            // Inicialize o WebView2
            await newWebView.EnsureCoreWebView2Async();

            // Inscrever-se no evento NavigationCompleted do novo WebView2
            newWebView.CoreWebView2.NavigationCompleted += async (sender, args) =>
            {
                if (args.IsSuccess)
                {
                    // A navegação foi concluída com sucesso
                    await ClickButtonGeneratePdf();
                }
            };

            // Adicione o novo WebView2 à sua interface do usuário conforme desejado
            // Por exemplo, você pode adicionar o novo WebView2 a um novo formulário ou painel
            // ...
        }

        private async Task NeoGabNavigate(string url)
        {
            // Crie uma instância de TaskCompletionSource que será usada para aguardar a conclusão da navegação.
            var navigationCompletedTcs = new TaskCompletionSource<object>();

            // Crie um manipulador de eventos temporário que sinalizará o TaskCompletionSource quando a navegação for concluída.
            EventHandler<CoreWebView2NavigationCompletedEventArgs> tempHandler = (sender, e) =>
            {
                if (e.IsSuccess)
                {
                    // A navegação foi concluída, sinalize o TaskCompletionSource.
                    navigationCompletedTcs.TrySetResult(null);
                }
                else
                {
                    // A navegação falhou, sinalize o TaskCompletionSource com uma exceção.
                    navigationCompletedTcs.TrySetException(new Exception("Navigation failed"));
                }
            };

            // Adicione o manipulador de eventos temporário.
            webView.CoreWebView2.NavigationCompleted += tempHandler;

            // Navegue para a URL especificada.
            webView.CoreWebView2.Navigate(url);

            // Aguarde a conclusão da navegação.
            await navigationCompletedTcs.Task;

            // Remova o manipulador de eventos temporário.
            webView.CoreWebView2.NavigationCompleted -= tempHandler;
        }
        private async Task<bool> IsDocumentLoadedAsync()
        {
            string script = @"
                (() => {
                    // Altere isso de acordo com os elementos que indicam que o documento está carregado
                    let loadedIndicator = document.querySelector('#some-loaded-indicator');
                    return loadedIndicator !== null;
                })();";
            string isLoaded = await webView.ExecuteScriptAsync(script);
            return bool.Parse(isLoaded);
        }

        private async Task<bool> ClickDocumentAsync()
        {
            string script = @"
                (() => {
                    let documentSelector = '#cdk-accordion-child-8 > div > div > div:nth-child(2) > mat-table > mat-row:nth-child(2) > mat-cell.mat-cell.cdk-cell.cdk-column-id.mat-column-id.ng-star-inserted > span.mat-tooltip-trigger';
                    let documentElement = document.querySelector(documentSelector);
                    if (documentElement) {
                        documentElement.click();
                        return true;
                    }
                    return false;
                })();";
            string clickedString = await webView.ExecuteScriptAsync(script);
            bool clicked = bool.Parse(clickedString);
            return clicked;
        }


        private async Task navegaParaProcesso(string urlProcesso)
        {
            webView.CoreWebView2.Navigate(urlProcesso);

            await Task.Delay(1000);

            //Dispatcher.Invoke(() => { }, DispatcherPriority.Background);
        }


        private async Task ClickButtonGeneratePdf()
        {
            // Clique no botão para abrir o menu
            await webView.ExecuteScriptAsync("document.querySelector('#conteudo > visualizador > div > visualizador-template > div > div.acoes.ng-star-inserted > div > menu-acao-feito > icone-acoes > button > span.mat-button-wrapper > mat-icon').click();");

            // Aguarde a animação do menu
            await Task.Delay(500);

            // Clique na opção 'Gerar PDF'
            await webView.ExecuteScriptAsync("document.querySelector('#mat-menu-panel-12 > div > div > acao-gerar-pdf > opcao-acao > button > span').click();");

            // Aguarde a caixa de diálogo abrir
            await Task.Delay(500);

            // Clique no botão para confirmar e baixar o arquivo PDF
            await webView.ExecuteScriptAsync("document.querySelector('#mat-dialog-1 > carregar-pdf > mat-dialog-actions > button:nth-child(2) > span.mat-button-wrapper').click();");
            MessageBox.Show("Gerando o PDF do Processo");
        }

        private static string RemoveUnwantedCharacters(string numeroProcesso)
        {
            return numeroProcesso.Replace(".", "").Replace("-", "");
        }


        private static string FormatURLWithProcessNumber(string numeroProcesso)
        {
            string baseURL = "https://intranet.mpdft.mp.br/sistemas/java/neogab/visualizador/processo/";
            string urlParameters = "?origemInstancia=1";
            return baseURL + numeroProcesso + urlParameters;
        }
        private string TreatText(string input)
        {
            // Inserir um separador antes de "Polo passivo:"
            //input = input.Replace("Polo passivo:", "|Polo passivo:");
            // Remove caracteres indesejados após 'Consulta Eletrônica:'
            input = Regex.Replace(input, @"(?<=Consulta Eletrônica:)[^\d\s]", "");

            // Remove a sequência "\n" após "Consulta Eletrônica:"
            input = input.Replace("Consulta Eletrônica:\n", "Consulta Eletrônica: ");
            input = input.Replace("\nPolo passivo:", " |Polo passivo: ");

            // Remover caracteres especiais antes de "Consulta Eletrônica:" e "Polo passivo:"
            //input = Regex.Replace(input, @"(?<=Consulta Eletrônica:)[^\d\s\\n]", "");
            //input = Regex.Replace(input, @"[^\w\s](?=Polo passivo:)", "");

            return input;
        }

        private string CleanText(string input)
        {
            // Tratar o texto antes de limpá-lo
            input = TreatText(input);

            // Remover espaços, tabulações e quebras de linha em excesso
            return Regex.Replace(input.Trim(), @"\s{2,}", " ");
        }

        private void WriteDataToCSV(List<List<string>> data, string fileName, int totalDeLinhas)
        {
            using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                //escreve o csv
                sw.WriteLine("ID|Processo|DataDaAbertura|PrazoParaConsulta|PoloAtivo|PoloPassivo|Classe|MembroResponsavel|Promotoria");
                foreach (List<string> rowData in data)
                {
                    // Verifica se rowData contém pelo menos um elemento não vazio
                    if (rowData.Any(item => !string.IsNullOrEmpty(item)))
                    {
                        // Verifica se PoloPassivo é vazio
                        if (string.IsNullOrEmpty(rowData[5]))
                        {
                            rowData[5] = "Polo Passivo: NÃO HÁ";
                        }

                        List<string> cleanedRowData = rowData.Select(item => CleanText(item).Replace("\"", "")).ToList();
                        sw.WriteLine(string.Join("|", cleanedRowData));
                    }
                }
            }
        }

        private void LimpaTreeViewPdfTexto()
        {
            // Limpe os nós existentes no TreeView
            treeViewProcesso.Items.Clear();

            // Criar o nó raiz do TreeView
            TreeViewItem rootNode = new TreeViewItem();
            rootNode.Header = "Atos Processuais";
            rootNode.IsExpanded = true; // Define o nó raiz como expandido
            treeViewProcesso.Items.Add(rootNode);
            textBoxResumo.Document.Blocks.Clear();
            textBoxConteudo.Document.Blocks.Clear();

        }


        private void CarregarTreeViewData()
        {
            ProcessoCSV processoAtivo = Singleton.Instance.ProcessoAtivo;

            OperacoesBLL _operacoesBLL = new OperacoesBLL();

            var tiposAtoProcessual = _operacoesBLL.GetTiposAtoProcessual();

            var tipoAtoProcessualMap = tiposAtoProcessual.ToDictionary(t => t.Id_Tipo_Ato_processual, t => t.Tipo.ToLower());

            List<AtoProcessualConsulta> atosProcessuais = _atoProcessualBLL.ObterListaAtoProcessualPorIdProcessoJ(processoAtivo.idPJ);

            treeViewProcesso.Items.Clear();

            TreeViewItem rootNode = new TreeViewItem();
            rootNode.Header = "Atos Processuais";
            rootNode.IsExpanded = true;
            treeViewProcesso.Items.Add(rootNode);

            var atosComDescricaoTipo = atosProcessuais.Select(ap => new
            {
                AtoProcessual = ap,
                TipoDescricao = tipoAtoProcessualMap.ContainsKey(ap.Tipo) ? tipoAtoProcessualMap[ap.Tipo] : ap.Tipo,
                Data = ap.DataInclusao.ToString("yyyy-MM-dd") // Adicionando a data no formato ano-mes-dia
            });

            var atosPorTipoEData = atosComDescricaoTipo.GroupBy(a => new { a.TipoDescricao, a.Data });

            foreach (var grupo in atosPorTipoEData)
            {
                TreeViewItem tipoNode = new TreeViewItem();

                var tipoAtoProcessualDescr = tiposAtoProcessual.FirstOrDefault(t => t.Id_Tipo_Ato_processual.ToString() == grupo.Key.TipoDescricao);

                tipoNode.Header = $"{tipoAtoProcessualDescr.Tipo.ToLower()} - {grupo.Key.Data}"; // Adicionando a data no cabeçalho do nó
                rootNode.Items.Add(tipoNode);

                foreach (var ato in grupo)
                {
                    TreeViewItem atoNode = new TreeViewItem();
                    atoNode.Header = ato.AtoProcessual.IdMovimento;
                    Debug.WriteLine(ato.AtoProcessual.IdMovimento);
                    atoNode.Tag = ato.AtoProcessual;
                    tipoNode.Items.Add(atoNode);
                }
            }
            Debug.WriteLine(processoAtivo.PoloAtivo);
        }



        private void CarregarTreeViewGrupo()
        {

            ProcessoCSV processoAtivo = Singleton.Instance.ProcessoAtivo;

            OperacoesBLL _operacoesBLL = new OperacoesBLL();

            // Obter todos os tipos de ato processual
            var tiposAtoProcessual = _operacoesBLL.GetTiposAtoProcessual();

            // Criar um dicionário que mapeia o ID do tipo de ato processual para a descrição do tipo
            var tipoAtoProcessualMap = tiposAtoProcessual.ToDictionary(t => t.Id_Tipo_Ato_processual, t => t.Tipo.ToLower());

            List<AtoProcessualConsulta> atosProcessuais = _atoProcessualBLL.ObterListaAtoProcessualPorIdProcessoJ(processoAtivo.idPJ);

            // Limpar os nós existentes no TreeView
            treeViewProcesso.Items.Clear();

            // Criar o nó raiz do TreeView
            TreeViewItem rootNode = new TreeViewItem();
            rootNode.Header = "Atos Processuais";
            rootNode.IsExpanded = true; // Define o nó raiz como expandido
            treeViewProcesso.Items.Add(rootNode);

            // Definir o ItemTemplate para o nó raiz
            //rootNode.ItemTemplate = itemTemplate;

            var atosComDescricaoTipo = atosProcessuais.Select(ap => new
            {
                AtoProcessual = ap,
                TipoDescricao = tipoAtoProcessualMap.ContainsKey(ap.Tipo) ? tipoAtoProcessualMap[ap.Tipo] : ap.Tipo
            });

            var atosPorTipo = atosComDescricaoTipo.GroupBy(a => a.TipoDescricao);

            foreach (var grupo in atosPorTipo)
            {
                TreeViewItem tipoNode = new TreeViewItem();

                var tipoAtoProcessualDescr = tiposAtoProcessual.FirstOrDefault(t => t.Id_Tipo_Ato_processual.ToString() == grupo.Key);


                tipoNode.Header = tipoAtoProcessualDescr.Tipo.ToLower(); 
                rootNode.Items.Add(tipoNode);

                foreach (var ato in grupo)
                {
                    TreeViewItem atoNode = new TreeViewItem();
                    atoNode.Header = ato.AtoProcessual.IdMovimento;
                    Debug.WriteLine(ato.AtoProcessual.IdMovimento);
                    atoNode.Tag = ato.AtoProcessual; // Armazenar o objeto AtoProcessual no item
                    tipoNode.Items.Add(atoNode);
                }
            }
            Debug.WriteLine(processoAtivo.PoloAtivo);
            //Debug.WriteLine($"incluídos {i} grupos e {f} itens");
            DadosDoProcesso.Text = "Processo\n0703046-52.2022.8.07.0021";
        }
        private string GetNextFileName(string folderPath)
        {
            int currentMax = 0;

            var files = Directory.GetFiles(folderPath, "dados_tabela_*.txt");

            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string[] splitName = fileName.Split('_');

                if (splitName.Length == 3)
                {
                    if (int.TryParse(splitName[2], out int fileNumber))
                    {
                        if (fileNumber > currentMax)
                        {
                            currentMax = fileNumber;
                        }
                    }
                }
            }
            return Path.Combine(folderPath, $"dados_tabela_{(currentMax + 1).ToString("D3")}.txt");
        }

        private string GetLastFileName(string folderPath)
        {
            int currentMax = 0;

            var files = Directory.GetFiles(folderPath, "dados_tabela_*.txt");

            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string[] splitName = fileName.Split('_');

                if (splitName.Length == 3)
                {
                    if (int.TryParse(splitName[2], out int fileNumber))
                    {
                        if (fileNumber > currentMax)
                        {
                            currentMax = fileNumber;
                        }
                    }
                }
            }
            return Path.Combine(folderPath, $"dados_tabela_{(currentMax).ToString("D3")}.txt");
        }

 
        private async Task MoveDownloadedPdfToDestinationFolder()
        {
            // Verifique se o CoreWebView2 está inicializado.
            if (webView.CoreWebView2 == null)
            {
                MessageBox.Show("WebView2 não está inicializado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string downloadsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string destinationFolderPath = @"D:\PJe\Baixados";

            DirectoryInfo downloadsFolder = new DirectoryInfo(downloadsFolderPath);

            var pdfFiles = downloadsFolder.GetFiles("*.pdf")
                .Where(f => Regex.IsMatch(f.Name, @"^\d{7}-\d{2}\.\d{4}\.\d\.\d{2}\.\d{4}.*\(TJDFT - PJe1.*\.pdf$", RegexOptions.IgnoreCase))
                .ToList();

            if (pdfFiles.Count > 0)
            {
                // Verifique se a pasta de destino existe. Se não, crie-a.
                if (!Directory.Exists(destinationFolderPath))
                {
                    Directory.CreateDirectory(destinationFolderPath);
                }

                // Mova todos os arquivos PDF para a pasta de destino e renomeie-os.
                foreach (FileInfo pdfFile in pdfFiles)
                {
                    processados ++;
                    string newFileName = $"{pdfFile.Name.Substring(0, 25)}.pdf";
                    string destinationFilePath = Path.Combine(destinationFolderPath, newFileName);
                    File.Move(pdfFile.FullName, destinationFilePath, true);

                    // Exclua o arquivo original da pasta de downloads.
                    pdfFile.Delete();
                }
            }
            else
            {
                MessageBox.Show("Nenhum arquivo PDF encontrado na pasta de Downloads do WebView2.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PDF_View_Clicked(object sender, RoutedEventArgs e)
        {
            processados = 0;
            MessageBox.Show("Iniciando a conversão dos Processos PDF para Texto.\n\nAguarde a mensagem de término!", "Processamento Automatizado");
            await MoveDownloadedPdfToDestinationFolder();
 //           pdfUserControl.separaPecasPDF();

            MessageBox.Show($"Terminou a conversão.\n\nArquivos processados: {processados.ToString()}!", "Processamento Automatizado");
        }

        private async void CarregaProcessos_Clicked(object sender, RoutedEventArgs e)
        {
            //atualizaProcessCSV(sender, null);
            await atualizaProcessCSV_New();
            TogglePdfAndWebView(false, false, false, false);
            //webView.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes");
        }
            
        private async void Navega_Button_Click(object sender, RoutedEventArgs e)
        {
            //abre o assistente
            // Desligar todos os outros grids
            gridUserControl.Visibility = Visibility.Collapsed;
            gridIntranet.Visibility = Visibility.Collapsed;
            processosView.Visibility = Visibility.Collapsed;
            gridProcesso.Visibility = Visibility.Collapsed;

            // Mostrar o grid do RichTextBox
            gridRichTextBox.Visibility = Visibility.Visible;

            PreprocessamentoTextoIA sumarizaParalelo = new PreprocessamentoTextoIA();

            sumarizaParalelo.ProcessarDiretorioEmParalelo(@"d:\PJe\Processos\0702153-66.2023.8.07.0008\ModeloDeLinguagem\Resumos", 
                            "", 
                            "0702153-66.2023.8.07.0008.Json");

            MessageBox.Show("Processado!!");

            if (File.Exists(@"d:\PJe\Processos\0702153-66.2023.8.07.0008\Analise Processual\entrada\Json\0702153-66.2023.8.07.0008.Json"))
            {
                
                    string texto = File.ReadAllText(@"d:\PJe\Processos\0702153-66.2023.8.07.0008\Analise Processual\entrada\Json\0702153-66.2023.8.07.0008.Json");
                // Ler o conteúdo do arquivo de texto
                //string conteudo = File.ReadAllText(nomeArquivoTexto);

                richTextVisualizer.FontFamily = new FontFamily("Arial");
                richTextVisualizer.FontSize = 14;

                // Preenche o RichTextBox com o conteúdo do arquivo
                richTextVisualizer.Document.Blocks.Clear();
                richTextVisualizer.Document.Blocks.Add(new Paragraph(new Run(texto)));
            };

        }


        private async Task<bool> TryClickTransformPdfButtonAsync()
        {
            // Tentativa 1: Clique no botão usando XPath
            bool clicked = await ClickGerarPdfWithXPathAsync();
            if (clicked) return true;

            // Tentativa 2: Clique no botão usando Selector
            clicked = await ClickGerarPdfWithSelectorAsync();
            if (clicked) return true;

            // Tentativa 3: Clique no botão usando FullPath
            clicked = await ClickGerarPdfWithFullPathAsync();
            if (clicked) return true;

            // Retorna false se todas as tentativas falharem
            return false;
        }


        private async void Res2_Button_Click(object sender, RoutedEventArgs e)
        {
            int processados = 0;
            
            //MessageBox.Show("Iniciando a conversão dos Processos PDF para Texto.\n\nAguarde a mensagem de término!", "Processamento Automatizado");
            // Primeiro, carregue a página
            await webView.EnsureCoreWebView2Async(null);

            // Execute o script para clicar no primeiro botão
            await webView.CoreWebView2.ExecuteScriptAsync(
                @"document.querySelector('button[mattooltip=""Gerar PDF""]').click();"
            );

            // Aguarde um tempo para a caixa de diálogo abrir
            await Task.Delay(1000);

            // Clique no segundo botão
            await webView.CoreWebView2.ExecuteScriptAsync(
                @"document.querySelector('button:contains(""Gerar PDF"")').click();"
            );
        }
        private string GetUserDownloadsFolderPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }


        private async void Separa_Pecas_PDF_Button_Click(object sender, RoutedEventArgs e)
        {
            int processados = 0;

            await webView.CoreWebView2.ExecuteScriptAsync(
                @"document.querySelector('button[mattooltip=""Gerar PDF""]').click();"
            );

            // Aguarde um tempo para a caixa de diálogo abrir
            await Task.Delay(1000);

            // Clique no segundo botão
            await webView.CoreWebView2.ExecuteScriptAsync(
                @"document.querySelector('button:contains(""Gerar PDF"")').click();"
            );

            webView.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/visualizador/processo/00016007020178070008?origemInstancia=1");


            MessageBox.Show("Iniciando a conversão dos Processos PDF para Texto.\n\nAguarde a mensagem de término!", "Processamento Automatizado");
            await MoveDownloadedPdfToDestinationFolder();

            pdfUserControl.separaPecasPDF();

            MessageBox.Show($"Terminou a conversão.\n\nArquivos processados: {processados.ToString()}!", "Processamento Automatizado");
        }

        private async void NavegaNeogab_Clicked(object sender, RoutedEventArgs e)
        {
            TogglePdfAndWebView(false, true, false, false);
        }


        private async Task atualizaProcessCSV_New()
        {
            shouldUpdateProcessCSV = true;

            isNavigationTriggeredByCode = true; 

            string currentUrl = webView.Source.AbsoluteUri;
            if (currentUrl == "https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes" && shouldUpdateProcessCSV)
            {
                shouldUpdateProcessCSV = false;
            }

            int cont = 0;
            int totalRows = 0;

            do
            {
                totalRows = await GetTotalRowsAsync();
                cont++;
            } while (totalRows <= 0 && cont < 12);

            if (cont > 11 || totalRows <= 0)
            {
                return;
            }

            List<List<string>> allData = new List<List<string>>();
            int rowsPerPage = 5;
            int totalPages = (int)Math.Ceiling(totalRows / (double)rowsPerPage);

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                allData.AddRange(await ExtractDataFromTable());

                if (pageIndex < totalPages - 1)
                {
                    string clickScript = "document.querySelector('#mat-tab-content-0-0 > div > agrupamento-intimacoes > article > " +
                        "div:nth-child(3) > mat-paginator > div > div > div.mat-paginator-range-actions > " +
                        "button.mat-focus-indicator.mat-tooltip-trigger.mat-paginator-navigation-next.mat-icon-button.mat-button-base').click();";

                    await webView.ExecuteScriptAsync(clickScript);
                    await Task.Delay(1000);
                }
            }

            string folderPath = @"D:\PJe\Dados";
            //string fileNameTemp = @"D:\PJe\Dados\dados_tabela_030.txt";
            string fileName = GetNextFileName(folderPath);
            WriteDataToCSV(allData, fileName, totalRows);

            var operacoesBLL = new OperacoesBLL();
            operacoesBLL.LoadDataFromCSV(fileName);

            ProcessListView.Items.Clear();

            foreach (List<string> row in allData)
            {
                // Verifique se a linha tem pelo menos 2 elementos
                if (row.Count >= 2)
                {
                    // Extraia o elemento no índice 1 (número do processo)
                    string proc = row[1];

                    // Adicione o número do processo diretamente à lista
                    ProcessListView.Items.Add(proc);
                }
            }
        }

        private void AddProcessItemToList(string processNumber)
        {
            ProcessItem processItem = new ProcessItem { ProcessNumber = processNumber };
            ProcessListView.Items.Add(processItem);
            listViewIndex = 0; // Zerar a variável listViewIndex
        }


        public async Task DownloadPDF(string numeroProcesso)
        {
            urlProcesso = FormatURLWithProcessNumber(numeroProcesso);

            NeoGabNavigate(urlProcesso);

        }

        private async void Redigir_Clicked(object sender, RoutedEventArgs e)
        {
            string argumento1 = @"d:\PJe\Processos\0001085-35.2017.8.07.0008";
            string argumento2 = @"d:\PJe\Processos\0001085-35.2017.8.07.0008\Doc";
            //argumento2 = "";
            //await ExecutarAplicacaoConsole_teste("d:\\PJe\\Processos\\0001085-35.2017.8.07.0008", "d:\\PJe\\Processos\\0001085-35.2017.8.07.0008\\Doc");
            await ExecutarAplicacaoConsole_teste(argumento1, argumento2);


        }
        public async Task ExecutarAplicacaoConsole_teste(string argumento1, string argumento2)
        {
            string caminhoAplicacao = @"D:\ExportDOCX\ExportPDFToDocx.exe";
            //caminhoAplicacao = @"D:\Usuarios\dotNET\RodandoAplicacaoRemota\RodandoAplicacaoRemota\bin\Debug\net7.0\RodandoAplicacaoRemota.exe";
            string argumentos = $"{argumento1} {argumento2}";

            MessageBox.Show(argumentos);

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
                int i = 0;
                do
                {
                    i++;
                    processo.Start();
                } while (i<100);
                

                // Insira o tempo de espera desejado em milissegundos
                int tempoDeEspera = 5000; // 5 segundos, por exemplo
                await Task.Delay(tempoDeEspera);

                // Se você quiser garantir que o processo termine antes de continuar, descomente a linha abaixo
                // processo.WaitForExit();
            }
            catch (Exception ex)
            {
                // Faça algo com a exceção, como exibir uma mensagem de erro
                MessageBox.Show($"Ocorreu um erro ao executar a aplicação console: {ex.Message}");
            }
        }



        public async Task ExecutarAplicacaoConsole_bichado(string argumento1, string argumento2)
        {
            string caminhoAplicacao = @"D:\Usuarios\dotNET\ExportDOCX\ExportPDFToDocx\bin\Debug\net6.0\ExportPDFToDocx.exe";
            string argumentos = $"{argumento1} {argumento2}";
            if (File.Exists(caminhoAplicacao))
            {
                MessageBox.Show(caminhoAplicacao + " Existe");
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
                    int tempoDeEspera = 5000; // 5 segundos, por exemplo
                    await Task.Delay(tempoDeEspera);

                    // Se você quiser garantir que o processo termine antes de continuar, descomente a linha abaixo
                    // processo.WaitForExit();
                }
                catch (Exception ex)
                {
                    // Faça algo com a exceção, como exibir uma mensagem de erro
                    MessageBox.Show($"Ocorreu um erro ao executar a aplicação console: {ex.Message}");
                }
            }
        }

        private async void Separa_Pecas_PDF_Clicked(object sender, RoutedEventArgs e)
        {
             int processados = 0;

            MessageBox.Show("Iniciando a conversão dos Processos PDF para Texto.\n\nAguarde a mensagem de término!", "Processamento Automatizado");
            
            await MoveDownloadedPdfToDestinationFolder();

            //AzureOpenai azureOpenai = new AzureOpenai();
            //await azureOpenai.GetChatCompletionsAsync();

            pdfUserControl.separaPecasPDF();

            //MessageBox.Show("Terminou a parte local!", "Termino do Processamento");

            //pdfUserControl.RegularizaPecas("0704628-29.2022.8.07.0008");
            //PreprocessamentoTextoIA sumarizaParalelo = new PreprocessamentoTextoIA();
            //sumarizaParalelo.ProcessarDiretorioEmParalelo(dirModeloDeLinguagem, "corrige", numeroProcesso + ".Json");

        }

        private async Task<bool> ClickPdfIconWithSelectorAsync(CoreWebView2 webView)
        {
            string script = @"
            (() => {
                let pdfIcon = document.querySelector('#conteudo > visualizador > div > visualizador-template > div > div.visualizador.com-acoes > div.peca.ng-star-inserted > exibe-documentos > div > mat-toolbar > div > div:nth-child(2) > button:nth-child(3) > span.mat-button-wrapper > mat-icon');
                if (pdfIcon) {
                    pdfIcon.click();
                    return true;
                }
                return false;
            })();";
            string result = await webView.ExecuteScriptAsync(script);
            return result.ToLower() == "true";
        }


        private async Task<bool> ClickPdfIconWithXPathAsync(CoreWebView2 webView)
        {
            string script = @"(() => {let xpath = '//*[@id=" + "\"conteudo\"" + @"]/visualizador/div/visualizador-template/div/div[1]/div[2]/exibe-documentos/div/mat-toolbar/div/div[2]/button[1]/span[1]/mat-icon';
                let result = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                if (result)
                {
                    result.click();
                    return true;
                }
                return false;
            })();";
            string result = await webView.ExecuteScriptAsync(script);
            return result.ToLower() == "true";
        }




        private async Task<bool> ClickPdfIconWithFullPathAsync(CoreWebView2 webView)
        {
            string script = @"
                (() => {
                    let fullPath = '/html/body/app-root/mat-sidenav-container/mat-sidenav-content/div[1]/visualizador/div/visualizador-template/div/div[1]/div[2]/exibe-documentos/div/mat-toolbar/div/div[2]/button[1]/span[1]/mat-icon';
                    let parts = fullPath.split('/').filter(part => part.length > 0);
                    let currentNode = document.documentElement;
                    for (let i = 0; i < parts.length; i++) {
                        let tagName = parts[i].split('[')[0];
                        let index = parts[i].match(/\[\d+\]/);
                        index = index ? parseInt(index[0].replace('[', '').replace(']', '')) - 1 : 0;
                        let found = false;
                        for (let j = 0; j < currentNode.children.length; j++) {
                            if (currentNode.children[j].tagName.toLowerCase() === tagName) {
                                if (index === 0) {
                                    currentNode = currentNode.children[j];
                                    found = true;
                                    break;
                                } else {
                                    index--;
                                }
                            }
                        }
                        if (!found) {
                            return false;
                        }
                    }
                    currentNode.click();
                    return true;
                })();";
            string result = await webView.ExecuteScriptAsync(script);
            return result.ToLower() == "true";
        }

        private async Task<bool> ClickGerarPdfWithSelectorAsync()
        {
            string script = @"
        (() => {
            let gerarPdfButton = document.querySelector('#mat-dialog-0 > carregar-pdf > mat-dialog-actions > button:nth-child(2) > span.mat-button-wrapper');
            if (gerarPdfButton) {
                gerarPdfButton.click();
                return true;
            }
            return false;
        })();";
            string result = await webView.ExecuteScriptAsync(script);
            return result.ToLower() == "true";
        }

        private async Task<bool> ClickGerarPdfWithXPathAsync()
        {
            string script = @"(() => {let xpath = '//*[@id=" + "\"mat-dialog-0\"" + @"]/carregar-pdf/mat-dialog-actions/button[2]/span[1]';
                        let result = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                        if (result)
                        {
                            result.click();
                            return true;
                        }
                        return false;
                        })();";
            string result = await webView.ExecuteScriptAsync(script);
            return result.ToLower() == "true";
        }

        private async Task<bool> ClickGerarPdfWithFullPathAsync()
        {
            string script = @"
                (() => {
                    let fullPath = '/html/body/div[1]/div[1]/carregar-pdf/mat-dialog-actions/button[2]/span[1]';
                    let parts = fullPath.split('/').filter(part => part.length > 0);
                    let currentNode = document.documentElement;
                    for (let i = 0; i < parts.length; i++) {
                        let tagName = parts[i].split('[')[0];
                        let index = parts[i].match(/\[\d+\]/);
                        index = index ? parseInt(index[0].replace('[', '').replace(']', '')) - 1 : 0;
                        let found = false;
                        for (let j = 0; j < currentNode.children.length; j++) {
                            if (currentNode.children[j].tagName.toLowerCase() === tagName) {
                                if (index === 0) {
                                    currentNode = currentNode.children[j];
                                    found = true;
                                    break;
                                } else {
                                    index--;
                                }
                            }
                        }
                        if (!found) {
                            return false;
                        }
                    }
                    currentNode.click();
                    return true;
                })();";
            string result = await webView.ExecuteScriptAsync(script);
            return result.ToLower() == "true";
        }

        private async void DownloadPDF_Click(object sender, RoutedEventArgs e)
        {
            var wpfPrincipalWindow = new wpfPrincipal();
            wpfPrincipalWindow.Show();
        }

        private async Task DownloadPDF()
        {
            ProcessoCSV processoAtivo = Singleton.Instance.ProcessoAtivo;

            //MessageBox.Show(processosView.ProcessoAtivo);
            NumeroProcesso = processoAtivo.Processo;
            int indiceProcessoJudicial = processoAtivo.idPJ;
            string dProc = Path.Combine(@"d:\PJe\Processos",NumeroProcesso);
            string dPecas = Path.Combine(dProc, "PecasProcessuais");
            string dSuma = Path.Combine(dProc, "ModeloDeLinguagem");

            //MessageBox.Show("Vou abrir o  Grid de visualização");
            // Digamos que o caminho para o seu arquivo PDF seja "C:/meusDocumentos/arquivo.pdf"
            //MessageBox.Show(NumeroProcesso);
            string dP = Path.Combine(@"d:\PJe\Processos\");

            if(File.Exists(Path.Combine(dProc, "pdf", NumeroProcesso + ".pdf")))
            {
                string caminhoDoArquivoPdf = Path.Combine(dProc, "pdf", NumeroProcesso + ".pdf");

                _atoProcessualBLL = new AtoProcessualConsultaBLL(); // Inicialize a classe BLL correspondente

                CarregarTreeViewGrupo();

                // Converta o caminho do arquivo para um formato URI
                Uri caminhoDoArquivoPdfUri = new Uri(caminhoDoArquivoPdf);

                // Navegue até o arquivo PDF
                await pdfWebView.EnsureCoreWebView2Async();
                pdfWebView.CoreWebView2.Navigate(caminhoDoArquivoPdfUri.ToString());

                TogglePdfAndWebView(false, false, true, false);
            }
            else
            {
                LimpaTreeViewPdfTexto();
                //MessageBox.Show("Processo ainda não carregado para a base, Clique em:\nBaixar PDF");
            }
        }

        private void MenuAtosProcessuais_Click(object sender, RoutedEventArgs e)
        {
            // Carrega o TreeView com os atos processuais agrupados
            CarregarTreeViewGrupo();
        }

        private void MenuMarchaProcessual_Click(object sender, RoutedEventArgs e)
        {
            // Carrega o TreeView com os atos processuais por data
            CarregarTreeViewData();
        }


        private void TreeViewProcesso_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem selectedNode && selectedNode.Header is string idMovimento)
            {
                TreeViewItem selectedItem = (TreeViewItem)treeViewProcesso.SelectedItem;
                if (selectedItem != null && selectedItem.Tag is AtoProcessualConsulta)
                {
                    AtoProcessualConsulta atoProcessual = (AtoProcessualConsulta)selectedItem.Tag;
                    // Agora você tem o objeto AtoProcessual associado ao item selecionado.
                }
                AbrirTexto(idMovimento);
                AbrirPDF(idMovimento);
                AbrirResumo(idMovimento);
                tabPecas.SelectedIndex = 0;
            }
        }

        private void AbrirResumo(string idMovimento)
        {
            // Obter o nome do arquivo de texto com base no idMovimento
            string nomeArquivoTexto = $"P{idMovimento}[001].json";

            string dP = Path.Combine("d:\\PJe\\Processos", NumeroProcesso, "ModeloDeLinguagem", "JsonSintetico");

            nomeArquivoTexto = Path.Combine(dP, nomeArquivoTexto);

            if (!File.Exists(nomeArquivoTexto))
            {
                string dPNR = Path.Combine("d:\\PJe\\Processos", NumeroProcesso, "ModeloDeLinguagem", "JsonSintetico");
                nomeArquivoTexto = $"P{idMovimento}[001].json";
                nomeArquivoTexto = Path.Combine(dPNR, nomeArquivoTexto);
            }

            // Verificar se o arquivo existe
            if (File.Exists(nomeArquivoTexto))
            {
                string texto = File.ReadAllText(nomeArquivoTexto);
                ProcessamentoDeTexto processamento = new ProcessamentoDeTexto();
                string textoJson = processamento.ExtrairConteudoJson(texto);

                // Preenche o RichTextBox com o conteúdo do arquivo
                textBoxResumo.Document.Blocks.Clear();
                textBoxResumo.FontSize = 18;

                if (texto.Length > 64 )
                {
                    textBoxResumo.Document.Blocks.Add(new Paragraph(new Run(textoJson)));
                }
            }
            else
            {
                // Arquivo de texto não encontrado, exiba uma mensagem de erro ou realize outra ação apropriada
                Debug.WriteLine($"Arquivo de texto para o idMovimento {idMovimento} não encontrado.");
            }
        }


        private void AbrirTexto(string idMovimento)
        {
            // Obter o nome do arquivo de texto com base no idMovimento
            string nomeArquivoTexto = $"P{idMovimento}[001].json";


            string dP = Path.Combine("d:\\PJe\\Processos", NumeroProcesso, "ModeloDeLinguagem", "JsonlReceived");
            nomeArquivoTexto = Path.Combine(dP, nomeArquivoTexto);
            if (!File.Exists(nomeArquivoTexto))
            {
                string dPNR= Path.Combine("d:\\PJe\\Processos", NumeroProcesso, "ModeloDeLinguagem", "JsonlReceived");
                nomeArquivoTexto = $"P{idMovimento}[001].txt";
                nomeArquivoTexto = Path.Combine(dPNR, nomeArquivoTexto);
            }

            // Lê o conteúdo do arquivo de texto

            // Verificar se o arquivo existe
            if (File.Exists(nomeArquivoTexto))
            {
                string texto = File.ReadAllText(nomeArquivoTexto);
                // Ler o conteúdo do arquivo de texto
                //string conteudo = File.ReadAllText(nomeArquivoTexto);

                ProcessamentoDeTexto processamento = new ProcessamentoDeTexto();
                string textoJson = processamento.ExtrairConteudoJson(texto);

                textBoxConteudo.FontFamily = new FontFamily("Arial");
                textBoxConteudo.FontSize = 18;

                // Preenche o RichTextBox com o conteúdo do arquivo
                textBoxConteudo.Document.Blocks.Clear();
                if (texto.Length > 64)
                {
                    textBoxConteudo.Document.Blocks.Add(new Paragraph(new Run(textoJson)));
                }
            }
            else
            {
                // Arquivo de texto não encontrado, exiba uma mensagem de erro ou realize outra ação apropriada
                //MessageBox.Show($"Arquivo de texto para o idMovimento {idMovimento} não encontrado.");
            }
        }

        private void AbrirPDF(string idMovimento)
        {

            // Obter o nome do arquivo de texto com base no idMovimento
            string nomeArquivoPDF = $"P{idMovimento}.pdf";

            string dP = Path.Combine("d:\\PJe\\Processos", NumeroProcesso, "PecasProcessuais", "PecasPDF");
            nomeArquivoPDF = Path.Combine(dP, nomeArquivoPDF);// Obter o nome do arquivo PDF com base no idMovimento
            // Verificar se o arquivo existe
            if (File.Exists(nomeArquivoPDF))
            {
                // Definir a URL do arquivo PDF para o WebView2
                string urlPDF = Path.GetFullPath(nomeArquivoPDF);
                pdfWebView.Source = new Uri(urlPDF);
            }
            else
            {
                // Arquivo PDF não encontrado, exiba uma mensagem de erro ou realize outra ação apropriada
                //MessageBox.Show($"Arquivo PDF para o idMovimento {idMovimento} não encontrado.");
            }
        }


        ////private void Carrega_Processos_Button(object sender, RoutedEventArgs e)
        ////{
        ////    string folderPath = @"D:\PJe\Dados";
        ////    string fileName = GetLastFileName(folderPath);

        ////    var processos = CarregarProcessosDoArquivo(fileName);

        ////    var processosViewModel = processosView.DataContext as ProcessosViewModel;
        ////    if (processosViewModel != null)
        ////    {
        ////        processosViewModel.Processos_DB = processos;
        ////    }
        ////    else
        ////    {
        ////        // Handle error - DataContext is not ProcessosViewModel
        ////    }

        ////    TogglePdfAndWebView(false, true, false);
        ////}


        ////private void SuspendMonitoringButton_Click(object sender, RoutedEventArgs e)
        ////{
        ////    if (isMonitoringSuspended)
        ////    {
        ////        // Reative o monitoramento aqui.
        ////        webView.Visibility = Visibility.Visible;
        ////        btn2Plan.Content = "Running";
        ////    }
        ////    else
        ////    {
        ////        // Suspenda o monitoramento aqui.
        ////        webView.Visibility = Visibility.Collapsed;
        ////        btn2Plan.Content = "Stoped";
        ////    }

        ////    isMonitoringSuspended = !isMonitoringSuspended;
        ////}

         private async void OCR_Button_Click(object sender, RoutedEventArgs e)
        {

            //extrai as imagens
            string diretorioImagens = Path.Combine(@"d:\PJe\Processos\0701819-66.2022.8.07.0008\PecasProcessuais", "Imagens");
            string nomeArquivo = Path.Combine(@"d:\PJe\Processos\0701819-66.2022.8.07.0008\PecasProcessuais","PDF" , "peca_121029802.PDF");

            if (!Directory.Exists(diretorioImagens))
            {
                Directory.CreateDirectory(diretorioImagens);
            }

            PDFToPNGConverter convert = new PDFToPNGConverter();

            convert.ConvertPdfPageToPng(nomeArquivo, diretorioImagens,200, 600, 1);

            //ConvertPdfToPng_cpp(nomeArquivo, diretorioImagens, "600");

            //extrai as tabelas
            string arquivoCabecalho = Path.Combine(@"d:\PJe\Processos\0701819-66.2022.8.07.0008\", "0701819-66.2022.8.07.0008" + "cab.pdf");
            string diretorioTabelas = Path.Combine(@"d:\PJe\Processos\0701819-66.2022.8.07.0008\PecasProcessuais", "Tabelas");
            if (!Directory.Exists(diretorioTabelas))
            {
                Directory.CreateDirectory(diretorioTabelas);
            }
            ExtractTablesFromPdfcpp(arquivoCabecalho, diretorioTabelas);


            //extrai o texto
            string diretorioTexto = Path.Combine(@"d:\PJe\Processos\0701819-66.2022.8.07.0008\PecasProcessuais", "Doc");
            if (!Directory.Exists(diretorioTexto))
            {
                Directory.CreateDirectory(diretorioTexto);
            }
            ExtractTextFromPdfcpp(arquivoCabecalho, diretorioTexto,"1");


            string tiffFilePath = @"D:\PJe\Processos\0001080-47.2016.8.07.0008\page_1_binary.png";
            string tessDataPath = @"C:\Arquivos de Programas\Tesseract-OCR\tessdata"; // Caminho para o diretório tessdata

            using (var engine = new TesseractEngine(tessDataPath, "por", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(tiffFilePath))
                {
                    using (var page = engine.Process(img))
                    {
                        string text = page.GetText();
                        File.WriteAllText(@"d:\PJe\Processos\0001080-47.2016.8.07.0008\page_1_binary_output.txt", text);
                        MessageBox.Show("OCR concluído!");
                    }
                }
            }
         }
        private async Task ExtractTextFromPdfcpp(string pdfFilePath, string outputTxtFile, string pagina)
        {
            string appPath = @"d:\PJe\App_CPP\Extract_Text.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = appPath,
                Arguments = $"\"{pdfFilePath}\" \"{outputTxtFile}\" \"{pagina}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
        }

        private void ExtractTablesFromPdfcpp(string pdfFilePath, string outputCsvDirectory)
        {
            string appPath = @"d:\PJe\App_CPP\ExtractTextTables.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = appPath,
                Arguments = $"\"{pdfFilePath}\" \"{outputCsvDirectory}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
        }

        private void BuscaProcessoEspecífico(object sender, RoutedEventArgs e)
        {
            // Desligar todos os outros grids
            gridUserControl.Visibility = Visibility.Collapsed;
            gridIntranet.Visibility = Visibility.Collapsed;
            processosView.Visibility = Visibility.Collapsed;
            gridProcesso.Visibility = Visibility.Collapsed;

            // Mostrar o grid do RichTextBox
            gridRichTextBox.Visibility = Visibility.Visible;

            bool mostrarNovosComponentes = true;
            if (mostrarNovosComponentes)
            {
                // Ocultar componentes existentes
                richTextEdit.Visibility = Visibility.Collapsed;
                richTextVisualizer.Visibility = Visibility.Collapsed;
                // Mostrar novos componentes
                tabControlLLM_Processing.Visibility = Visibility.Visible;
                azureView.Visibility = Visibility.Visible;
                logRichTextBox.Visibility = Visibility.Visible;

                IniciarAplicacaoPython();

            }
            else
            {
                // Mostrar componentes existentes
                richTextEdit.Visibility = Visibility.Visible;
                richTextVisualizer.Visibility = Visibility.Visible;
                // Ocultar novos componentes
                tabControlLLM_Processing.Visibility = Visibility.Collapsed;
                azureView.Visibility = Visibility.Collapsed;
                logRichTextBox.Visibility = Visibility.Collapsed;
            }

        }


        private void IniciarAplicacaoPython()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {

                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    FileName = "c:\\Python311\\Python.exe", // Caminho completo do Python
                    Arguments = @"D:/Usuarios/DotNET/PPProcessingEEN/main.py D:/PJe/Processos/0705484-96.2022.8.07.0006 argumento2",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                Process processoPython = new Process { StartInfo = startInfo };
                processoPython.Start();

                processoPython.BeginOutputReadLine();
                processoPython.BeginErrorReadLine();

                processoPython.OutputDataReceived += (sender, args) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (args.Data != null)
                        {
                            logRichTextBox.AppendText(args.Data);
                            if (!string.IsNullOrEmpty(args.Data) && args.Data.Contains("Aplicacao Python terminou com sucesso."))
                            {
                                processoPython.CancelOutputRead();
                                processoPython.CancelErrorRead();
                                //processoPython.Close(); // Ou processoPython.Kill() se você quer garantir que o processo seja encerrado
                                processoPython.Kill();
                            }
                        }
                    });
                };

                processoPython.ErrorDataReceived += (sender, args) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (args.Data != null)
                        {
                            logRichTextBox.AppendText(args.Data + "\n");
                        }
                    });
                };
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    logRichTextBox.AppendText($"Erro ao iniciar a aplicação Python: {ex.Message}\n");
                });
            }
        }



        public void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Adicione a lógica para salvar o texto do controle RichTextBox em um arquivo aqui.
            MessageBox.Show("Botão CLicado!");
        }


        public void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            // Implemente a lógica para carregar o texto do arquivo para o controle RichTextBox aqui.
            MessageBox.Show("Botão CLicado!");
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            richTextEdit.Document.Blocks.Clear();
            // Altere a cor de fundo para cinza
            richTextEdit.Background = Brushes.White;
            richTextEdit.Foreground = Brushes.Black;

        }


        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|XML Files (*.xml)|*.xml|Word Documents (*.docx)|*.docx|Old Word Documents (*.doc)|*.doc|OpenDocument Text (*.odt)|*.odt"
            };

            var result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                string fileExtension = Path.GetExtension(filename).ToLower();

                switch (fileExtension)
                {
                    case ".txt":
                        var content = File.ReadAllText(filename);

                        // Clear the existing content
                        richTextEdit.Document.Blocks.Clear();

                        // Create a new paragraph with the desired content
                        Paragraph paragraph = new Paragraph(new Run(content));

                        // Set the text color to black
                        paragraph.Foreground = Brushes.Black;

                        // Add the paragraph to the RichTextBox's document
                        richTextEdit.Document.Blocks.Add(paragraph);

                        // Set the background of RichTextBox to white
                        richTextEdit.Background = Brushes.White;

                        break;


                    // TODO: adicione lógica semelhante para ler arquivos .csv, .xml, .docx, .doc e .odt

                    default:
                        MessageBox.Show("Formato de arquivo não suportado.");
                        break;
                }

                // Mude a cor de fundo para cinza quando um novo arquivo for aberto
                richTextEdit.Background = Brushes.White ;
                richTextEdit.Foreground= Brushes.Black;
            }
        }



        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Código para salvar o conteúdo do RichTextBox em um arquivo
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            // Corte o texto selecionado
            richTextEdit.Cut();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            // Copia o texto selecionado
            richTextEdit.Copy();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            // Cola o texto da área de transferência
            richTextEdit.Paste();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            // Desfaz a última ação
            richTextEdit.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            // Refaz a última ação desfeita
            richTextEdit.Redo();
        }

        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            // Alterna o negrito para o texto selecionado
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            // Alterna o itálico para o texto selecionado
        }

        private void Underline_Click(object sender, RoutedEventArgs e)
        {
            // Alterna o sublinhado para o texto selecionado
        }
        /*
         
         <MenuItem Header="Pequeno" Click="EditFontSizeSmall_Click"/>
                                <MenuItem Header="Médio" Click="EditFontSizeMedium_Click"/>
                                <MenuItem Header="Grande" Click="EditFontSizeLarge_Click"/>
         
         
         
         */



        public void EditFontSizeSmall_Click(object sender, RoutedEventArgs e)
        {
            if (richTextEdit.Selection.IsEmpty)
            {
                richTextEdit.FontSize = 12;
            }
            else
            {
                richTextEdit.Selection.ApplyPropertyValue(Control.FontSizeProperty, 12.0);
            }
        }

        public void EditFontSizeMedium_Click(object sender, RoutedEventArgs e)
        {
            if (richTextEdit.Selection.IsEmpty)
            {
                richTextEdit.FontSize = 16;
            }
            else
            {
                richTextEdit.Selection.ApplyPropertyValue(Control.FontSizeProperty, 16.0);
            }
        }

        //public void EditFontSizeLarge_Click(object sender, RoutedEventArgs e)
        //{
        //    if (richTextEdit.Selection.IsEmpty)
        //    {
        //        richTextEdit.FontSize = 22;
        //    }
        //    else
        //    {
        //        richTextEdit.Selection.ApplyPropertyValue(Control.FontSizeProperty, 22.0);
        //    }
        //}
        public void EditFontSizeLarge_Click(object sender, RoutedEventArgs e)
        {
            if (richTextEdit.Selection.IsEmpty)
            {
                richTextEdit.FontSize = 22;
                richTextEdit.Background = Brushes.White; // Define o background como branco
                richTextEdit.Foreground = Brushes.Black; // Define a cor da fonte como preto
            }
            else
            {
                richTextEdit.Selection.ApplyPropertyValue(Control.FontSizeProperty, 22.0);
                richTextEdit.Selection.ApplyPropertyValue(Control.BackgroundProperty, Brushes.White); // Define o background da seleção como branco
                richTextEdit.Selection.ApplyPropertyValue(Control.ForegroundProperty, Brushes.Black); // Define a cor da fonte da seleção como preto
            }
        }


        public void ViewFontSizeSmall_Click(object sender, RoutedEventArgs e)
        {
            richTextVisualizer.FontSize = 12;
        }

        public void ViewFontSizeMedium_Click(object sender, RoutedEventArgs e)
        {
            richTextVisualizer.FontSize = 16;
        }

        public void ViewFontSizeLarge_Click(object sender, RoutedEventArgs e)
        {
            richTextVisualizer.FontSize = 22;
        }



        //só formata
        private void FormatIA_Click_Old(object sender, RoutedEventArgs e)
        {
            // 1. Crie os diretórios, se eles não existirem
            string rootPath = @"d:\PJe\InteligenciaArtificial\edição";
            Directory.CreateDirectory(rootPath);

            string dateSubdirectory = Path.Combine(rootPath, DateTime.Now.ToString("yyyyMMdd"));
            Directory.CreateDirectory(dateSubdirectory);

            // 2. Selecione todo o texto do RichTextBox
            TextRange textRange = new TextRange(richTextEdit.Document.ContentStart, richTextEdit.Document.ContentEnd);
            string fullText = textRange.Text;

            // 3. Divida o texto em pedaços de até 37 KB
            int partSize = 32 * 1024; // 37 KB
            List<string> parts = new List<string>();

            for (int i = 0; i < fullText.Length; i += partSize)
            {
                parts.Add(fullText.Substring(i, Math.Min(partSize, fullText.Length - i)));
            }

            // 4. Armazene cada pedaço em um arquivo no diretório do dia atual, com nomes sequenciais
            for (int i = 0; i < parts.Count; i++)
            {
                string fileName = Path.Combine(dateSubdirectory, $"I{i + 1:000}.txt");
                File.WriteAllText(fileName, parts[i]);
            }
            string diretorioSaida = Path.Combine(dateSubdirectory, "Saida");

            FormataTextoIA(dateSubdirectory, diretorioSaida, "", 1);

            MessageBox.Show($"Texto formatado e armazenado em {dateSubdirectory}");
        }
        public void FormataTextoIA(string diretorio, string diretorioPecas, string tipoProc, int nrPag)
        {
            // Carregar todos os arquivos no diretório
            var arquivos = Directory.GetFiles(diretorio);
            string diretorioPecasSumarizadas = Path.GetDirectoryName(diretorio);

            // Ordenar os arquivos por data de criação
            var arquivosOrdenados = arquivos.OrderBy(f => File.GetCreationTime(f)).ToList();

            string script = "";

            switch (tipoProc)
            {
                case "resumo":
                    script = "d:\\PJe\\App_cpp\\openAI_sumarizaRes.py";
                    break;
                case "corrige":
                    script = "d:\\PJe\\App_cpp\\openAI_sumarizaCor.py";
                    break;
                case "formatared":
                    script = "d:\\PJe\\App_cpp\\openAI_sumarizaFor.py";
                    break;
                default:
                    script = "d:\\PJe\\App_cpp\\openAI_sumarizaPar.py";
                    break;
            }




            int count = 0;
            string feitos = "";
            // Iterar em todos os arquivos
            foreach (var arquivo in arquivosOrdenados)
            {
                // Obter o tamanho do arquivo
                count++;

                feitos = $"formatados {count} de {arquivosOrdenados.Count()}";
                Debug.WriteLine(feitos);
                Debug.WriteLine(arquivo);
                if (File.Exists(arquivo))
                {
                    long tamanhoArquivo = new FileInfo(arquivo).Length;

                    // Exibir uma mensagem com o tamanho do arquivo
                    if (tamanhoArquivo < 200)
                    {
                        return;
                    }
                    else if (tamanhoArquivo < 7000)
                    {
                        ChamaScriptPython(script,
                                arquivo,
                                diretorioPecasSumarizadas,
                                tipoProc,
                                "gpt-3.5-turbo");
                    }
                    else
                    {
                        ChamaScriptPython(script,
                            arquivo,
                            diretorioPecasSumarizadas,
                                tipoProc,
                            "gpt-3.5-turbo-16k");
                    }
                }
            }
            //agora vamos mesclar os arquivos e carregar no 'richTextVisualizer' 

            // Chamar o novo método
            MesclarEExibirArquivos(diretorioPecasSumarizadas);
        }


        public void MesclarEExibirArquivos(string diretorio)
        {
            // Obter todos os arquivos no diretório
            var arquivos = Directory.GetFiles(diretorio);

            // Ordenar os arquivos por nome (assumindo que eles têm um número sequencial no nome)
            var arquivosOrdenados = arquivos.OrderBy(f => f).ToList();

            StringBuilder textoCompleto = new StringBuilder();

            // Iterar em todos os arquivos
            foreach (var arquivo in arquivosOrdenados)
            {
                // Ler o conteúdo do arquivo como UTF-8
                string conteudo = File.ReadAllText(arquivo, Encoding.UTF8);

                // Adicionar o conteúdo ao texto completo
                textoCompleto.AppendLine(conteudo);
            }

            // Limpar o RichTextBox
            richTextVisualizer.Document.Blocks.Clear();

            // Carregar o texto completo no RichTextBox
            richTextVisualizer.Document.Blocks.Add(new Paragraph(new Run(textoCompleto.ToString())));
        }



        private void ChamaScriptPython(string scriptPython, string arquivo, string diretorioPecasSumarizadas, string tipoResumo, string modelo)
        {
            // Preparar argumentos para o script Python
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
                "C:\\Arquivos de Programas\\Java\\jdk-21\\bin\\java.exe",
                "d:\\Java\\projetos\\gabia.pdf\\pdfutils-app\\target\\AppPyOpenAI_Sumariza.jar",
                argsAI
            );
            rodaAI.Run();
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            // Implemente a lógica para exportar para PDF aqui.
        }


        private void FerramentasSlideDown(object sender, RoutedEventArgs e)
        {
            if (Ferramentas.Visibility == Visibility.Collapsed)
            {
                Ferramentas.Visibility = Visibility.Visible;
            }
            else
            {
                Ferramentas.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenDocument_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void NewDocument_Click(object sender, RoutedEventArgs e)
        {
            richTextEdit.Document.Blocks.Clear();
            //richTextEdit.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38ADF0"));
            //richTextEdit.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
        }

        private void SaveDocument_Click(object sender, RoutedEventArgs e)
        {
            // Configuração da caixa de diálogo de salvar arquivo
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
            saveFileDialog.DefaultExt = "rtf";

            // Mostrar a caixa de diálogo de salvar arquivo
            if (saveFileDialog.ShowDialog() == true)
            {
                // Obter o fluxo de arquivo para o arquivo selecionado
                Stream fileStream = saveFileDialog.OpenFile();

                // Verificar se o arquivo foi aberto com sucesso
                if (fileStream != null)
                {
                    TextRange range;
                    System.Windows.Documents.FlowDocument doc = richTextEdit.Document;
                    range = new TextRange(doc.ContentStart, doc.ContentEnd);
                    range.Save(fileStream, DataFormats.Rtf);
                    fileStream.Close();
                    MessageBox.Show("Documento salvo com sucesso.");
                }
                else
                {
                    MessageBox.Show("Erro ao salvar o arquivo.");
                }
            }
        }


        private void Exit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CutText_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CopyText_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PasteText_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BoldText_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ItalicText_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UnderlineText_Click(object sender, RoutedEventArgs e)
        {

        }

        private void IncreaseFontSize_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DecreaseFontSize_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Correct_Click(object sender, RoutedEventArgs e)
        {
            if (RichTextEditEstaVazio())
            {
                MessageBox.Show("O editor de texto está vazio. Por favor, insira algum texto para corrigir.");
                return;
            }

            ProcessaTexto(32 * 1024, "corrige");
        }

        private void FormatarEcorrigir_Click(object sender, RoutedEventArgs e)
        {
            if (RichTextEditEstaVazio())
            {
                MessageBox.Show("O editor de texto está vazio. Por favor, insira algum texto para formatar e corrigir.");
                return;
            }

            ProcessaTexto(32 * 1024, "formatared");
        }

        private void Sumarizar_Click(object sender, RoutedEventArgs e)
        {
            if (RichTextEditEstaVazio())
            {
                MessageBox.Show("O editor de texto está vazio. Por favor, insira algum texto para sumarizar.");
                return;
            }

            ProcessaTexto(40 * 1024, "resumo");
        }

        private bool RichTextEditEstaVazio()
        {
            TextRange textRange = new TextRange(richTextEdit.Document.ContentStart, richTextEdit.Document.ContentEnd);
            return string.IsNullOrWhiteSpace(textRange.Text);
        }

        private void ProcessaTexto(int tamanhoParte, string tipoProcessamento)
        {

            // 1. Cria os diretórios, se eles não existirem
            string caminhoRaiz = @"d:\PJe\InteligenciaArtificial\edição";
            Directory.CreateDirectory(caminhoRaiz);

            string subdiretorioData = Path.Combine(caminhoRaiz, DateTime.Now.ToString("yyyyMMdd"));
            Directory.CreateDirectory(subdiretorioData);

            // 2. Seleciona todo o texto do RichTextBox
            TextRange textRange = new TextRange(richTextEdit.Document.ContentStart, richTextEdit.Document.ContentEnd);
            string textoCompleto = textRange.Text;

            // 3. Divide o texto em pedaços de até tamanhoParte
            List<string> partes = new List<string>();

            for (int i = 0; i < textoCompleto.Length; i += tamanhoParte)
            {
                partes.Add(textoCompleto.Substring(i, Math.Min(tamanhoParte, textoCompleto.Length - i)));
            }

            // 4. Armazena cada pedaço em um arquivo no diretório do dia atual, com nomes sequenciais
            for (int i = 0; i < partes.Count; i++)
            {
                string nomeArquivo = Path.Combine(subdiretorioData, $"I{i + 1:000}.txt");
                File.WriteAllText(nomeArquivo, partes[i]);
            }

            string diretorioSaida = Path.Combine(subdiretorioData, "Saida");

            // Chama a ação de processamento apropriada
            FormataTextoIA(subdiretorioData, diretorioSaida, tipoProcessamento, 1);

            MessageBox.Show($"Texto processado e armazenado em {subdiretorioData}");
        }
        private void MenuRequisitos_Click(object sender, RoutedEventArgs e)
        {
            // Coloque sua lógica aqui
        }

        private void MenuResumoProcessual_Click(object sender, RoutedEventArgs e)
        {
            // Coloque sua lógica aqui
        }

        private void MenuRelatorioSintetico_Click(object sender, RoutedEventArgs e)
        {
            if(!File.Exists(@"d:\PJe\Processos\" + Singleton.Instance.ProcessoAtivo.Processo + @"\Doc\" + Singleton.Instance.ProcessoAtivo.Processo + ".html"))
            {
                ProcessamentoDeTexto processamento = new ProcessamentoDeTexto();
                processamento.ProcessDirtXml("d:\\PJe\\Processos\\" + Singleton.Instance.ProcessoAtivo.Processo + "\\PecasProcessuais\\PecasTxt_S", "d:\\PJe\\Processos",
                                        Singleton.Instance.ProcessoAtivo.Processo,"resumido");
            }
            if (File.Exists(@"d:\PJe\Processos\" + Singleton.Instance.ProcessoAtivo.Processo + @"\Doc\" + Singleton.Instance.ProcessoAtivo.Processo + ".html"))
            {
                string htmlFile = $@"d:\PJe\Processos\{Singleton.Instance.ProcessoAtivo.Processo}\Doc\{Singleton.Instance.ProcessoAtivo.Processo}.html";
                Uri fileUri = new Uri(htmlFile);
                pdfWebView.CoreWebView2.Navigate(fileUri.AbsoluteUri);
            }
        }

        private void MenuRelatorioAnalitico_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(@"d:\PJe\Processos\" + Singleton.Instance.ProcessoAtivo.Processo + @"\Doc\" + Singleton.Instance.ProcessoAtivo.Processo + "A.html"))
            {
                ProcessamentoDeTexto processamento = new ProcessamentoDeTexto();
                processamento.ProcessDirtXml("d:\\PJe\\Processos\\" + Singleton.Instance.ProcessoAtivo.Processo + "\\PecasProcessuais\\PecasTxt_R", "d:\\PJe\\Processos",
                                        Singleton.Instance.ProcessoAtivo.Processo, "completo");
            }
            if (File.Exists(@"d:\PJe\Processos\" + Singleton.Instance.ProcessoAtivo.Processo + @"\Doc\" + Singleton.Instance.ProcessoAtivo.Processo + ".html"))
            {
                string htmlFile = $@"d:\PJe\Processos\{Singleton.Instance.ProcessoAtivo.Processo}\Doc\{Singleton.Instance.ProcessoAtivo.Processo}.html";
                Uri fileUri = new Uri(htmlFile);
                pdfWebView.CoreWebView2.Navigate(fileUri.AbsoluteUri);
            }
        }

        private void MenuBuscaSemantica_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuTranscrição_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuDeatalhesPeticao_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuRelatorioParecer_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(@"d:\PJe\Processos\" + Singleton.Instance.ProcessoAtivo.Processo + @"\Doc\" + Singleton.Instance.ProcessoAtivo.Processo + "PF.html"))
            {
                ProcessamentoDeTexto processamento = new ProcessamentoDeTexto();
                processamento.ProcessDirtXml("d:\\PJe\\Processos\\" + Singleton.Instance.ProcessoAtivo.Processo + "\\PecasProcessuais\\PecasTxt_R", "d:\\PJe\\Processos",
                                        Singleton.Instance.ProcessoAtivo.Processo,"pf");
            }
            if (File.Exists(@"d:\PJe\Processos\" + Singleton.Instance.ProcessoAtivo.Processo + @"\Doc\" + Singleton.Instance.ProcessoAtivo.Processo + ".html"))
            {
                string htmlFile = $@"d:\PJe\Processos\{Singleton.Instance.ProcessoAtivo.Processo}\Doc\{Singleton.Instance.ProcessoAtivo.Processo}.html";
                Uri fileUri = new Uri(htmlFile);
                pdfWebView.CoreWebView2.Navigate(fileUri.AbsoluteUri);
            }
        }

    }
}
