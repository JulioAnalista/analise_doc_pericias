using GabIA.BLL;
using GabIA.ENT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace GabIA.WPF.ViewModels
{
    public class ProcessosViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ProcessoCSV> _processos_DB;

        private string _searchText = string.Empty;

        public ObservableCollection<ProcessoCSV> Processos_DB
        {
            get { return _processos_DB; }
            set
            {
                _processos_DB = value;
                OnPropertyChanged("Processos_DB");
            }
        }

        public ObservableCollection<ProcessoCSV> GetProcessosByPage(int pageIndex)
        {
            // Verifica se Processos_DB é null
            if (Processos_DB == null)
            {
                // Trata o caso em que Processos_DB é null
                // Por exemplo, você pode retornar uma nova ObservableCollection vazia
                return new ObservableCollection<ProcessoCSV>();
            }

            // Definindo o tamanho da página
            int pageSize = 12;  // ou qualquer outro valor que você escolheu para o tamanho da página

            // Use Linq para pegar a página específica
            var processosByPage = Processos_DB.Skip(pageIndex * pageSize).Take(pageSize);

            // Retornando a página de processos como ObservableCollection
            return new ObservableCollection<ProcessoCSV>(processosByPage);
        }

        ////private void FilterProcessos()
        ////{
        ////    if (string.IsNullOrEmpty(_searchText))
        ////    {
        ////        // Se o texto de pesquisa estiver vazio, mostre todos os processos
        ////        Processos_DB = new ObservableCollection<ProcessoCSV>(allProcessos);
        ////    }
        ////    else
        ////    {
        ////        // Se o texto de pesquisa não estiver vazio, filtre a lista de processos
        ////        Processos_DB = new ObservableCollection<ProcessoCSV>(allProcessos.Where(p => p.NumeroDoProcesso.StartsWith(_searchText)));
        ////    }
        ////}



        ////public string SearchText
        ////{
        ////    get { return _searchText; }
        ////    set
        ////    {
        ////        _searchText = value;
        ////        OnPropertyChanged(nameof(SearchText));

        ////        // Atualize a lista de processos quando o texto de pesquisa mudar
        ////        FilterProcessos();
        ////    }
        ////}



        public int GetTotalPages()
        {
            int pageSize = 12;  // O tamanho da página que você escolheu
            return (int)Math.Ceiling((double)Processos_DB.Count / pageSize);
        }


        public void LoadProcessosCSVFromDB()
        {
            // Inicialize a classe BLL e recupere os dados
            var operacoesBLL = new OperacoesBLL();

            var processosFromDB = operacoesBLL.GetAllProcessosCSV();

            // Converta a lista em uma ObservableCollection e atribua à propriedade Processos_DB
            Processos_DB = new ObservableCollection<ProcessoCSV>(processosFromDB);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public ProcessoCSV GetProcessoByNumero(string numeroProcesso)
        {
            // Verifica se Processos_DB é null
            if (Processos_DB == null)
            {
                // Trata o caso em que Processos_DB é null
                // Por exemplo, você pode retornar null para indicar que não foi encontrado nenhum processo
                return null;
            }

            foreach (var proc in Processos_DB)
            {
                if (proc.Processo == numeroProcesso)
                {
                    Debug.WriteLine("Registro encontrado!");
                    break;
                }
            }



            // Use Linq para encontrar o processo específico
            var processo = Processos_DB.FirstOrDefault(p => p.Processo.Trim() == numeroProcesso);


            // Retorne o processo encontrado ou null se nenhum processo com esse número for encontrado
            return processo;
        }
    }
}
