using FramePFX.Themes;
using GabIA.BLL;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using IOPath = System.IO.Path;
using System.Collections.ObjectModel;
using System.Diagnostics;
using GabIA.ENT;
using System.Globalization;
using GabIA.DAL;

namespace GabIA.WPF
{
    /// <summary>
    /// Interaction logic for wpfPrincipal.xaml
    /// </summary>
    class ProcessoPolos
    {
        public List<string> PoloAtivo { get; set; }
        public List<string> PoloPassivo { get; set; }

        public ProcessoPolos()
        {
            PoloAtivo = new List<string>();
            PoloPassivo = new List<string>();
        }
    }

    public partial class wpfPrincipal : Window
    {
        public bool enteredIntranet = false;
        public string connectionString = "";
        public bool shouldUpdateProcessCSV = true;
        public bool isNavigationTriggeredByCode = true;
        public wpfPrincipal()
        {
            InitializeComponent();
        }

        public class ProcessoViewModel
        {
            public ObservableCollection<string> Triagem { get; set; }
            public string ExpedienteID { get; set; }
            public string Numero { get; set; }
            public DateTime Prazo { get; set; }
            public ObservableCollection<string> Partes { get; set; }
            public string Classe { get; set; }
            public ObservableCollection<string> Detalhes { get; set; }

            // Construtor
            public ProcessoViewModel()
            {
                Triagem = new ObservableCollection<string>();
                Partes = new ObservableCollection<string>();
                Detalhes = new ObservableCollection<string>();
            }
        }


        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
            switch (((MenuItem)sender).Uid)
            {
                case "0": ThemesController.SetTheme(ThemeType.DeepDark); break;
                case "1": ThemesController.SetTheme(ThemeType.SoftDark); break;
                case "2": ThemesController.SetTheme(ThemeType.DarkGreyTheme); break;
                case "3": ThemesController.SetTheme(ThemeType.GreyTheme); break;
                case "4": ThemesController.SetTheme(ThemeType.LightTheme); break;
                case "5": ThemesController.SetTheme(ThemeType.RedBlackTheme); break;
                case "6": ThemesController.SetTheme(ThemeType.HighContrast); break;
            }

            e.Handled = true;
        }

        public void BancoDeDadosHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private void TestarConexaoEExibirMensagem()
        {
            try
            {
                MessageBox.Show(connectionString);
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Conexão OK", "Conexão com o Banco de Dados", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Conexão falhou: {ex.Message}", "Erro de Conexão", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void nowDisplay(string componentName)
        {
            // Inicialmente, define todos os componentes para 'Collapsed'
            webViewMP.Visibility = Visibility.Collapsed;
            Processos.Visibility = Visibility.Collapsed;
            Processo.Visibility = Visibility.Collapsed;
            Assistente.Visibility = Visibility.Collapsed;
            InteractiveLog.Visibility = Visibility.Collapsed;
            webNav.Visibility = Visibility.Collapsed;
            ListViewProcessos.Visibility = Visibility.Collapsed;
            dataGridProcessos.Visibility= Visibility.Collapsed;
            ComVista.Visibility = Visibility.Collapsed;

            // Ativa a visibilidade do componente especificado
            switch (componentName)
            {
                case "WebViewMP":
                    webViewMP.Visibility = Visibility.Visible;
                    break;

                case "Processos":
                    Processos.Visibility = Visibility.Visible;
                    break;
                case "Processo":
                    Processo.Visibility = Visibility.Visible;
                    break;
                case "Assistente":
                    Assistente.Visibility = Visibility.Visible;
                    break;
                case "InteractiveLog":
                    InteractiveLog.Visibility = Visibility.Visible;
                    break;
                case "WebNav":
                    webNav.Visibility = Visibility.Visible;
                    break;
                case "ListViewProcessos":
                    ListViewProcessos.Visibility = Visibility.Visible;
                    ComVista.Visibility= Visibility.Visible;
                    dataGridProcessos.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show($"Erro ao carregar a página: {e.WebErrorStatus}");
            }
        }

        private void Processo_Click(object sender, RoutedEventArgs e)
        {
            nowDisplay("Processo");
        }

        private void WebView_Click(object sender, RoutedEventArgs e)
        {
            nowDisplay("WebView");
            //var webViewWindow = new Themes.WebView2(); // Use o namespace correto se necessário
            //webViewWindow.Show();
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            // Encerra a aplicação
            this.Close();
        }

        private void WebNav_Click(object sender, RoutedEventArgs e)
        {
            webNav.CoreWebView2.Navigate("https://www.jusbrasil.com.br/");
            nowDisplay("WebNav");
        }
        private void NeoGab_Click(object sender, RoutedEventArgs e)
        {
            if (!enteredIntranet)
            {
                webViewMP.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes");
                enteredIntranet = true;
            }
            nowDisplay("WebViewMP");
        }

        private void Testar_Conexao_Click(object sender, RoutedEventArgs e)
        {
            TestarConexaoEExibirMensagem();
        }

        private async Task atualizaProcessCSV_wpfFORM()
        {
            shouldUpdateProcessCSV = true;

            isNavigationTriggeredByCode = true;

            string currentUrl = webViewMP.Source.AbsoluteUri;
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

            List<ProcessoCompletoENT> allData = new List<ProcessoCompletoENT>();

            int rowsPerPage = 5;
            int totalPages = (int)Math.Ceiling(totalRows / (double)rowsPerPage);

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                var pageData = await ExtractDataFromTable();
                foreach (var rowData in pageData)
                {
                    if (rowData == null || rowData.All(string.IsNullOrWhiteSpace)) // Verifica se rowData é nulo ou todos os elementos são vazios
                    {
                        continue; // Pula para a próxima iteração se rowData for vazio ou nulo
                    }

                    string csvRow = string.Join("|", rowData);
                    ProcessoCompletoENT processo = ConvertRowToProcesso(csvRow);
                    if (processo != null)
                    {
                        allData.Add(processo);
                    }
                }


                if (pageIndex < totalPages - 1)
                {
                    string clickScript = "document.querySelector('#mat-tab-content-0-0 > div > agrupamento-intimacoes > article > " +
                        "div:nth-child(3) > mat-paginator > div > div > div.mat-paginator-range-actions > " +
                        "button.mat-focus-indicator.mat-tooltip-trigger.mat-paginator-navigation-next.mat-icon-button.mat-button-base').click();";

                    await webViewMP.ExecuteScriptAsync(clickScript);
                    await Task.Delay(1000);
                }
            }
            var processoBll = new ProcessoCompletoBLL(); // Adicione outras dependências DAL conforme necessário

            // Execução do método PersisteDataBase
            if (processoBll.PersisteDataBase(allData))
            {
                // Lógica a ser executada se a persistência for bem-sucedida
                MessageBox.Show("atualizei");
            }
            else
            {
                // Lógica a ser executada se a persistência falhar
                MessageBox.Show("não consegui atualizar");
            }


            // Popula o DataGrid
            dataGridProcessos.ItemsSource = allData;

            // Salva os dados em CSV
            string folderPath = @"D:\PJe\Dados";
            string fileName = GetNextFileName(folderPath);
            WriteDataToCSV(allData.Select(processo => new List<string> { ProcessoToCSVRow(processo) }).ToList(), fileName, totalRows);
        }

        private ProcessoCompletoENT ConvertRowToProcesso(string csvRow)
        {
            var campos = csvRow.Split('|');
            var processo = new ProcessoCompletoENT();

            // Extração e tratamento dos campos do CSV
            var tipoEMovimento = campos[0].Trim().Split(' ');
            processo.Movimento = tipoEMovimento[0]; // Exemplo: "32728079"
            processo.Tipo = tipoEMovimento.Length > 1 ? tipoEMovimento[1].Replace("(", "").Replace(")", "") : ""; // Exemplo: "Despacho"

            processo.NumeroProcesso = campos[1].Trim();

            string dateString = campos[2];
            DateTime dataConvertida;  // DateTime não anulável
            dataConvertida = DateTime.Now;
            bool sucesso = false;
            string[] formatosPossiveis = new string[] {
                "dd/MM/yyyy 'às' HH:mm:ss",
                "dd-MM-yyyy HH:mm:ss",
                // Adicione mais formatos conforme necessário
            };

            foreach (var formato in formatosPossiveis)
            {
                sucesso = DateTime.TryParseExact(dateString, formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out dataConvertida);
                if (sucesso)
                {

                    break; // Sai do loop se a conversão for bem-sucedida
                }
            }

            if (!sucesso)
            {
                // Nenhum formato funcionou, decida como lidar com isso
                throw new FormatException("Formato de data inválido ou desconhecido: " + dateString);
            }

            DateTime dataReferencia = DateTime.Now; // Pode ser hoje ou outra data de referência
            //dataConvertida = DateTime.Now;
            // Calculando a diferença em dias
            int prazoParaConsulta = (dataReferencia - dataConvertida).Days;

            // Atribuindo a prazoParaConsulta
            processo.PrazoParaConsulta = prazoParaConsulta;

            processo.DataDaAbertura = dataReferencia;

            // Supondo que 'campos[4]' contém a string com Polo Ativo e 'campos[5]' contém a string com Polo Passivo
            var polos = ExtrairNomesPolo(campos[4]);
            processo.PoloAtivo = polos.PoloAtivo;
            processo.PoloPassivo = polos.PoloPassivo;


            processo.Classe = campos[5].Trim();

            processo.MembroResponsavel = campos[6].Trim();
            processo.Promotoria = campos[7].Trim();

            // Retornando o objeto preenchido
            return processo;
        }

        private string ProcessoToCSVRow(ProcessoCompletoENT processo)
        {
            var camposCSV = new List<string>
            {
                $"{processo.Movimento} ({processo.Tipo})",
                processo.NumeroProcesso,
                processo.DataDaAbertura.ToString("dd/MM/yyyy 'às' HH:mm:ss"),
                $"Consulta Eletrônica: {processo.PrazoParaConsulta} dia{(processo.PrazoParaConsulta != 1 ? "s" : "")}",
                $"Polo ativo: {processo.PoloAtivo}",
                $"Polo passivo: {processo.PoloPassivo}",
                processo.Classe,
                processo.MembroResponsavel,
                processo.Promotoria
            };

            return string.Join("|", camposCSV);
        }


        private async Task atualizaProcessCSV_New()
        {
            shouldUpdateProcessCSV = true;

            isNavigationTriggeredByCode = true;

            string currentUrl = webViewMP.Source.AbsoluteUri;
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

                    await webViewMP.ExecuteScriptAsync(clickScript);
                    await Task.Delay(1000);
                }
            }

            string folderPath = @"D:\PJe\Dados";
            //string fileNameTemp = @"D:\PJe\Dados\dados_tabela_030.txt";
            string fileName = GetNextFileName(folderPath);
            WriteDataToCSV(allData, fileName, totalRows);

            var operacoesBLL = new OperacoesBLL();
            operacoesBLL.LoadDataFromCSV(fileName);

            //ProcessListView.Items.Clear();

            foreach (List<string> row in allData)
            {
                // Verifique se a linha tem pelo menos 2 elementos
                if (row.Count >= 2)
                {
                    // Extraia o elemento no índice 1 (número do processo)
                    string proc = row[1];

                    // Adicione o número do processo diretamente à lista
                    //ProcessListView.Items.Add(proc);
                }
            }
        }

        private string GetNextFileName(string folderPath)
        {
            int currentMax = 0;

            var files = Directory.GetFiles(folderPath, "dados_tabela_*.txt");

            foreach (var file in files)
            {
                string fileName = IOPath.GetFileNameWithoutExtension(file);
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
            return IOPath.Combine(folderPath, $"dados_tabela_{(currentMax + 1).ToString("D3")}.txt");
        }

        private string GetLastFileName(string folderPath)
        {
            int currentMax = 0;

            var files = Directory.GetFiles(folderPath, "dados_tabela_*.txt");

            foreach (var file in files)
            {
                string fileName = IOPath.GetFileNameWithoutExtension(file);
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
            return IOPath.Combine(folderPath, $"dados_tabela_{(currentMax).ToString("D3")}.txt");
        }
        private async void atualizaProcessCSV(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            // Verifique se a URL atual é "intranet.mpdft.mp.br/novoGabinete"
            string currentUrl = webViewMP.Source.AbsoluteUri;
            if (currentUrl != "https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes")
            {
                webViewMP.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes");
                MessageBox.Show("Aguarde NeoGab->Intimações", "Carregar Processos com Vista");
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

                        await webViewMP.ExecuteScriptAsync(clickScript);
                        for (int contTemp = 1; contTemp < 1000000; contTemp++)
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

        //teste do data grid


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

            string resultJson = await webViewMP.ExecuteScriptAsync(script);
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

            string totalRowsText = await webViewMP.ExecuteScriptAsync(script);

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

        private async void Atualizar_Base_click(object sender, RoutedEventArgs e)
        {
            //nowDisplay("WebView");

            await atualizaProcessCSV_wpfFORM();
            //webView.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/intimacoes");
        }

        private void Processos_Click(object sender, RoutedEventArgs e)
        {
            nowDisplay("ListViewProcessos");
            dataGridProcessos.ItemsSource = PreencheDataGridTeste(20); // para 10 linhas de dados fictícios
        }

        private void Extrajudicial_Click(object sender, RoutedEventArgs e)
        {
            // Exibe o menu Extrajudicial
            MessageBox.Show("Menu Extrajudicial selecionado", "Extrajudicial", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NoticiaFato_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para Notícia de Fato
            MessageBox.Show("Notícia de Fato selecionada", "Extrajudicial", MessageBoxButton.OK, MessageBoxImage.Information);
            nowDisplay("WebViewMP");
            webViewMP.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/extrajudicial");
        }

        private void ProcedimentoAdm_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para Procedimento Administrativo
            MessageBox.Show("Procedimento Administrativo selecionado", "Extrajudicial", MessageBoxButton.OK, MessageBoxImage.Information);
            nowDisplay("WebViewMP");
            webViewMP.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/extrajudicial");
        }

        private void InqueritoCivil_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para Inquérito Civil
            MessageBox.Show("Inquérito Civil selecionado", "Extrajudicial", MessageBoxButton.OK, MessageBoxImage.Information);
            nowDisplay("WebViewMP");
            webViewMP.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/extrajudicial");
        }

        private void PericiaCarregarDocs_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para Carregar Documentos de Perícia
            MessageBox.Show("Carregando documentos para perícia", "Perícia", MessageBoxButton.OK, MessageBoxImage.Information);

            // Exibir uma caixa de diálogo para selecionar arquivos
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Documentos (*.pdf;*.doc;*.docx)|*.pdf;*.doc;*.docx|Todos os arquivos (*.*)|*.*";
            openFileDialog.Title = "Selecione os documentos para perícia";

            if (openFileDialog.ShowDialog() == true)
            {
                // Processar os arquivos selecionados
                string[] arquivosSelecionados = openFileDialog.FileNames;
                if (arquivosSelecionados.Length > 0)
                {
                    string mensagem = $"Foram selecionados {arquivosSelecionados.Length} arquivo(s):\n";
                    foreach (string arquivo in arquivosSelecionados)
                    {
                        mensagem += System.IO.Path.GetFileName(arquivo) + "\n";
                    }
                    MessageBox.Show(mensagem, "Arquivos Selecionados", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void PericiaPreprocessar_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para Preprocessar documentos de Perícia
            MessageBox.Show("Iniciando preprocessamento de documentos para perícia", "Perícia", MessageBoxButton.OK, MessageBoxImage.Information);

            // Aqui você pode adicionar a lógica para preprocessar os documentos
            // Por exemplo, extrair texto, identificar entidades, etc.

            // Simulação de processamento
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                MessageBox.Show("Preprocessamento concluído com sucesso!", "Perícia", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            timer.Start();
        }

        private void PericiaAnalisePrev_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para Análise Prévia de Perícia
            MessageBox.Show("Iniciando análise prévia dos documentos periciais", "Perícia", MessageBoxButton.OK, MessageBoxImage.Information);

            // Aqui você pode adicionar a lógica para realizar a análise prévia
            // Por exemplo, gerar relatórios, identificar inconsistências, etc.

            // Navegar para uma página de análise (exemplo)
            nowDisplay("WebViewMP");
            webViewMP.CoreWebView2.Navigate("https://intranet.mpdft.mp.br/sistemas/java/neogab/acompanhamento/pericia");
        }

        private void ConfiguracoesAmbiente_Click(object sender, RoutedEventArgs e)
        {
            // Abrir a janela de configurações
            var configDialog = new ConfigurationDialog();
            configDialog.Owner = this; // Define a janela principal como proprietária

            // Mostrar a janela de configurações como diálogo modal
            bool? result = configDialog.ShowDialog();

            // Se o usuário clicou em Salvar (DialogResult = true)
            if (result == true)
            {
                // Aplicar as configurações atualizadas (se necessário)
                // Por exemplo, atualizar caminhos de diretórios, modelos, etc.

                // Exibir mensagem de confirmação (opcional)
                MessageBox.Show("As configurações foram aplicadas com sucesso.", "Configurações", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ConfiguracoesBancoDados_Click(object sender, RoutedEventArgs e)
        {
            // Abrir a janela de configurações do banco de dados
            var dbSetupWindow = new DatabaseSetupWindow();
            dbSetupWindow.Owner = this; // Define a janela principal como proprietária

            // Mostrar a janela de configurações como diálogo modal
            dbSetupWindow.ShowDialog();
        }

        private void ProcessarPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir a janela de processamento de PDF
                var pdfProcessingWindow = new PDFProcessingWindow();
                pdfProcessingWindow.Owner = this; // Define a janela principal como proprietária

                // Mostrar a janela de processamento como diálogo modal
                bool? result = pdfProcessingWindow.ShowDialog();

                if (result == true)
                {
                    // Obter os arquivos selecionados
                    var selectedFiles = pdfProcessingWindow.SelectedFiles;

                    if (selectedFiles.Count > 0)
                    {
                        // Exibir mensagem com os arquivos selecionados (temporário para teste)
                        MessageBox.Show($"Arquivos selecionados para processamento: {selectedFiles.Count}\n\n" +
                                      $"{string.Join("\n", selectedFiles)}",
                                      "Processamento de PDF", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Implementar o processamento real dos arquivos PDF
                        ProcessarArquivosPDF(selectedFiles);
                    }
                }
            }
            catch (Exception ex)
            {
                // Capturar e exibir qualquer exceção que ocorra
                MessageBox.Show($"Ocorreu um erro ao processar PDFs: {ex.Message}\n\nDetalhes: {ex.StackTrace}",
                              "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ObservableCollection<ProcessoViewModel> PreencheDataGridTeste(int numeroDeLinhas)
        {
            var processos = new ObservableCollection<ProcessoViewModel>();

            for (int i = 0; i < numeroDeLinhas; i++)
            {
                var processo = new ProcessoViewModel
                {
                    ExpedienteID = $"ID-{i + 1}",
                    Numero = $"{i + 1000000}-{i % 100:00}.{i % 10000:0000}.{i % 10:0}.{i % 100:00}.{i % 10000:0000}",
                    Prazo = DateTime.Now.AddDays(i),
                    Classe = $"Classe {i + 1}"
                };

                // Adicionando triagens fictícias
                for (int j = 0; j < 2; j++) // 2 triagens por processo
                {
                    processo.Triagem.Add($"Triagem {j + 1} do Processo {i + 1}");
                }
                // Adicionando partes fictícias
                for (int j = 0; j < 3; j++) // 3 partes por processo
                {
                    processo.Partes.Add($"Parte {j + 1} do Processo {i + 1}");
                }
                // Adicionando detalhes fictícios
                for (int k = 0; k < 2; k++) // 2 detalhes por processo
                {
                    processo.Detalhes.Add($"Detalhe {k + 1} do Processo {i + 1}");
                }
                processos.Add(processo);
            }

            return processos;
        }
        private void ProcessarArquivosPDF(ObservableCollection<string> arquivosPDF)
        {
            try
            {
                // Criar uma instância da view NeoGabView
                var neoGabView = new Views.NeoGabView();

                // Copiar os arquivos selecionados para o diretório de processamento
                string diretorioDestino = "d:\\PJe\\Baixados";

                // Verificar se o diretório existe, caso contrário, criar
                if (!Directory.Exists(diretorioDestino))
                {
                    Directory.CreateDirectory(diretorioDestino);
                }

                // Copiar cada arquivo para o diretório de processamento
                foreach (string arquivoPDF in arquivosPDF)
                {
                    string nomeArquivo = IOPath.GetFileName(arquivoPDF);
                    string caminhoDestino = IOPath.Combine(diretorioDestino, nomeArquivo);

                    // Se já existir um arquivo com o mesmo nome, excluir
                    if (File.Exists(caminhoDestino))
                    {
                        File.Delete(caminhoDestino);
                    }

                    // Copiar o arquivo
                    File.Copy(arquivoPDF, caminhoDestino);
                }

                // Iniciar o processamento dos arquivos
                neoGabView.separaPecasPDF();

                MessageBox.Show("Processamento concluído com sucesso!", "Processamento de PDF", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao processar arquivos PDF: {ex.Message}\n\nDetalhes: {ex.StackTrace}",
                              "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        static ProcessoPolos ExtrairNomesPolo(string campo)
        {
            ProcessoPolos processo = new ProcessoPolos();

            // Dividindo a string nas linhas
            string[] linhas = campo.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            bool isAtivo = false;

            foreach (var linha in linhas)
            {
                if (linha.StartsWith("Polo ativo:"))
                {
                    // Processando polo ativo
                    string nomeAtivo = linha.Substring("Polo ativo:".Length).Trim();
                    if (!string.IsNullOrEmpty(nomeAtivo))
                    {
                        processo.PoloAtivo.Add(nomeAtivo);
                    }
                    isAtivo = true; // Indica que as próximas linhas pertencem ao polo ativo
                }
                else if (linha.StartsWith("Polo passivo:"))
                {
                    // Processando polo passivo
                    string nomePassivo = linha.Substring("Polo passivo:".Length).Trim();
                    if (!string.IsNullOrEmpty(nomePassivo))
                    {
                        processo.PoloPassivo.Add(nomePassivo);
                    }
                    isAtivo = false; // Indica que as próximas linhas pertencem ao polo passivo
                }
                else if (isAtivo)
                {
                    // Continuação de um nome no polo ativo
                    processo.PoloAtivo[processo.PoloAtivo.Count - 1] += " " + linha.Trim();
                }
                else
                {
                    // Continuação de um nome no polo passivo
                    processo.PoloPassivo[processo.PoloPassivo.Count - 1] += " " + linha.Trim();
                }
            }

            return processo;
        }





    }
}
