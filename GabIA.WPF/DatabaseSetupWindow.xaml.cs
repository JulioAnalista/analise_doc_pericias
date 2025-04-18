using GabIA.DAL;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using IOPath = System.IO.Path;

namespace GabIA.WPF
{
    /// <summary>
    /// Interaction logic for DatabaseSetupWindow.xaml
    /// </summary>
    public partial class DatabaseSetupWindow : Window
    {
        private IConfiguration _configuration;
        private DatabaseManager _databaseManager;

        public DatabaseSetupWindow()
        {
            InitializeComponent();
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builder.Build();

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                ParseConnectionString(connectionString);
            }
        }

        private void ParseConnectionString(string connectionString)
        {
            try
            {
                var parts = connectionString.Split(';');
                foreach (var part in parts)
                {
                    var keyValue = part.Split('=');
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim().ToLower();
                        var value = keyValue[1].Trim();

                        switch (key)
                        {
                            case "server":
                                txtServer.Text = value;
                                break;
                            case "port":
                                txtPort.Text = value;
                                break;
                            case "database":
                                txtDatabase.Text = value;
                                break;
                            case "uid":
                                txtUsername.Text = value;
                                break;
                            case "pwd":
                                txtPassword.Password = value;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao analisar a string de conexão: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string BuildConnectionString()
        {
            return $"Server={txtServer.Text};Port={txtPort.Text};Database={txtDatabase.Text};Uid={txtUsername.Text};Pwd={txtPassword.Password};";
        }

        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var connectionString = BuildConnectionString();
                _databaseManager = new DatabaseManager(connectionString);

                bool canConnect = await _databaseManager.TestConnection();
                if (canConnect)
                {
                    txtStatus.Text = "Conexão bem-sucedida!";
                    MessageBox.Show("Conexão com o banco de dados estabelecida com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    txtStatus.Text = "Falha na conexão. Verifique as configurações.";
                    MessageBox.Show("Não foi possível conectar ao banco de dados. Verifique as configurações e tente novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Erro: {ex.Message}";
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CreateDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var connectionString = BuildConnectionString();
                _databaseManager = new DatabaseManager(connectionString);

                bool success = await _databaseManager.EnsureDatabaseCreated();
                if (success)
                {
                    txtStatus.Text = "Banco de dados criado com sucesso!";
                    MessageBox.Show("Banco de dados criado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    txtStatus.Text = "Falha ao criar o banco de dados.";
                    MessageBox.Show("Não foi possível criar o banco de dados. Verifique as configurações e tente novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Erro: {ex.Message}";
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var connectionString = BuildConnectionString();
                _databaseManager = new DatabaseManager(connectionString);

                _databaseManager.InitializeDatabase();
                txtStatus.Text = "Banco de dados inicializado com sucesso!";
                MessageBox.Show("Banco de dados inicializado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Erro: {ex.Message}";
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MigrateDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var connectionString = BuildConnectionString();
                _databaseManager = new DatabaseManager(connectionString);

                bool success = await _databaseManager.MigrateDatabase();
                if (success)
                {
                    txtStatus.Text = "Migração do banco de dados concluída com sucesso!";
                    MessageBox.Show("Migração do banco de dados concluída com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    txtStatus.Text = "Falha na migração do banco de dados.";
                    MessageBox.Show("Não foi possível migrar o banco de dados. Verifique as configurações e tente novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Erro: {ex.Message}";
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var connectionString = BuildConnectionString();

                // Atualiza o arquivo appsettings.json
                var appSettingsPath = IOPath.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var json = @"{
  ""ConnectionStrings"": {
    ""DefaultConnection"": """ + connectionString + @"""
  }
}";
                File.WriteAllText(appSettingsPath, json);

                // Atualiza o arquivo App.config
                var appConfigPath = IOPath.Combine(Directory.GetCurrentDirectory(), "GabIA.WPF.dll.config");
                var config = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <connectionStrings>
    <add name=""DB_ConnectionString"" connectionString=""" + connectionString + @""" providerName=""MySql.Data.MySqlClient"" />
  </connectionStrings>
</configuration>";
                File.WriteAllText(appConfigPath, config);

                txtStatus.Text = "Configurações salvas com sucesso!";
                MessageBox.Show("Configurações salvas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Erro: {ex.Message}";
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
