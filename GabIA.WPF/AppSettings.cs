using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GabIA.WPF
{
    public class AppSettings
    {
        // Propriedades de configuração
        public string DefaultDirectory { get; set; } = @"D:\PJe\Processos";
        public string LanguageModel { get; set; } = "gpt-4";
        public string EmbeddingsModel { get; set; } = "text-embedding-ada-002";
        public string TesseractPath { get; set; } = @"C:\Arquivos de Programas\Tesseract-OCR\tessdata";
        public int OcrLowDpi { get; set; } = 200;
        public int OcrHighDpi { get; set; } = 600;
        public string PythonPath { get; set; } = @"c:\Python311\python.exe";
        public string ApiKey { get; set; } = "";
        
        // Caminho do arquivo de configuração
        private static readonly string ConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GabIA",
            "settings.json");
            
        // Instância singleton
        private static AppSettings _instance;
        
        // Obter a instância singleton
        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }
        
        // Construtor privado para o padrão singleton
        private AppSettings() { }
        
        // Carregar configurações do arquivo
        private static AppSettings Load()
        {
            try
            {
                // Verificar se o diretório existe, se não, criar
                string configDir = Path.GetDirectoryName(ConfigFilePath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                // Verificar se o arquivo existe
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro, registrar e retornar configurações padrão
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar configurações: {ex.Message}");
            }
            
            // Retornar configurações padrão se não conseguir carregar
            return new AppSettings();
        }
        
        // Salvar configurações no arquivo
        public void Save()
        {
            try
            {
                string configDir = Path.GetDirectoryName(ConfigFilePath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar configurações: {ex.Message}");
                throw;
            }
        }
    }
}
