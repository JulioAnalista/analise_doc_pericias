using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Media;

namespace GabIA.WPF
{
    public partial class PDFProcessingWindow : Window
    {
        public ObservableCollection<string> SelectedFiles { get; private set; }

        public PDFProcessingWindow()
        {
            InitializeComponent();
            SelectedFiles = new ObservableCollection<string>();
            lstFiles.ItemsSource = SelectedFiles;

            // Desabilitar o botão Processar inicialmente
            btnProcess.IsEnabled = false;

            // Adicionar manipulador de eventos para a coleção
            SelectedFiles.CollectionChanged += (sender, e) => {
                // Atualizar o estado do botão com base na contagem de arquivos
                btnProcess.IsEnabled = SelectedFiles.Count > 0;
            };
        }

        private void BtnSelectFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Arquivos PDF (*.pdf)|*.pdf",
                Title = "Selecione os arquivos PDF para processamento"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    if (!SelectedFiles.Contains(fileName))
                    {
                        SelectedFiles.Add(fileName);
                    }
                }
            }
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower() == ".pdf" && !SelectedFiles.Contains(file))
                    {
                        SelectedFiles.Add(file);
                    }
                }
            }
        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                bool allPdf = files.All(file => Path.GetExtension(file).ToLower() == ".pdf");
                e.Effects = allPdf ? DragDropEffects.Copy : DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string filePath = button.Tag as string;
            SelectedFiles.Remove(filePath);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnProcess_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFiles.Count > 0)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Por favor, selecione pelo menos um arquivo PDF para processar.",
                    "Nenhum arquivo selecionado", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }


}
