using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Forms;

namespace GabIA.WPF
{
    public partial class ConfigurationDialog : Window
    {
        private AppSettings _settings;

        public ConfigurationDialog()
        {
            InitializeComponent();
            _settings = AppSettings.Instance;
            LoadSettings();

            // Aplicar o tema atual aos ComboBoxItems
            ApplyThemeToComboBoxItems();
        }

        private void ApplyThemeToComboBoxItems()
        {
            // Aplicar o tema aos itens do ComboBox de modelo de linguagem
            foreach (ComboBoxItem item in cmbLanguageModel.Items)
            {
                item.Background = (System.Windows.Media.Brush)FindResource("ABrush.Tone5.Background.Static");
                item.Foreground = (System.Windows.Media.Brush)FindResource("ABrush.Foreground.Static");
            }

            // Aplicar o tema aos itens do ComboBox de modelo de embeddings
            foreach (ComboBoxItem item in cmbEmbeddingsModel.Items)
            {
                item.Background = (System.Windows.Media.Brush)FindResource("ABrush.Tone5.Background.Static");
                item.Foreground = (System.Windows.Media.Brush)FindResource("ABrush.Foreground.Static");
            }
        }

        private void LoadSettings()
        {
            // Carregar configurações nos controles
            txtDefaultDirectory.Text = _settings.DefaultDirectory;

            // Selecionar o modelo de linguagem na ComboBox
            foreach (ComboBoxItem item in cmbLanguageModel.Items)
            {
                if (item.Content.ToString() == _settings.LanguageModel)
                {
                    cmbLanguageModel.SelectedItem = item;
                    break;
                }
            }

            // Selecionar o modelo de embeddings na ComboBox
            foreach (ComboBoxItem item in cmbEmbeddingsModel.Items)
            {
                if (item.Content.ToString() == _settings.EmbeddingsModel)
                {
                    cmbEmbeddingsModel.SelectedItem = item;
                    break;
                }
            }

            txtApiKey.Password = _settings.ApiKey;
            txtTesseractPath.Text = _settings.TesseractPath;
            txtLowDpi.Text = _settings.OcrLowDpi.ToString();
            txtHighDpi.Text = _settings.OcrHighDpi.ToString();
            txtPythonPath.Text = _settings.PythonPath;
        }

        private void SaveSettings()
        {
            // Salvar configurações do formulário para o objeto AppSettings
            _settings.DefaultDirectory = txtDefaultDirectory.Text;

            if (cmbLanguageModel.SelectedItem != null)
            {
                _settings.LanguageModel = ((ComboBoxItem)cmbLanguageModel.SelectedItem).Content.ToString();
            }

            if (cmbEmbeddingsModel.SelectedItem != null)
            {
                _settings.EmbeddingsModel = ((ComboBoxItem)cmbEmbeddingsModel.SelectedItem).Content.ToString();
            }

            _settings.ApiKey = txtApiKey.Password;
            _settings.TesseractPath = txtTesseractPath.Text;

            if (int.TryParse(txtLowDpi.Text, out int lowDpi))
            {
                _settings.OcrLowDpi = lowDpi;
            }

            if (int.TryParse(txtHighDpi.Text, out int highDpi))
            {
                _settings.OcrHighDpi = highDpi;
            }

            _settings.PythonPath = txtPythonPath.Text;

            // Salvar configurações no arquivo
            _settings.Save();
        }

        private void BrowseDefaultDirectory_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Selecione o diretório padrão";
                dialog.SelectedPath = txtDefaultDirectory.Text;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtDefaultDirectory.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowseTesseractPath_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Selecione o diretório do Tesseract";
                dialog.SelectedPath = txtTesseractPath.Text;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtTesseractPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowsePythonPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Selecione o executável do Python",
                Filter = "Executáveis (*.exe)|*.exe",
                InitialDirectory = System.IO.Path.GetDirectoryName(txtPythonPath.Text)
            };

            if (dialog.ShowDialog() == true)
            {
                txtPythonPath.Text = dialog.FileName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSettings();
                System.Windows.MessageBox.Show("Configurações salvas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao salvar configurações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
