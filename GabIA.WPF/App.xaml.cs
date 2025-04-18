using Autofac;
using GabIA.BLL;
using GabIA.DAL;
using GabIA.ENT;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using DocumentFormat.OpenXml.Presentation;

namespace GabIA.WPF
{
    public partial class App : Application
    {
        public IContainer? Container { get; private set; }

        private RemoverTextoInutil removerTextoInutil;

        private DataManager _dataManager;


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // textos sempre remover
            removerTextoInutil = RemoverTextoInutil.Instance;
            removerTextoInutil.CarregarTextosPermanentes("d:\\pje\\dados\\remover.txt");

            // Registre o provedor de codificação
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Carregue a configuração do arquivo appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath("D:\\PJe\\Config\\")
                .AddJsonFile("appsettings.json")
                .Build();

            // Configure o contêiner Autofac
            Container = IoCConfig.ConfigureContainer(configuration);

            Application.Current.Properties["ID_PJ"] = 123;
            Application.Current.Properties["PJ"] = "0000000-00.2000.8.07.0008";


            ///////////////////////DataManager.LimparTodasAsTabelas();

            // Inicie a janela principal usando o contêiner
            var scope = Container.BeginLifetimeScope();
            //var mainWindow = scope.Resolve<MainWindow>();
            var wpfPrincipal = scope.Resolve<wpfPrincipal>(); // Certifique-se de que WpfPrincipal está registrada no Autofac

            /*
            OperacoesBLL operacoes = new OperacoesBLL(/* coloque aqui as dependências necessárias );
            // Obtem o objeto ProcessoCompletoENT atualizado
            List<AtoProcessualENT> atosDoProcesso = operacoes.BuscarTodosOsAtosDoProcesso("0702153 -66.2023.8.07.0008");



            string dirModeloDeLinguagem = Path.Combine("d:\\PJe\\Processos\\0702153-66.2023.8.07.0008\\ModeloDeLinguagem");
            dirModeloDeLinguagem = Path.Combine(dirModeloDeLinguagem, "JsonlOriginal");
            string origem = @"d:\PJe\Processos\0702153-66.2023.8.07.0008\PecasProcessuais\PecasTxt_R";
            PreprocessamentoTextoIA preprocessamento = new PreprocessamentoTextoIA();
            preprocessamento.ProcessFilesInDirectoryToJson(origem, dirModeloDeLinguagem, atosDoProcesso);




            //string fileNameTemp = @"D:\PJe\Dados\dados_tabela_033.txt";
            dirModeloDeLinguagem = Path.Combine("d:\\PJe\\Processos\\0702153-66.2023.8.07.0008\\ModeloDeLinguagem");
            dirModeloDeLinguagem =Path.Combine(dirModeloDeLinguagem, "JsonlOriginal");
            //// Em algum lugar de MainWindow.xaml.cs
            //var operacoesBLL = new OperacoesBLL();
            //operacoesBLL.LoadDataFromCSV(fileNameTemp);
            //Cria o processo texto json
            string destinatioDirectoryJson = Path.Combine(Path.GetDirectoryName(dirModeloDeLinguagem), "JsonlOriginal");

            //WriteProcessoCompleto(processoAtualizado);
            string dirModeloDeLinguagem = Path.Combine("d:\\PJe\\Processos\\0702153-66.2023.8.07.0008\\ModeloDeLinguagem");
            dirModeloDeLinguagem = Path.Combine(dirModeloDeLinguagem, "JsonlOriginal");
            PreprocessamentoTextoIA sumarizaParalelo = new PreprocessamentoTextoIA();
            sumarizaParalelo.DeserializeJsonFile("d:\\PJe\\Processos\\0702153-66.2023.8.07.0008\\ModeloDeLinguagem\\JsonlReceived\\156099597.json", "d:\\PJe\\Processos\\0702153-66.2023.8.07.0008\\ModeloDeLinguagem\\JsonlReceived\\156099597.txt");

            dirModeloDeLinguagem = Path.Combine(Path.GetDirectoryName(dirModeloDeLinguagem), "JsonlReceived");
            sumarizaParalelo.separaLinhasJson("d:\\PJe\\Processos\\0702153-66.2023.8.07.0008\\ModeloDeLinguagem\\JsonLoriginal\\0702153-66.2023.8.07.0008B.jsonl", dirModeloDeLinguagem); */
            //testesOperacionais();
            wpfPrincipal.Show();
        }
        public async void testesOperacionais()
        {
            string dirModeloDeLinguagem = Path.Combine("d:\\PJe\\Processos\\0704628-29.2022.8.07.0008\\ModeloDeLinguagem");
            dirModeloDeLinguagem = Path.Combine(dirModeloDeLinguagem, "JsonlOriginal");
            string origem = @"d:\PJe\Processos\0704628-29.2022.8.07.0008\PecasProcessuais\PecasTxt";

            //agora vamos atualizar o banco de dados
            Debug.WriteLine("Atualizando a base de dados");

            _dataManager = new DataManager();
            // Executa o método para carregar os dados
            await _dataManager.CarregarDadosProcessoAsync(@"d:\PJe\Processos\0704628-29.2022.8.07.0008", "0704628-29.2022.8.07.0008");



            // este método resolve o problema das petições de encaminhamento "segue petição anexa"
            ProcessamentoDeTexto limpeza = new ProcessamentoDeTexto();
            limpeza.MoveConteudoIDCorreto(Path.Combine("d:\\PJe\\Processos", "0704628-29.2022.8.07.0008", "PecasProcessuais", "PecasTxt"));

            /*
            string dirModeloDeLinguagem = Path.Combine("d:\\PJe\\Processos\\0704628-29.2022.8.07.0008\\ModeloDeLinguagem");
            dirModeloDeLinguagem = Path.Combine(dirModeloDeLinguagem, "JsonlOriginal");
            string origem = @"d:\PJe\Processos\0704628-29.2022.8.07.0008\PecasProcessuais\PecasTxt_R";
            PreprocessamentoTextoIA preprocessamento = new PreprocessamentoTextoIA();
            preprocessamento.ProcessFilesInDirectoryToJson(origem, dirModeloDeLinguagem, atosDoProcesso);
            */


            OperacoesBLL operacoes = new OperacoesBLL();
            List<AtoProcessualENT> atosDoProcesso = operacoes.BuscarTodosOsAtosDoProcesso("0704628-29.2022.8.07.0008");

            //já fiz depois de separar peças pdf
            OrganizaPecas toJson = new OrganizaPecas();
            toJson.RegularizaArquivosPorTamanhoToJson(origem, dirModeloDeLinguagem, 7000, atosDoProcesso);

            APIRequest processa_IA = new APIRequest();

            string promptFilePath = "d:\\PJe\\Dados\\Templates\\Corrige.txt";
            string systemContent = processa_IA.ReadPromptFromFile(promptFilePath);
            string inputDirectory = "D:\\PJe\\Processos\\0704628-29.2022.8.07.0008\\ModeloDeLinguagem\\JsonlOriginal";
            string outputDirectory = "D:\\PJe\\Processos\\0704628-29.2022.8.07.0008\\ModeloDeLinguagem\\JsonlReceived";
            string apiKey = "d:\\PJe\\App_cpp\\Senha.py";

            await APIRequest.ProcessDirectory(inputDirectory, outputDirectory, apiKey, systemContent);

            PreprocessamentoTextoIA sumarizaParalelo = new PreprocessamentoTextoIA();
            sumarizaParalelo.ProcessarDiretorioEmParalelo(dirModeloDeLinguagem, "corrige", "0702153-66.2023.8.07.0008O.Json");

            //MoveConteudoIDCorreto
        }


        public void WriteProcessoCompleto(ProcessoCompletoENT processoCompleto)
        {
            Debug.WriteLine($"ID: {processoCompleto.IdProcessoCompleto}");
            Debug.WriteLine($"Tipo: {processoCompleto.Tipo}");
            Debug.WriteLine($"Movimento: {processoCompleto.Movimento}");
            Debug.WriteLine($"NumeroProcesso: {processoCompleto.NumeroProcesso}");
            Debug.WriteLine($"DataDaAbertura: {processoCompleto.DataDaAbertura}");
            Debug.WriteLine($"PrazoParaConsulta: {processoCompleto.PrazoParaConsulta}");
            Debug.WriteLine($"MembroResponsavel: {processoCompleto.MembroResponsavel}");
            Debug.WriteLine($"Promotoria: {processoCompleto.Promotoria}");
            Debug.WriteLine($"IdPJ: {processoCompleto.IdPJ}");

            Debug.WriteLine("AtosProcessuais:");
            foreach (var ato in processoCompleto.AtosProcessuais)
            {
                Debug.WriteLine($"ID: {ato.IdAtoProcessual}, Tipo: {ato.Tipo}, Descricao: {ato.Resumo}, Data: {ato.Data}");
            }

            // Acessar Partes do Processo através de ProcessoJudicial
            if (processoCompleto.ProcessoJudicial != null)
            {
                Debug.WriteLine("Elementos do Processo Judicial:");
                foreach (var elemento in processoCompleto.ProcessoJudicial.Elementos)
                {
                    // Exemplo de como você pode querer exibir informações dos elementos
                    Debug.WriteLine($"Elemento ID: {elemento.IdElemento}, PessoaID: {elemento.PartesDoProcesso}, CausaDePedirID: {elemento.CausasDePedir}, PedidoID: {elemento.Pedidos}");
                }
            }
        }

    }
}
