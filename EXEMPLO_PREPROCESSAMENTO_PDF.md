# Exemplo de Uso do Sistema de Preprocessamento de PDF

Este documento apresenta exemplos práticos de como utilizar o sistema de preprocessamento de documentos PDF do projeto GabIA.

## Exemplo 1: Preprocessamento Completo de um Processo

Este exemplo demonstra como realizar o preprocessamento completo de um processo judicial, desde a conversão dos PDFs até a geração dos arquivos de texto e JSON.

```csharp
// Importações necessárias
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using GabIA.WPF;

// Método para iniciar o preprocessamento
private async Task PreprocessarProcesso(string numeroProcesso)
{
    try
    {
        // Definir diretórios
        string diretorioBase = Path.Combine(@"d:\PJe\Processos", numeroProcesso);
        string diretorioPDF = Path.Combine(diretorioBase, "PaginasPDF");
        string diretorioPNG = Path.Combine(diretorioBase, "PNG");
        string diretorioOCR = Path.Combine(diretorioBase, "OCR");
        string diretorioPecasTxt = Path.Combine(diretorioBase, "PecasProcessuais", "PecasTxt");
        string diretorioPecasPDF = Path.Combine(diretorioBase, "PecasProcessuais", "PecasPDF");
        
        // Criar diretórios se não existirem
        Directory.CreateDirectory(diretorioPDF);
        Directory.CreateDirectory(diretorioPNG);
        Directory.CreateDirectory(diretorioOCR);
        Directory.CreateDirectory(diretorioPecasTxt);
        Directory.CreateDirectory(diretorioPecasPDF);
        
        // Mover PDFs para o diretório de processamento
        await MoverPDFsParaDiretorioProcessamento(numeroProcesso);
        
        // Iniciar o preprocessamento
        ProcessamentoDeTexto processador = new ProcessamentoDeTexto();
        processador.ProcessaArquivos(diretorioPDF, diretorioBase);
        
        // Processar textos para formato JSON
        string diretorioModeloLinguagem = Path.Combine(diretorioBase, "ModeloDeLinguagem", "JsonlOriginal");
        Directory.CreateDirectory(diretorioModeloLinguagem);
        
        PreprocessamentoTextoIA preprocessadorIA = new PreprocessamentoTextoIA();
        preprocessadorIA.ProcessFilesInDirectoryToJson(diretorioPecasTxt, diretorioModeloLinguagem, null);
        
        MessageBox.Show("Preprocessamento concluído com sucesso!", "GabIA", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Erro durante o preprocessamento: {ex.Message}", "GabIA", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

// Método para mover PDFs para o diretório de processamento
private async Task MoverPDFsParaDiretorioProcessamento(string numeroProcesso)
{
    string diretorioOrigem = @"d:\Downloads";
    string diretorioDestino = Path.Combine(@"d:\PJe\Processos", numeroProcesso, "PaginasPDF");
    
    // Criar diretório de destino se não existir
    Directory.CreateDirectory(diretorioDestino);
    
    // Obter todos os arquivos PDF do diretório de origem
    string[] arquivosPDF = Directory.GetFiles(diretorioOrigem, "*.pdf");
    
    // Mover cada arquivo para o diretório de destino
    foreach (string arquivo in arquivosPDF)
    {
        string nomeArquivo = Path.GetFileName(arquivo);
        string caminhoDestino = Path.Combine(diretorioDestino, nomeArquivo);
        
        // Verificar se o arquivo já existe no destino
        if (File.Exists(caminhoDestino))
        {
            File.Delete(caminhoDestino);
        }
        
        // Mover o arquivo
        File.Move(arquivo, caminhoDestino);
    }
}
```

## Exemplo 2: Conversão de PDF para PNG e OCR

Este exemplo demonstra como converter um arquivo PDF específico para PNG e realizar OCR.

```csharp
// Importações necessárias
using System;
using System.IO;
using GabIA.WPF;

// Método para converter PDF para PNG e realizar OCR
private string ConverterPDFParaPNGERealizarOCR(string caminhoArquivoPDF)
{
    try
    {
        // Definir diretórios
        string diretorioBase = Path.GetDirectoryName(caminhoArquivoPDF);
        string diretorioPNG = Path.Combine(diretorioBase, "PNG");
        string diretorioOCR = Path.Combine(diretorioBase, "OCR");
        
        // Criar diretórios se não existirem
        Directory.CreateDirectory(diretorioPNG);
        Directory.CreateDirectory(diretorioOCR);
        
        // Converter PDF para PNG
        PDFToPNGConverter converter = new PDFToPNGConverter();
        converter.ConvertPdfPageToPng(caminhoArquivoPDF, diretorioPNG, 200, 600, 1);
        
        // Caminho da imagem PNG gerada
        string caminhoPNG = Path.Combine(diretorioPNG, Path.GetFileNameWithoutExtension(caminhoArquivoPDF) + "L.png");
        
        // Realizar OCR
        ProcessamentoDeTexto processador = new ProcessamentoDeTexto();
        string textoExtraido = processador.PerformOcr(caminhoPNG);
        
        // Salvar texto extraído
        string caminhoTexto = Path.Combine(diretorioOCR, Path.GetFileNameWithoutExtension(caminhoArquivoPDF) + ".txt");
        File.WriteAllText(caminhoTexto, textoExtraido);
        
        return textoExtraido;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro durante a conversão e OCR: {ex.Message}");
        return null;
    }
}
```

## Exemplo 3: Preprocessamento de Texto para IA

Este exemplo demonstra como preprocessar um texto para uso em modelos de IA.

```csharp
// Importações necessárias
using System;
using System.IO;
using System.Collections.Generic;
using GabIA.WPF;
using Newtonsoft.Json;

// Método para preprocessar texto para IA
private void PreprocessarTextoParaIA(string caminhoArquivoTexto, string diretorioSaida)
{
    try
    {
        // Ler o texto do arquivo
        string texto = File.ReadAllText(caminhoArquivoTexto);
        
        // Dividir o texto em chunks
        List<string> chunks = PreprocessamentoTextoIA.DividirTexto(texto, 32 * 1024);
        
        // Criar diretório de saída se não existir
        Directory.CreateDirectory(diretorioSaida);
        
        // Processar cada chunk
        for (int i = 0; i < chunks.Count; i++)
        {
            // Sanitizar o texto
            string textoSanitizado = PreprocessamentoTextoIA.SanitizaUPPERCASE(chunks[i]);
            
            // Criar objeto JSON
            var objetoJson = new
            {
                texto = textoSanitizado,
                parte = i + 1,
                totalPartes = chunks.Count,
                nomeArquivo = Path.GetFileNameWithoutExtension(caminhoArquivoTexto)
            };
            
            // Serializar para JSON
            string json = JsonConvert.SerializeObject(objetoJson);
            
            // Salvar arquivo JSON
            string caminhoSaida = Path.Combine(diretorioSaida, $"{Path.GetFileNameWithoutExtension(caminhoArquivoTexto)}_parte{i + 1}.json");
            File.WriteAllText(caminhoSaida, json);
        }
        
        Console.WriteLine($"Preprocessamento concluído. {chunks.Count} arquivos gerados em {diretorioSaida}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro durante o preprocessamento: {ex.Message}");
    }
}
```

## Exemplo 4: Avaliação da Qualidade do OCR

Este exemplo demonstra como avaliar a qualidade do texto extraído pelo OCR.

```csharp
// Importações necessárias
using System;
using GabIA.WPF;

// Método para avaliar a qualidade do OCR
private bool AvaliarQualidadeOCR(string textoOCR)
{
    // Parâmetros de avaliação
    int shortWordThreshold = 3; // Limite para considerar uma palavra curta
    int longWordThreshold = 4; // Limite para considerar uma palavra longa
    float ratioThreshold = 0.1f; // Limite da razão entre palavras longas e curtas
    float isolatedCharRatioThreshold = 0.05f; // Limite para a proporção de caracteres isolados
    
    int shortWordCount = 0;
    int longWordCount = 0;
    int isolatedCharCount = 0;
    int totalCharCount = 0;
    
    // Dividir o texto em palavras
    string[] palavras = textoOCR.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    
    // Analisar cada palavra
    foreach (string palavra in palavras)
    {
        int comprimentoPalavra = palavra.Length;
        totalCharCount += comprimentoPalavra;
        
        if (comprimentoPalavra < shortWordThreshold)
            shortWordCount++;
        else if (comprimentoPalavra > longWordThreshold)
            longWordCount++;
        
        if (comprimentoPalavra == 1 && !char.IsPunctuation(palavra[0]))
            isolatedCharCount++;
    }
    
    // Calcular as razões
    float longToShortRatio = (shortWordCount > 0) ? (float)longWordCount / shortWordCount : longWordCount;
    float isolatedCharRatio = (totalCharCount > 0) ? (float)isolatedCharCount / totalCharCount : 0;
    
    // Verificar se as razões estão abaixo dos limiares estabelecidos
    bool baixaQualidade = longToShortRatio < ratioThreshold || isolatedCharRatio > isolatedCharRatioThreshold;
    
    Console.WriteLine($"Avaliação de qualidade do OCR:");
    Console.WriteLine($"- Total de caracteres: {totalCharCount}");
    Console.WriteLine($"- Palavras curtas: {shortWordCount}");
    Console.WriteLine($"- Palavras longas: {longWordCount}");
    Console.WriteLine($"- Caracteres isolados: {isolatedCharCount}");
    Console.WriteLine($"- Razão palavras longas/curtas: {longToShortRatio:F2} (limiar: {ratioThreshold})");
    Console.WriteLine($"- Razão caracteres isolados: {isolatedCharRatio:F2} (limiar: {isolatedCharRatioThreshold})");
    Console.WriteLine($"- Qualidade: {(baixaQualidade ? "Baixa" : "Boa")}");
    
    return !baixaQualidade;
}
```

## Exemplo 5: Pré-processamento de Imagem para Melhorar OCR

Este exemplo demonstra como pré-processar uma imagem para melhorar os resultados do OCR.

```csharp
// Importações necessárias
using System;
using System.Drawing;
using System.Drawing.Imaging;
using GabIA.WPF;

// Método para pré-processar uma imagem
private void PreProcessarImagem(string caminhoImagemEntrada, string caminhoImagemSaida)
{
    try
    {
        // Carregar a imagem
        using (Bitmap imagemOriginal = new Bitmap(caminhoImagemEntrada))
        {
            // Criar uma cópia da imagem para processamento
            using (Bitmap imagemProcessada = new Bitmap(imagemOriginal))
            {
                // Aplicar filtro gaussiano para suavização
                ApplyGaussianBlur(imagemProcessada, 5, 1.0);
                
                // Ajustar contraste e brilho
                AdjustContrastAndBrightness(imagemProcessada, 1.5f, 1.0f);
                
                // Aplicar binarização adaptativa
                ApplyAdaptiveThresholding(imagemProcessada, 11, 10);
                
                // Remover speckles
                RemoveSpeckles(imagemProcessada, 0.5);
                
                // Salvar a imagem processada
                imagemProcessada.Save(caminhoImagemSaida, ImageFormat.Png);
                
                Console.WriteLine($"Imagem pré-processada salva em: {caminhoImagemSaida}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro durante o pré-processamento da imagem: {ex.Message}");
    }
}

// Método para aplicar binarização adaptativa
private void ApplyAdaptiveThresholding(Bitmap bmp, int blockSize, int offset)
{
    // Implementação da binarização adaptativa
    // (código similar ao da classe PDFToPNGConverter)
}

// Método para aplicar filtro gaussiano
private void ApplyGaussianBlur(Bitmap bmp, int filterSize, double sigma)
{
    // Implementação do filtro gaussiano
    // (código similar ao da classe PDFToPNGConverter)
}

// Método para ajustar contraste e brilho
private void AdjustContrastAndBrightness(Bitmap bmp, float contrast, float brightness)
{
    // Implementação do ajuste de contraste e brilho
    // (código similar ao da classe PDFToPNGConverter)
}

// Método para remover speckles
private void RemoveSpeckles(Bitmap bmp, double minSpeckleSizeMm)
{
    // Implementação da remoção de speckles
    // (código similar ao da classe PDFToPNGConverter)
}
```

## Exemplo 6: Processamento em Lote de Múltiplos PDFs

Este exemplo demonstra como processar múltiplos arquivos PDF em lote.

```csharp
// Importações necessárias
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using GabIA.WPF;

// Método para processar múltiplos PDFs em lote
private async Task ProcessarMultiplosPDFs(string diretorioEntrada, string diretorioSaida)
{
    try
    {
        // Obter todos os arquivos PDF do diretório de entrada
        string[] arquivosPDF = Directory.GetFiles(diretorioEntrada, "*.pdf");
        
        Console.WriteLine($"Encontrados {arquivosPDF.Length} arquivos PDF para processamento.");
        
        // Criar diretório de saída se não existir
        Directory.CreateDirectory(diretorioSaida);
        
        // Criar diretórios para processamento
        string diretorioPNG = Path.Combine(diretorioSaida, "PNG");
        string diretorioOCR = Path.Combine(diretorioSaida, "OCR");
        string diretorioPecasTxt = Path.Combine(diretorioSaida, "PecasTxt");
        
        Directory.CreateDirectory(diretorioPNG);
        Directory.CreateDirectory(diretorioOCR);
        Directory.CreateDirectory(diretorioPecasTxt);
        
        // Inicializar conversor e processador
        PDFToPNGConverter converter = new PDFToPNGConverter();
        ProcessamentoDeTexto processador = new ProcessamentoDeTexto();
        
        // Lista para armazenar tarefas
        List<Task> tarefas = new List<Task>();
        
        // Processar cada arquivo PDF
        foreach (string arquivoPDF in arquivosPDF)
        {
            string nomeArquivo = Path.GetFileNameWithoutExtension(arquivoPDF);
            
            // Criar tarefa para processar o arquivo
            Task tarefa = Task.Run(() =>
            {
                try
                {
                    Console.WriteLine($"Processando: {nomeArquivo}");
                    
                    // Converter PDF para PNG
                    converter.ConvertPdfPageToPng(arquivoPDF, diretorioPNG, 200, 600, 1);
                    
                    // Caminho da imagem PNG gerada
                    string caminhoPNG = Path.Combine(diretorioPNG, nomeArquivo + "L.png");
                    
                    // Realizar OCR
                    string textoExtraido = processador.PerformOcr(caminhoPNG);
                    
                    // Salvar texto extraído
                    string caminhoTexto = Path.Combine(diretorioPecasTxt, nomeArquivo + ".txt");
                    File.WriteAllText(caminhoTexto, textoExtraido);
                    
                    Console.WriteLine($"Concluído: {nomeArquivo}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar {nomeArquivo}: {ex.Message}");
                }
            });
            
            tarefas.Add(tarefa);
        }
        
        // Aguardar a conclusão de todas as tarefas
        await Task.WhenAll(tarefas);
        
        Console.WriteLine("Processamento em lote concluído.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro durante o processamento em lote: {ex.Message}");
    }
}
```

Estes exemplos demonstram diferentes aspectos do sistema de preprocessamento de PDF do projeto GabIA. Eles podem ser adaptados e combinados conforme necessário para atender a requisitos específicos.

Para mais informações, consulte o [README do Preprocessamento de PDF](README_PREPROCESSAMENTO_PDF.md).
