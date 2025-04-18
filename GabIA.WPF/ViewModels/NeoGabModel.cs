using GabIA.WPF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GabIA.WPF.Commands;

namespace GabIA.WPF.ViewModels
{
    public class NeoGabModel
    {
        public ICommand ExtractPanelsCommand { get; }

        public event EventHandler ExtractPanelsRequested;

        public NeoGabView ViewInstance { get; set; }

        public NeoGabModel()
        {
            ExtractPanelsCommand = new RelayCommand(ExtractPanels);
        }

        private void ExtractPanels(object parameter)
        {
            // Implementação do método ExtractPanels
        }

        // Outras propriedades, campos, métodos e eventos do ViewModel vão aqui
    }
}
