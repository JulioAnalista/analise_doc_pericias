using GabIA.BLL;
using GabIA.ENT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GabIA.WPF.Commands;
using GabIA.DAL;
using System.Windows;
using System.Runtime.CompilerServices;
using Themes.ViewModels.DataGrids;
using PdfSharp.Pdf.Advanced;
using GabIA.WPF.Utilities;

namespace GabIA.WPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _site1;
        private string _site2;
        private string _site3;
        private string _site4;

        public string Site1
        {
            get { return _site1; }
            set { _site1 = value; OnPropertyChanged(); }
        }

        public string Site2
        {
            get { return _site2; }
            set { _site2 = value; OnPropertyChanged(); }
        }

        public string Site3
        {
            get { return _site3; }
            set { _site3 = value; OnPropertyChanged(); }
        }
        public string Site4
        {
            get { return _site4; }
            set { _site4 = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand AddContentCommand { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Adicione a propriedade CurrentViewModel
        public NeoGabModel CurrentViewModel { get; set; }

        private ProcessoBLL _processoBLL;
        private ObservableCollection<ProcessoENT> _processos;
        private int _currentIndex;
        private ProcessoENT _currentProcesso;

        // Propriedades do DataGridViewModel e EditorGridViewModel do exemplo
        public DataGridViewModel DataGridViewModel { get; private set; }
        public EditorGridViewModel EditorGridViewModel { get; private set; }

        // Propriedades de coleção de exemplo
        public ObservableCollection<string> SomeItems { get; private set; }


        public MainViewModel()
        {
            // Criando instâncias de cada DAL
            ProcessoDAL processoDAL = new ProcessoDAL();
            _processoBLL = new ProcessoBLL(processoDAL);

            // Cria uma instância da camada de Lógica de Negócio (BLL) e passa a instância da camada DAL
            // Busca todos os registros de processos
            _processos = new ObservableCollection<ProcessoENT>(_processoBLL.GetAllProcessos());

            // Inicializa o índice atual e o processo selecionado
            _currentIndex = 0;
            // Inicializa os comandos de navegação
            NavigateNextCommand = new RelayCommand(NavigateNext, CanNavigateNext);
            NavigatePreviousCommand = new RelayCommand(NavigatePrevious, CanNavigatePrevious);
            // Inicializações do MainViewModel de exemplo
            DataGridViewModel = new DataGridViewModel();
            EditorGridViewModel = new EditorGridViewModel();
            SomeItems = new ObservableCollection<string> { "1024x576", "1280x720", "1920x1080", "3840x2160" };
        }


        // Propriedade que expõe o processo atualmente selecionado para a camada de apresentação
        public ProcessoENT CurrentProcesso
        {
            get { return _currentProcesso; }
            set
            {
                // Atualiza o processo selecionado se o valor for diferente
                if (_currentProcesso != value)
                {
                    _currentProcesso = value;
                    OnPropertyChanged(nameof(CurrentProcesso));
                }
            }
        }

        // Comandos de navegação
        public ICommand NavigateNextCommand { get; }
        public ICommand NavigatePreviousCommand { get; }

        // Método para navegar até o próximo registro
        private void NavigateNext(object obj)
        {
            _currentIndex++;
            CurrentProcesso = _processos[_currentIndex];
        }

        // Método para navegar até o registro anterior
        private void NavigatePrevious(object obj)
        {
            _currentIndex--;
            CurrentProcesso = _processos[_currentIndex];
        }

        // Verifica se é possível navegar até o próximo registro
        private bool CanNavigateNext(object obj)
        {
            return _currentIndex < _processos.Count - 1;
        }

        // Verifica se é possível navegar até o registro anterior
        private bool CanNavigatePrevious(object obj)
        {
            return _currentIndex > 0;
        }
    }
}
