using GabIA.WPF.ViewModels;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using IOPath = System.IO.Path;
using System.Windows.Markup;
using System.Xml;
using System.Collections.ObjectModel;
using GabIA.ENT;
using GabIA.BLL;
using GabIA.WPF.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;



namespace GabIA.WPF.Views
{
    /// <summary>
    /// Interaction logic for RedView.xaml
    /// </summary>

    public partial class ProcessosView : UserControl
    {
        private AtoProcessualConsultaBLL _atoProcessualBLL;

        

        private ProcessosViewModel viewModel;
        public ProcessosView()
        {
            InitializeComponent();
            viewModel = (ProcessosViewModel)DataContext;
        }

        private int _linesPerPage = 12;
        private List<string> _csvLines;
        private Border _ultimoBorderClicado;
        private int _currentPageIndex = 0;


        // Define o evento
        public event EventHandler ProcessoSelected;


        // No ProcessosView.xaml.cs
        private MainWindow mainWindow;

        public ProcessosView(MainWindow mainWindow)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;

            viewModel = new ProcessosViewModel();  // Aqui está a mudança: removemos a palavra "var"
            DataContext = viewModel;

            // Substituir a chamada de leitura do CSV para buscar dados do banco de dados
            viewModel.LoadProcessosCSVFromDB();


            // Exibir a primeira página de dados, se houver
            if (viewModel.Processos_DB.Count > 0)
            {
                ShowProcessoPage(viewModel.GetProcessosByPage(0));
            }
        }



        private void CsvListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ProcessoAtivo = "0704517-45.2022.8.07.0008";
            // Invoca o evento
            ProcessoSelected?.Invoke(this, EventArgs.Empty);
        }

        private void ShowProcessoPage(ObservableCollection<ProcessoCSV> processos)
        {
            for (int i = 0; i < processos.Count; i++)
            {
                ProcessoCSV processo = processos[i];
                TextBlock txtBlock = new TextBlock
                {
                    FontFamily = new FontFamily("Oswald"),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(8, 5, 0, 0)
                };

                txtBlock.Inlines.Add(new Run { Text = "Processo: ", FontSize = 12, Foreground = Brushes.White });
                txtBlock.Inlines.Add(new LineBreak());
                txtBlock.Inlines.Add(new Run { Text = processo.Processo, FontSize = 13, Foreground = Brushes.White });
                txtBlock.Inlines.Add(new LineBreak());
                txtBlock.Inlines.Add(new LineBreak());
                txtBlock.Inlines.Add(new Run { Text = "Data da Abertura: ", FontSize = 12, Foreground = Brushes.White });
                txtBlock.Inlines.Add(new LineBreak());
                txtBlock.Inlines.Add(new Run { Text = processo.DataDaAbertura.ToString("dd/MM/yyyy"), FontSize = 13, Foreground = Brushes.White });
                txtBlock.Inlines.Add(new LineBreak());
                txtBlock.Inlines.Add(new LineBreak());
                txtBlock.Inlines.Add(new Run { Text = "Prazo para Consulta: ", FontSize = 12, Foreground = Brushes.White });
                txtBlock.Inlines.Add(new LineBreak());
                DateTime dataDaAbertura = processo.DataDaAbertura;
                DateTime prazoParaConsulta = dataDaAbertura.AddDays(10);
                TimeSpan diferenca = DateTime.Now - prazoParaConsulta;

                int diasDeDiferenca = diferenca.Days; // Isto será positivo se a data atual estiver depois do prazo, e negativo se a data atual estiver antes do prazo.

                txtBlock.Inlines.Add(new Run { Text = diasDeDiferenca.ToString(), FontSize = 13, Foreground = Brushes.White });
                txtBlock.Inlines.Add(new LineBreak());
                txtBlock.Inlines.Add(new LineBreak());
                txtBlock.Inlines.Add(new Run { Text = "Membro Responsável: ", FontSize = 12, Foreground = Brushes.White });
                txtBlock.Inlines.Add(new LineBreak());
                txtBlock.Inlines.Add(new Run { Text = processo.MembroResponsavel, FontSize = 13, Foreground = Brushes.White });
                txtBlock.Inlines.Add(new LineBreak());
                txtBlock.Inlines.Add(new LineBreak());

                string borderName = GenerateBorderName(i);
                Border border = FindName(borderName) as Border;

                // Adicione o TextBlock ao Border
                if (border != null)
                {
                    if (border.Child is Grid originalGrid)
                    {
                        // Crie um novo Grid e copie as propriedades do grid original
                        Grid newGrid = new Grid();
                        newGrid.Height = originalGrid.Height;
                        newGrid.Width = originalGrid.Width;
                        newGrid.HorizontalAlignment = originalGrid.HorizontalAlignment;
                        newGrid.VerticalAlignment = originalGrid.VerticalAlignment;
                        newGrid.Margin = originalGrid.Margin;


                        // Adicione os elementos filhos do grid original ao novo grid
                        for (int j = 0; j < originalGrid.Children.Count; j++)
                        {
                            UIElement originalChild = originalGrid.Children[j];
                            UIElement clonedChild = CloneElement(originalChild);

                            // Se o elemento filho clonado for um TextBlock, limpe seu conteúdo
                            if (clonedChild is TextBlock clonedTextBlock)
                            {
                                clonedTextBlock.Inlines.Clear();
                            }

                            newGrid.Children.Add(clonedChild);
                        }


                        // Adicione o novo TextBlock ao novo grid
                        newGrid.Children.Add(txtBlock);

                        // Substitua o grid original pelo novo grid
                        border.Child = newGrid;
                    }
                }
            }
        }
        public static T CloneElement<T>(T original) where T : UIElement
        {
            string elementXaml = XamlWriter.Save(original);
            StringReader stringReader = new StringReader(elementXaml);
            XmlTextReader xmlTextReader = new XmlTextReader(stringReader);
            T clonedElement = (T)XamlReader.Load(xmlTextReader);
            return clonedElement;
        }

        private string GenerateBorderName(int index)
        {
            int row = index / 6 + 1;
            int column = index % 6 + 1;
            return $"Border{row}{column}";
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

        public void todosBackgroundDefault(Border borderManter)
        {
            for (int i = 11; i <= 16; i++)
            {
                Border border = (Border)FindName("Border" + i.ToString());
                if (border != borderManter)
                {
                    TextBlock textBlock = (TextBlock)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(border, 0), 1);
                    AtualizarTextBlockEBorder(border, textBlock);
                }
            }
            for (int i = 21; i <= 26; i++)
            {
                Border border = (Border)FindName("Border" + i.ToString());
                if (border != borderManter)
                {
                    TextBlock textBlock = (TextBlock)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(border, 0), 1);
                    AtualizarTextBlockEBorder(border, textBlock);
                }
            }
        }
        public void AtualizarTextBlockEBorder(Border border, TextBlock textBlock)
        {
            // Restaure a cor o background original do Border interno
            Border innerBorder = (Border)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(border, 0), 0);
            innerBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5E5E5E"));
            innerBorder.Opacity = 0.3;

            // Restaure a cor da fonte original do TextBlock
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));

            // Restaure o gradiente original do Border externo
            border.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(0x00, 0x00, 0x00), 0.1),
                    new GradientStop(Color.FromRgb(0x24, 0x24, 0x24), 0.8)
                }
            };
        }
        
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border borderClicado = sender as Border;

            // Se já houver um Border clicado anteriormente, retorne-o ao modo escuro
            if (_ultimoBorderClicado != null)
            {
                if (VisualTreeHelper.GetChildrenCount(_ultimoBorderClicado) > 0)
                {
                    var child1 = VisualTreeHelper.GetChild(_ultimoBorderClicado, 0);
                    if (VisualTreeHelper.GetChildrenCount(child1) > 1)
                    {
                        TextBlock textBlockAnterior = (TextBlock)VisualTreeHelper.GetChild(child1, 1);
                        AtualizarTextBlockEBorder(_ultimoBorderClicado, textBlockAnterior);
                    }
                }
            }

            // Mude o Border clicado para o modo light
            TextBlock textBlockClicado = (TextBlock)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(borderClicado, 0), 1);
            borderClicado.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(0x0B, 0x18, 0x33), 0.0),
                    new GradientStop(Color.FromRgb(0x13, 0x2A, 0x57), 1.0)
                }

            };
            textBlockClicado.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Acesse as informações vinculadas do TextBlock
            var informacaoVinculada = textBlockClicado.DataContext;

            // Atualize o último Border clicado
            _ultimoBorderClicado = borderClicado;

            string borderName = borderClicado.Name;

            int index = ExtractProcessoIndex(borderName);
            // Obtenha o índice do Border clicado

            // Calcule a página atual
            //int page = index / _linesPerPage;
            int page = _currentPageIndex;


            // Obtenha o ProcessoCSV correspondente ao índice
            //ProcessoCSV processoAtivo = GetProcessoByIndex(index % _linesPerPage, page);
            ProcessoCSV processoAtivo = GetProcessoByIndex(index, page);

            Singleton.Instance.ProcessoAtivo = GetProcessoByIndex(index, page);

            DateTime dataDaAbertura = processoAtivo.DataDaAbertura;
            DateTime prazoParaConsulta = dataDaAbertura.AddDays(10);
            TimeSpan diferenca = DateTime.Now - prazoParaConsulta;

            int diasDeDiferenca = diferenca.Days; // Isto será positivo se a data atual estiver depois do prazo, e negativo se a data atual estiver antes do prazo.

            DateTime dataA = processoAtivo.DataDaAbertura;
            Debug.WriteLine("Data da Abertura: " + dataA);

            DateTime prazoC = dataDaAbertura.AddDays(10);
            Debug.WriteLine("Prazo para Consulta: " + prazoC);

            DateTime dataAtual = DateTime.Now;
            Debug.WriteLine("Data Atual: " + dataAtual);

            TimeSpan diffD = dataAtual - prazoC;
            Debug.WriteLine("Diferença: " + diffD.Days);

            processoAtivo.PrazoParaConsulta = diasDeDiferenca;

            ListBoxItem formattedListBoxItem = UpdateFormattedListView(processoAtivo, CsvListView);
            ((ObservableCollection<ListBoxItem>)CsvListView.ItemsSource).Add(formattedListBoxItem);


            // Aqui você pode preencher seu TextBox
            MainWindow.DadosDoProcessoBox.Text = UpdateFormattedTextBox(processoAtivo); // Substitua por um método para formatar os dados do Ato Processual


            OperacoesBLL _operacoesBLL = new OperacoesBLL();

            // Obter todos os tipos de ato processual
            var tiposAtoProcessual = _operacoesBLL.GetTiposAtoProcessual();

            // Criar um dicionário que mapeia o ID do tipo de ato processual para a descrição do tipo
            var tipoAtoProcessualMap = tiposAtoProcessual.ToDictionary(t => t.Id_Tipo_Ato_processual, t => t.Tipo.ToLower());


            _atoProcessualBLL = new AtoProcessualConsultaBLL(); // Inicialize a classe BLL correspondente

            List<AtoProcessualConsulta> atosProcessuaisII = _atoProcessualBLL.ObterListaAtoProcessualPorIdProcessoJ(processoAtivo.idPJ);

            List<AtoProcessualConsulta> atosProcessuais = _atoProcessualBLL.ObterListaAtoProcessualPorIdProcessoJ(processoAtivo.idPJ);
            // Verifique se os atos processuais foram recuperados corretamente
            if (atosProcessuais != null && atosProcessuais.Count > 0)
            {
                // Buscar o último ato tipo decisão ou despacho
                AtoProcessualConsulta ultimoAto = atosProcessuaisII
                    .Where(ap => ap.Resumo.ToLower() == "decisão" || ap.Resumo.ToLower() == "despacho")
                    .OrderByDescending(ap => ap.DataInclusao) // Supondo que existe um campo de Data
                    .FirstOrDefault();


                //Debug.WriteLine(tipoAtoProcessualMap.ContainsKey(ultimoAto.Tipo));

                if (ultimoAto != null )
                {
                    RichTextBoxAto.Document.Blocks.Clear();
                    //RichTextBoxAto.Document.Blocks.Add(new Paragraph(new Run(tipoAtoProcessualMap[ultimoAto.Tipo]))); // Ajuste para o campo correto
                    Debug.WriteLine(processoAtivo.Processo);
                    // Procurar o arquivo resumo
                    string arquivoPdf = processoAtivo.Processo;
                    string diretorioBase = IOPath.Combine("D:\\PJe\\Processos", processoAtivo.Processo);
                    string diretorioPecas = IOPath.Combine(diretorioBase, "PecasProcessuais");
                    string diretorioResumos = IOPath.Combine(diretorioBase, "PecasProcessuais","PecasTxt_R");

                    // Procurar o arquivo com base no atoProcessual.idMovimento
                    //string arquivoResumo = Directory.GetFiles(diretorioResumos, $"*{ultimoAto.IdMovimento}*").FirstOrDefault();
                    string arquivoResumo = IOPath.Combine(diretorioResumos, "P" + ultimoAto.IdMovimento.ToString() + "_R.txt");
                    // Se o arquivo foi encontrado
                    if (!string.IsNullOrEmpty(arquivoResumo))
                    {
                        // Ler o conteúdo do arquivo
                        if(File.Exists(arquivoResumo))
                        {
                            string conteudoResumo = File.ReadAllText(arquivoResumo);

                            // Adicionar o conteúdo do arquivo ao RichTextBox
                            RichTextBoxAto.Document.Blocks.Add(new Paragraph(new Run(conteudoResumo)));
                                                                                       // Preencher o RichTextBox com o último ato

                        }

                    }
                }
            }
        }

        private ListBoxItem UpdateFormattedListView(ProcessoCSV processo, ListView listView)
        {
            if (listView.ItemsSource == null)
            {
                listView.ItemsSource = new ObservableCollection<ListBoxItem>();
            }



            // Limpar o ItemsSource do ListView
            ((ObservableCollection<ListBoxItem>)listView.ItemsSource).Clear();

            // Crie um novo ListBoxItem
            ListBoxItem listBoxItem = new ListBoxItem();

            // Crie um StackPanel para adicionar os campos formatados
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;

            Label titleLabel = new Label
            {
                Content = $"Detalhes do Processo nº {processo.Processo}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            stackPanel.Children.Add(titleLabel);

            // Adicione os detalhes do processo
            AddDetail(stackPanel, "Movimento", processo.Movimento);
            AddDetail(stackPanel, "Processo", processo.Processo);
            AddDetail(stackPanel, "Data da Abertura", processo.DataDaAbertura.ToString());
            AddDetail(stackPanel, "Prazo para Consulta", processo.PrazoParaConsulta.ToString());
            AddDetail(stackPanel, "Polo Ativo", processo.PoloAtivo);
            AddDetail(stackPanel, "Polo Passivo", processo.PoloPassivo);
            //AddDetail(stackPanel, "Classe", processo.Classe);
            AddDetail(stackPanel, "Membro Responsável", processo.MembroResponsavel);
            AddDetail(stackPanel, "Promotoria", processo.Promotoria);

            // Adicione o StackPanel ao ListBoxItem
            listBoxItem.Content = stackPanel;

            return listBoxItem;
        }

        private string UpdateFormattedTextBox(ProcessoCSV processo)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Detalhes do Processo nº {processo.Processo}\n");

            stringBuilder.AppendLine(AddDetail("Movimento", processo.Movimento));
            stringBuilder.AppendLine(AddDetail("Processo", processo.Processo));
            stringBuilder.AppendLine(AddDetail("Data da Abertura", processo.DataDaAbertura.ToString()));
            stringBuilder.AppendLine(AddDetail("Prazo para Consulta", processo.PrazoParaConsulta.ToString()));
            stringBuilder.AppendLine(AddDetail("Polo Ativo", processo.PoloAtivo));
            stringBuilder.AppendLine(AddDetail("Polo Passivo", processo.PoloPassivo));
            stringBuilder.AppendLine(AddDetail("Membro Responsável", processo.MembroResponsavel));
            stringBuilder.AppendLine(AddDetail("Promotoria", processo.Promotoria));

            return stringBuilder.ToString();
        }

        private string AddDetail(string label, string detail)
        {
            return $"{label}: {detail}";
        }


        // Adiciona um detalhe ao painel
        private void AddDetail(StackPanel stackPanel, string header, string detail)
        {
            TextBlock fieldTextBlock = new TextBlock
            {
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Ivory
            };

            // Adicione o nome do campo e o valor correspondente
            fieldTextBlock.Inlines.Add(new Run { Text = $"{header}: ", FontSize = 14, Foreground = Brushes.White });
            fieldTextBlock.Inlines.Add(new Run { Text = detail });

            // Adicione o TextBlock formatado ao StackPanel
            stackPanel.Children.Add(fieldTextBlock);
        }





        public ProcessoCSV GetProcessoByIndex(int index, int page)
        {
            Debug.WriteLine("page??");

            //viewModel.GetProcessosByPage(0);

            return viewModel.GetProcessosByPage(page)[index];
        }

        private ListBoxItem UpdateFormattedListView(string csvLine, string[] header, string processoNumber, ListBox csvListView)
        {
            // Limpar o ItemsSource do ListBox
            ((ObservableCollection<ListBoxItem>)csvListView.ItemsSource).Clear();

            // Crie um novo ListBoxItem
            ListBoxItem listBoxItem = new ListBoxItem();

            // Crie um StackPanel para adicionar os campos formatados
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;

            // Adicione o título "Detalhes do Processo nº [numerodoprocesso]"
            TextBlock titleTextBlock = new TextBlock
            {
                Text = $"Detalhes do Processo nº {processoNumber}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            stackPanel.Children.Add(titleTextBlock);

            // Separe a linha CSV em colunas usando o separador '|'
            string[] columns = csvLine.Split('|');

            for (int i = 0; i < columns.Length; i++)
            {
                // Crie um novo TextBlock para cada campo formatado
                TextBlock fieldTextBlock = new TextBlock
                {
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Ivory
                };

                // Adicione o nome do campo do cabeçalho CSV e o valor da coluna correspondente
                fieldTextBlock.Inlines.Add(new Run { Text = $"{header[i]}: ", FontSize = 14, Foreground = Brushes.White });
                fieldTextBlock.Inlines.Add(new Run { Text = columns[i] });

                // Adicione o TextBlock formatado ao StackPanel
                stackPanel.Children.Add(fieldTextBlock);
            }

            // Adicione o StackPanel ao ListBoxItem
            listBoxItem.Content = stackPanel;

            return listBoxItem;
        }




        private TextBlock CreateFormattedTextBlock(string text, int fontSize, bool isBold, Brush foregroundColor)
        {
            TextBlock txtBlock = new TextBlock
            {
                FontSize = fontSize,
                Foreground = foregroundColor
            };

            if (isBold)
            {
                txtBlock.FontWeight = FontWeights.Bold;
            }

            txtBlock.Inlines.Add(new Run { Text = text });

            return txtBlock;
        }


        private int ExtractProcessoIndex(string borderName)
        {
            if (borderName == null)
                return -1;

            int row = int.Parse(borderName[6].ToString()) - 1;
            int column = int.Parse(borderName[7].ToString()) - 1;
            //IndiceGeral.Text = $"Indice: {row * 6 + column}";
            return row * 6 + column;
        }
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            string[] dirs = Directory.GetDirectories(@"D:\PJe\Processos");

            // Cria uma lista para armazenar os nomes dos diretórios
            List<string> dirNames = new List<string>();

            foreach (string dir in dirs)
            {
                string subDir = IOPath.Combine(dir, "PecasProcessuais", "PecasTxt");
                // Verifica se o subdiretório existe e se contém arquivos
                if (Directory.Exists(subDir) && Directory.EnumerateFiles(subDir).Any())
                {
                    var dirInfo = new DirectoryInfo(dir);
                    dirNames.Add(dirInfo.Name);
                }
            }

            // Ordena os nomes dos diretórios alfabeticamente
            dirNames.Sort();

            ComboBox comboBox = sender as ComboBox;

            foreach (string dirName in dirNames)
            {
                comboBox.Items.Add(dirName);
            }
        }



        private string GetCsvLineByIndex(int index, int page)
        {
            int startIndex = page * _linesPerPage;
            int lineIndex = startIndex + index;

            if (lineIndex >= 0 && lineIndex < _csvLines.Count)
            {
                return _csvLines[lineIndex];
            }
            else
            {
                return null;
            }
        }

        private void Border11_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border11);

            // Altere o gradiente do Border11
            Border11.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock11.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border11;
        }

        private void Border12_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border12);

            // Altere o gradiente do Border11
            Border12.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock12.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border12;
        }


        private void Border13_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border13);

            // Altere o gradiente do Border11
            Border13.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock13.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border13;
        }


        private void Border14_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border14);

            // Altere o gradiente do Border11
            Border14.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock14.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border14;
        }


        private void Border15_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border15);
            // Altere o gradiente do Border11
            Border15.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock15.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border15;
        }



        private void Border16_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border16);

            // Altere o gradiente do Border11
            Border16.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock16.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border16;
        }




        private void Border21_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border21);

            // Altere o gradiente do Border11
            Border21.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock21.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border21;
        }


        private void Border22_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border22);

            // Altere o gradiente do Border11
            Border22.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock22.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border22;
        }



        private void Border23_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border23);

            // Altere o gradiente do Border11
            Border24.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock23.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border23;
        }


        private void Border24_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border24);

            // Altere o gradiente do Border11
            Border24.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock24.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border24;
        }

        private void Border25_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border25);

            // Altere o gradiente do Border11
            Border25.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock25.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border11;
        }


        private void Border26_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Restaure o último Border clicado
            todosBackgroundDefault(Border26);

            // Altere o gradiente do Border11
            Border26.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0.75, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0xFF, 0xE5, 0x73), 0.1),
            new GradientStop(Color.FromRgb(0x45, 0x3E, 0x1F), 0.8)
        }
            };

            // Altere a cor da fonte do TextBlock
            txtBlock26.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));

            // Atualize o último Border clicado
            _ultimoBorderClicado = Border26;
        }


        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Botão Exit Clicado!");
        }

        private void BtnFirst_Click(object sender, RoutedEventArgs e)
        {
            ResetBordersAndClearListBox();
            if (_currentPageIndex != 0)
            {
                _currentPageIndex = 0;
                //Pagina.Text = $"Página: {_currentPageIndex.ToString()}";

                ShowProcessoPage(viewModel.GetProcessosByPage(_currentPageIndex));
            }
        }

        private void BtnLast_Click(object sender, RoutedEventArgs e)
        {
            ResetBordersAndClearListBox();
            int lastIndex = viewModel.GetTotalPages() - 1;
            if (_currentPageIndex != lastIndex)
            {
                _currentPageIndex = lastIndex;
                //Pagina.Text = $"Página: {_currentPageIndex.ToString()}";

                ShowProcessoPage(viewModel.GetProcessosByPage(_currentPageIndex));
            }
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            ResetBordersAndClearListBox();
            if (_currentPageIndex > 0)
            {
                _currentPageIndex--;
                //Pagina.Text = $"Página: {_currentPageIndex.ToString()}";
                ShowProcessoPage(viewModel.GetProcessosByPage(_currentPageIndex));
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            ResetBordersAndClearListBox();
            int totalPages = viewModel.GetTotalPages();
            if (_currentPageIndex < totalPages - 1)
            {
                _currentPageIndex++;
               // Pagina.Text = $"Página: {_currentPageIndex.ToString()}";
                ShowProcessoPage(viewModel.GetProcessosByPage(_currentPageIndex));
            }
        }

        private void ResetBordersAndClearListBox()
        {
            // Limpe o ListBox
            CsvListView.ItemsSource = new ObservableCollection<ListBoxItem>();

            // Lista com os nomes dos Borders e Borders internos que você deseja atualizar
            List<string> borderNames = new List<string> { "11", "12", "13", "14", "15", "16", "21", "22", "23", "24", "25", "26" };

            // Atualize os Borders e TextBlocks específicos
            foreach (string borderName in borderNames)
            {
                // Encontre o Border e TextBlock pelo nome
                Border border = (Border)this.FindName($"Border{borderName}");
                TextBlock textBlock = (TextBlock)this.FindName($"txtBlock{borderName}");

                // Verifique se o Border e o TextBlock foram encontrados
                if (border != null && textBlock != null)
                {
                    AtualizarTextBlockEBorder(border, textBlock);
                }
            }

            // Redefina _ultimoBorderClicado para null
            _ultimoBorderClicado = null;
        }

        private void btnAbreDetalhes_Click(object sender, RoutedEventArgs e)
        {
            // Acessar o valor do item selecionado como string
            string selectedValue = IndiceGeral.SelectedItem as string;

            //var processoBuscado = viewModel.GetProcessoByNumero("0703034-38.2022.8.07.0021");
            string processoPJe = selectedValue;
            var processoBuscado = viewModel.GetProcessoByNumero(processoPJe);

            if (processoBuscado != null)
            {
                Singleton.Instance.ProcessoAtivo = processoBuscado;
                Debug.WriteLine(Singleton.Instance.ProcessoAtivo.Processo);
                //Invoca o evento
                ProcessoSelected?.Invoke(this, EventArgs.Empty);

                //MainWindow.DadosDoProcessoBox.Text = "Algum conteúdo";
            }
            else
            {
                MessageBox.Show("Processo não carregado na base de dados");
            }

        }
        public async void RegularizaPecasProcessoView(string processoNR)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string dirModeloDeLinguagem = IOPath.Combine("d:\\PJe\\Processos\\" + processoNR + "\\ModeloDeLinguagem");
            dirModeloDeLinguagem = IOPath.Combine(dirModeloDeLinguagem, "JsonlOriginal");
            string origem = @"d:\PJe\Processos\" + processoNR + @"\PecasProcessuais\PecasTxt";

            OperacoesBLL operacoes = new OperacoesBLL();
            List<AtoProcessualENT> atosDoProcesso = operacoes.BuscarTodosOsAtosDoProcesso(processoNR);

            OrganizaPecas toJson = new OrganizaPecas();
            toJson.RegularizaArquivosPorTamanhoToJson(origem, dirModeloDeLinguagem, 3000, atosDoProcesso);
            stopwatch.Stop();
            TimeSpan tempoDecorrido = stopwatch.Elapsed;
            //MessageBox.Show($"Processado: {processoNR}, em {tempoDecorrido} ");
        }

        public async void SumarizaPecasProcessoView(string processoNR)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string origem = @"d:\PJe\Processos\" + processoNR + @"\PecasProcessuais\PecasTxt_R";

            string pecasSumarizadas = IOPath.Combine("d:\\PJe\\Processos\\" + processoNR + "\\ModeloDeLinguagem");
            pecasSumarizadas = IOPath.Combine(pecasSumarizadas, "JsonRegOrigem");

            if (!Directory.Exists(pecasSumarizadas))
            {
                Directory.CreateDirectory(pecasSumarizadas);
            }


            OperacoesBLL operacoes = new OperacoesBLL();
            List<AtoProcessualENT> atosDoProcesso = operacoes.BuscarTodosOsAtosDoProcesso(processoNR);

            OrganizaPecas toJson = new OrganizaPecas();
            toJson.RegularizaArquivosPorTamanhoToJson(origem, pecasSumarizadas, 3000, atosDoProcesso);
            stopwatch.Stop();
            TimeSpan tempoDecorrido = stopwatch.Elapsed;
            //MessageBox.Show($"Processado: {processoNR}, em {tempoDecorrido} ");
        }



        private async void btnExec_Clean_Click(object sender, RoutedEventArgs e)
        {
            // Acessar o valor do item selecionado como string
            string selectedValue = IndiceGeral.SelectedItem as string;

            //var processoBuscado = viewModel.GetProcessoByNumero("0703034-38.2022.8.07.0021");
            string processoPJe = selectedValue;
            var processoBuscado = viewModel.GetProcessoByNumero(processoPJe);

            if (processoBuscado != null)
            {
                Singleton.Instance.ProcessoAtivo = processoBuscado;
                Debug.WriteLine(Singleton.Instance.ProcessoAtivo.Processo);


                string consoleAppPath = @"D:\DotNET\OpenAI-API\testesAPI\testesAPI\bin\Debug\net6.0\ProcessaParaleloAPI.exe";
                string inputDirectory = "D:\\PJe\\Processos\\"+ Singleton.Instance.ProcessoAtivo.Processo + "\\ModeloDeLinguagem\\JsonlOriginal";
                string outputDirectory = "D:\\PJe\\Processos\\"+ Singleton.Instance.ProcessoAtivo.Processo + "\\ModeloDeLinguagem\\JsonlReceived";
                string promptFilePath = "d:\\PJe\\Dados\\Templates\\Corrige.txt";
                string apiKey = "d:\\PJe\\App_cpp\\Senha.py";

                if (!Directory.Exists(inputDirectory))
                {
                    Directory.CreateDirectory(inputDirectory);
                }
                if (!Directory.EnumerateFiles(inputDirectory).Any())
                {
                    // Se não houver arquivos no diretório de entrada, executa RegularizaPecasProcessoView
                    RegularizaPecasProcessoView(Singleton.Instance.ProcessoAtivo.Processo);
                }

                if (Directory.EnumerateFiles(outputDirectory).Any())
                {
                    // Se houver arquivos no diretório de saída, informa ao usuário que o processo já foi regularizado
                    MessageBox.Show("O processo já foi regularizado");
                    return;
                }

                string arguments = $"\"{inputDirectory}\" \"{outputDirectory}\" \"{promptFilePath}\" \"{apiKey}\"";

                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = consoleAppPath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    Process process = new Process { StartInfo = startInfo };
                    process.Start();

                    // Se você precisa ler a saída do console
                    string output = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();


                    string outputDirectoryTxt = "D:\\PJe\\Processos\\" + Singleton.Instance.ProcessoAtivo.Processo + "\\PecasProcessuais\\PecasTxt_R";

                    if (!Directory.Exists(outputDirectoryTxt))
                    {
                        Directory.CreateDirectory(outputDirectoryTxt);
                    }

                    ExtractJsonMessageToTxt(outputDirectory, outputDirectoryTxt);

                    string outputDirectoryTxtS = "D:\\PJe\\Processos\\" + Singleton.Instance.ProcessoAtivo.Processo + "\\PecasProcessuais\\PecasTxt_S";

                    if (!Directory.Exists(outputDirectoryTxtS))
                    {
                        Directory.CreateDirectory(outputDirectoryTxtS);
                    }

                    SumarizaPecasProcessoView(Singleton.Instance.ProcessoAtivo.Processo);

                    consoleAppPath = @"D:\DotNET\OpenAI-API\testesAPI\testesAPI\bin\Debug\net6.0\ProcessaParaleloAPI.exe";
                    inputDirectory = "D:\\PJe\\Processos\\" + Singleton.Instance.ProcessoAtivo.Processo + "\\ModeloDeLinguagem\\JsonRegOrigem";
                    outputDirectory = "D:\\PJe\\Processos\\" + Singleton.Instance.ProcessoAtivo.Processo + "\\ModeloDeLinguagem\\JsonSumarized";
                    promptFilePath = "individual";
                    apiKey = "d:\\PJe\\App_cpp\\Senha.py";


                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    arguments = $"\"{inputDirectory}\" \"{outputDirectory}\" \"{promptFilePath}\" \"{apiKey}\"";


                    ProcessStartInfo startInfoII = new ProcessStartInfo
                    {
                        FileName = consoleAppPath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    Process processII = new Process { StartInfo = startInfoII };
                    processII.Start();

                    // Se você precisa ler a saída do console
                    string outputII = processII.StandardOutput.ReadToEnd();

                    processII.WaitForExit();

                    outputDirectoryTxt = "D:\\PJe\\Processos\\" + Singleton.Instance.ProcessoAtivo.Processo + "\\PecasProcessuais\\PecasTxt_S";

                    if (!Directory.Exists(outputDirectoryTxt))
                    {
                        Directory.CreateDirectory(outputDirectoryTxt);
                    }

                    ExtractJsonMessageToTxt(outputDirectory, outputDirectoryTxt);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao iniciar o aplicativo de console: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Processo não carregado na base de dados");
            }
        }

        public void ExtractJsonMessageToTxt(string directoryPath, string diretorioTxt)
        {
            // Verificar se o diretório existe
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Diretório {directoryPath} não existe.");
                return;
            }

            // Dicionário para armazenar as mensagens por ID do arquivo
            var messagesById = new Dictionary<string, StringBuilder>();

            // Obter todos os arquivos .json no diretório
            string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");

            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    // Extrair o ID do arquivo a partir do nome do arquivo
                    string fileId = IOPath.GetFileNameWithoutExtension(jsonFile).Split('[')[0];

                    // Ler o conteúdo do arquivo .json
                    string jsonContent = File.ReadAllText(jsonFile);

                    // Dividir a string em duas partes
                    string[] jsonParts = jsonContent.Split(new string[] { "}{" }, StringSplitOptions.None);

                    foreach (var part in jsonParts)
                    {
                        string validJson = part;

                        // Corrigir o json caso esteja incompleto
                        if (!part.StartsWith("{")) validJson = "{" + part;
                        if (!part.EndsWith("}")) validJson += "}";

                        // Parsear o conteúdo do .json
                        JObject jsonObject = JObject.Parse(validJson);

                        // Extrair a mensagem
                        string message = jsonObject["Choices"]?[0]?["Message"]?["Content"]?.ToString();

                        if (message != null)
                        {
                            // Adicionar a mensagem ao StringBuilder correspondente
                            if (!messagesById.ContainsKey(fileId))
                            {
                                messagesById[fileId] = new StringBuilder();
                            }

                            messagesById[fileId].Append(message);
                        }
                    }
                }
                catch (JsonReaderException ex)
                {
                    Console.WriteLine($"Erro ao parsear o arquivo {jsonFile}: {ex.Message}");
                }
            }

            // Gravar as mensagens em arquivos .txt
            foreach (var pair in messagesById)
            {
                // Criar o nome do arquivo .txt
                string txtFile = IOPath.Combine(diretorioTxt, $"{pair.Key}_R.txt");

                // Gravar a mensagem no arquivo .txt com codificação UTF-8
                File.WriteAllText(txtFile, pair.Value.ToString(), Encoding.UTF8);
            }
        }
    }
}
