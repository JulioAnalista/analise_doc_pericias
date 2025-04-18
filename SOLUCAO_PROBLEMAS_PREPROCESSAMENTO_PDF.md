# Solução de Problemas no Preprocessamento de PDF

Este documento apresenta soluções para problemas comuns encontrados durante o uso do sistema de preprocessamento de documentos PDF do projeto GabIA.

## Índice

- [Problemas de OCR](#problemas-de-ocr)
- [Problemas de Conversão PDF para PNG](#problemas-de-conversão-pdf-para-png)
- [Problemas de Processamento de Texto](#problemas-de-processamento-de-texto)
- [Problemas de Desempenho](#problemas-de-desempenho)
- [Problemas de Integração com IA](#problemas-de-integração-com-ia)
- [Problemas de Armazenamento](#problemas-de-armazenamento)
- [Logs e Depuração](#logs-e-depuração)

## Problemas de OCR

### Texto Extraído com Baixa Qualidade

**Sintomas:**
- Texto extraído contém muitos erros
- Caracteres são reconhecidos incorretamente
- Palavras estão truncadas ou mescladas

**Possíveis Causas e Soluções:**

1. **Tesseract não está instalado corretamente**
   - Verifique se o Tesseract está instalado no caminho correto (`C:\Arquivos de Programas\Tesseract-OCR\`)
   - Reinstale o Tesseract se necessário

2. **Pacote de idioma português não está instalado**
   - Verifique se o pacote de idioma português (`por.traineddata`) está presente no diretório `tessdata`
   - Baixe e instale o pacote de idioma se necessário

3. **Resolução da imagem muito baixa**
   - Aumente o valor do parâmetro `lowDpi` (por exemplo, de 200 para 300)
   - Aumente o valor do parâmetro `highDpi` (por exemplo, de 600 para 900)

4. **Imagem com ruído ou baixo contraste**
   - Ajuste os parâmetros de pré-processamento de imagem:
     ```csharp
     // Aumentar o contraste
     float contrastValue = 2.0f;
     // Ajustar o brilho
     float brightnessValue = 1.2f;
     PreProcessImageForOCR(bitmapHighRes, contrastValue, brightnessValue);
     ```

5. **Documento com fontes não padrão ou estilizadas**
   - Tente usar um modelo de OCR mais avançado
   - Considere usar o script Python auxiliar para OCR:
     ```csharp
     string pythonScriptPath = "d:\\PJe\\App_cpp\\ocr.py";
     string pythonExecutablePath = "c:\\Python311\\python.exe";
     
     var args = new List<string> {
         pythonExecutablePath,
         pythonScriptPath,
         pngPath,
         outputTextPath
     };
     
     Process.Start(new ProcessStartInfo {
         FileName = pythonExecutablePath,
         Arguments = string.Join(" ", args),
         UseShellExecute = false,
         CreateNoWindow = true
     });
     ```

### OCR Muito Lento

**Sintomas:**
- O processo de OCR leva muito tempo para concluir
- O sistema parece travar durante o OCR

**Possíveis Causas e Soluções:**

1. **Imagens muito grandes**
   - Reduza a resolução das imagens
   - Divida documentos grandes em partes menores

2. **Recursos de sistema insuficientes**
   - Feche outros aplicativos que consomem muitos recursos
   - Aumente a memória RAM disponível para o aplicativo

3. **Configuração do Tesseract**
   - Ajuste o modo do Tesseract para priorizar velocidade:
     ```csharp
     using (var engine = new TesseractEngine(tessDataPath, "por", EngineMode.TesseractOnly))
     ```

4. **Muitas imagens sendo processadas simultaneamente**
   - Limite o número de processamentos paralelos
   - Implemente um sistema de fila para processar as imagens sequencialmente

## Problemas de Conversão PDF para PNG

### Erro ao Converter PDF para PNG

**Sintomas:**
- Mensagens de erro durante a conversão
- Imagens PNG não são geradas

**Possíveis Causas e Soluções:**

1. **Ghostscript não está instalado corretamente**
   - Verifique se o Ghostscript está instalado
   - Reinstale o Ghostscript se necessário
   - Verifique se o caminho do Ghostscript está no PATH do sistema

2. **PDF corrompido ou protegido**
   - Verifique se o PDF pode ser aberto normalmente
   - Tente remover a proteção do PDF usando ferramentas apropriadas
   - Tente reparar o PDF usando ferramentas de reparo

3. **Permissões de acesso insuficientes**
   - Verifique se o aplicativo tem permissões para ler o PDF e escrever no diretório de saída
   - Execute o aplicativo como administrador se necessário

4. **Versão incompatível do Ghostscript**
   - Atualize para a versão mais recente do Ghostscript
   - Verifique a compatibilidade entre a versão do Ghostscript e a biblioteca Ghostscript.NET

### Imagens PNG de Baixa Qualidade

**Sintomas:**
- Imagens PNG geradas têm baixa qualidade
- Texto nas imagens está borrado ou pixelizado

**Possíveis Causas e Soluções:**

1. **Resolução (DPI) muito baixa**
   - Aumente o valor do parâmetro `lowDpi` (por exemplo, de 200 para 300)
   - Aumente o valor do parâmetro `highDpi` (por exemplo, de 600 para 900)

2. **PDF original de baixa qualidade**
   - Verifique a qualidade do PDF original
   - Se o PDF for uma digitalização, tente obter uma versão de melhor qualidade

3. **Configurações de conversão inadequadas**
   - Ajuste as configurações de conversão do Ghostscript:
     ```csharp
     // Exemplo de configurações personalizadas para o Ghostscript
     List<string> switches = new List<string>();
     switches.Add("-dNOPAUSE");
     switches.Add("-dBATCH");
     switches.Add("-dSAFER");
     switches.Add("-sDEVICE=pngalpha");
     switches.Add("-r" + highDpi.ToString());
     switches.Add("-dTextAlphaBits=4");
     switches.Add("-dGraphicsAlphaBits=4");
     
     // Usar estas configurações ao inicializar o rasterizer
     ```

## Problemas de Processamento de Texto

### Falha na Identificação de Informações

**Sintomas:**
- Informações como número do processo, ID e página não são identificadas corretamente
- Arquivos de texto não são agrupados corretamente

**Possíveis Causas e Soluções:**

1. **Expressões regulares não correspondem ao formato do texto**
   - Verifique se as expressões regulares estão corretas
   - Ajuste as expressões regulares para corresponder ao formato do texto:
     ```csharp
     // Exemplo de ajuste de expressão regular
     var regex = new Regex(@"Número processo PJe:\s*(?<pjeNumber>\d+-\d+\.\d+\.\d+\.\d+\.\d+)\s*ID\.\s*(?<id>\d+)\s*Pág\.\s*(?<page>\d+)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
     ```

2. **Texto extraído pelo OCR contém erros**
   - Melhore a qualidade do OCR (veja as soluções para problemas de OCR)
   - Implemente correção de texto pós-OCR

3. **Formato do documento não é padrão**
   - Adapte o código para lidar com diferentes formatos de documento
   - Implemente detecção automática de formato

### Erro ao Combinar Páginas

**Sintomas:**
- Páginas não são combinadas corretamente em documentos completos
- Documentos combinados contêm páginas incorretas ou faltando páginas

**Possíveis Causas e Soluções:**

1. **Identificação incorreta de páginas relacionadas**
   - Verifique se as informações de ID e página estão sendo extraídas corretamente
   - Ajuste o algoritmo de agrupamento de páginas

2. **Arquivos PDF não existem ou estão inacessíveis**
   - Verifique se os arquivos PDF existem no diretório esperado
   - Verifique as permissões de acesso aos arquivos

3. **Erro na biblioteca iText7**
   - Atualize para a versão mais recente da biblioteca iText7
   - Verifique se há exceções específicas da biblioteca e trate-as adequadamente

## Problemas de Desempenho

### Processamento Muito Lento

**Sintomas:**
- O processo completo leva muito tempo para concluir
- O sistema fica lento durante o processamento

**Possíveis Causas e Soluções:**

1. **Processamento sequencial**
   - Implemente processamento paralelo para melhorar o desempenho:
     ```csharp
     // Exemplo de processamento paralelo
     Parallel.ForEach(arquivos, new ParallelOptions { MaxDegreeOfParallelism = 4 }, arquivo =>
     {
         // Processar arquivo
     });
     ```

2. **Arquivos muito grandes**
   - Divida arquivos grandes em partes menores
   - Processe arquivos grandes separadamente

3. **Recursos de sistema insuficientes**
   - Feche outros aplicativos que consomem muitos recursos
   - Aumente a memória RAM disponível para o aplicativo
   - Considere usar um hardware mais potente

4. **Operações de I/O ineficientes**
   - Minimize as operações de leitura e escrita em disco
   - Use buffers para operações de I/O
   - Considere usar armazenamento mais rápido (SSD em vez de HDD)

### Consumo Excessivo de Memória

**Sintomas:**
- O aplicativo consome muita memória RAM
- O sistema fica lento ou trava durante o processamento

**Possíveis Causas e Soluções:**

1. **Muitos arquivos sendo processados simultaneamente**
   - Limite o número de processamentos paralelos
   - Implemente um sistema de fila para processar os arquivos sequencialmente

2. **Imagens muito grandes sendo carregadas na memória**
   - Reduza a resolução das imagens
   - Processe as imagens em partes
   - Libere recursos imediatamente após o uso:
     ```csharp
     using (var bitmap = new Bitmap(imagePath))
     {
         // Processar a imagem
     } // O bitmap é liberado automaticamente aqui
     ```

3. **Vazamentos de memória**
   - Verifique se todos os recursos estão sendo liberados corretamente
   - Use ferramentas de análise de memória para identificar vazamentos
   - Implemente coleta de lixo explícita em pontos críticos:
     ```csharp
     GC.Collect();
     GC.WaitForPendingFinalizers();
     ```

## Problemas de Integração com IA

### Erro ao Gerar Arquivos JSON

**Sintomas:**
- Arquivos JSON não são gerados corretamente
- Erros durante a serialização para JSON

**Possíveis Causas e Soluções:**

1. **Formato de texto incompatível com JSON**
   - Sanitize o texto antes da serialização:
     ```csharp
     // Exemplo de sanitização de texto para JSON
     string textoSanitizado = PreprocessamentoTextoIA.PreprocessJsonString(texto);
     ```

2. **Caracteres especiais ou de controle no texto**
   - Remova ou substitua caracteres problemáticos:
     ```csharp
     // Exemplo de remoção de caracteres de controle
     string textoLimpo = Regex.Replace(texto, @"[\x00-\x1F]", "");
     ```

3. **Tamanho do texto excede limites**
   - Divida o texto em partes menores:
     ```csharp
     // Exemplo de divisão de texto
     List<string> chunks = PreprocessamentoTextoIA.DividirTexto(texto, 32 * 1024);
     ```

### Problemas com Tamanho de Chunks

**Sintomas:**
- Chunks de texto são muito grandes ou muito pequenos
- Modelos de IA não processam os chunks corretamente

**Possíveis Causas e Soluções:**

1. **Tamanho máximo de chunk inadequado**
   - Ajuste o valor da constante `MaxChunkSize`:
     ```csharp
     // Exemplo de ajuste do tamanho máximo de chunk
     private const int MaxChunkSize = 16 * 1024; // 16KB
     ```

2. **Divisão de texto não respeita limites semânticos**
   - Melhore o algoritmo de divisão para respeitar limites semânticos:
     ```csharp
     // Exemplo de divisão de texto melhorada
     public static List<string> DividirTextoMelhorado(string texto, int tamanhoMaximo)
     {
         List<string> partes = new List<string>();
         
         // Dividir o texto em parágrafos
         string[] paragrafos = texto.Split(new string[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None);
         
         StringBuilder parteAtual = new StringBuilder();
         
         foreach (string paragrafo in paragrafos)
         {
             // Se adicionar o parágrafo atual exceder o tamanho máximo
             if (parteAtual.Length + paragrafo.Length > tamanhoMaximo && parteAtual.Length > 0)
             {
                 // Adicionar a parte atual à lista
                 partes.Add(parteAtual.ToString());
                 parteAtual.Clear();
             }
             
             // Adicionar o parágrafo à parte atual
             parteAtual.AppendLine(paragrafo);
             parteAtual.AppendLine();
         }
         
         // Adicionar a última parte se não estiver vazia
         if (parteAtual.Length > 0)
         {
             partes.Add(parteAtual.ToString());
         }
         
         return partes;
     }
     ```

## Problemas de Armazenamento

### Espaço em Disco Insuficiente

**Sintomas:**
- Erros de "espaço em disco insuficiente"
- Arquivos não são salvos corretamente

**Possíveis Causas e Soluções:**

1. **Muitos arquivos temporários**
   - Limpe arquivos temporários regularmente
   - Implemente limpeza automática de arquivos temporários:
     ```csharp
     // Exemplo de limpeza de arquivos temporários
     private void LimparArquivosTemporarios(string diretorio, int diasAntigos)
     {
         try
         {
             DirectoryInfo di = new DirectoryInfo(diretorio);
             
             foreach (FileInfo arquivo in di.GetFiles())
             {
                 if (arquivo.LastWriteTime < DateTime.Now.AddDays(-diasAntigos))
                 {
                     arquivo.Delete();
                 }
             }
             
             foreach (DirectoryInfo dir in di.GetDirectories())
             {
                 LimparArquivosTemporarios(dir.FullName, diasAntigos);
             }
         }
         catch (Exception ex)
         {
             Console.WriteLine($"Erro ao limpar arquivos temporários: {ex.Message}");
         }
     }
     ```

2. **Arquivos muito grandes**
   - Compacte arquivos que não estão em uso ativo
   - Armazene apenas os dados necessários

3. **Disco cheio**
   - Libere espaço em disco
   - Use um disco com mais capacidade
   - Implemente verificação de espaço em disco antes de iniciar o processamento:
     ```csharp
     // Exemplo de verificação de espaço em disco
     private bool VerificarEspacoEmDisco(string unidade, long espacoNecessarioBytes)
     {
         DriveInfo drive = new DriveInfo(unidade);
         return drive.AvailableFreeSpace >= espacoNecessarioBytes;
     }
     ```

### Problemas de Permissão de Acesso

**Sintomas:**
- Erros de "acesso negado"
- Arquivos não podem ser criados ou modificados

**Possíveis Causas e Soluções:**

1. **Permissões insuficientes**
   - Execute o aplicativo como administrador
   - Ajuste as permissões das pastas relevantes

2. **Arquivos em uso por outros processos**
   - Verifique se os arquivos não estão abertos em outros aplicativos
   - Implemente mecanismo de tentativas com espera:
     ```csharp
     // Exemplo de mecanismo de tentativas
     private bool TentarAcessarArquivo(string caminhoArquivo, Action<FileStream> acao, int maxTentativas = 3)
     {
         int tentativa = 0;
         
         while (tentativa < maxTentativas)
         {
             try
             {
                 using (FileStream fs = new FileStream(caminhoArquivo, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                 {
                     acao(fs);
                     return true;
                 }
             }
             catch (IOException)
             {
                 tentativa++;
                 
                 if (tentativa >= maxTentativas)
                     return false;
                 
                 // Esperar antes da próxima tentativa
                 Thread.Sleep(1000);
             }
         }
         
         return false;
     }
     ```

## Logs e Depuração

### Habilitando Logs Detalhados

Para facilitar a depuração de problemas, você pode habilitar logs detalhados no sistema:

```csharp
// Exemplo de implementação de log detalhado
private static void LogarInformacao(string mensagem, string nivel = "INFO")
{
    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    string logEntry = $"[{timestamp}] [{nivel}] {mensagem}";
    
    // Exibir no console de depuração
    Debug.WriteLine(logEntry);
    
    // Salvar em arquivo de log
    string caminhoLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"log_{DateTime.Now:yyyyMMdd}.txt");
    
    try
    {
        // Criar diretório de logs se não existir
        Directory.CreateDirectory(Path.GetDirectoryName(caminhoLog));
        
        // Adicionar entrada ao arquivo de log
        File.AppendAllText(caminhoLog, logEntry + Environment.NewLine);
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Erro ao salvar log: {ex.Message}");
    }
}

// Uso:
LogarInformacao("Iniciando conversão de PDF para PNG", "DEBUG");
LogarInformacao("Erro durante o OCR: " + ex.Message, "ERROR");
```

### Depuração Passo a Passo

Para depurar problemas específicos, você pode usar a depuração passo a passo no Visual Studio:

1. Adicione pontos de interrupção (breakpoints) em pontos críticos do código
2. Execute o aplicativo em modo de depuração (F5)
3. Examine variáveis e o fluxo de execução
4. Use a janela "Inspeção" para avaliar expressões durante a depuração

### Captura de Exceções

Implemente captura e registro detalhado de exceções para facilitar a identificação de problemas:

```csharp
try
{
    // Código que pode gerar exceções
}
catch (Exception ex)
{
    // Registrar informações detalhadas sobre a exceção
    LogarInformacao($"Exceção: {ex.Message}", "ERROR");
    LogarInformacao($"Tipo: {ex.GetType().FullName}", "ERROR");
    LogarInformacao($"Stack Trace: {ex.StackTrace}", "ERROR");
    
    if (ex.InnerException != null)
    {
        LogarInformacao($"Inner Exception: {ex.InnerException.Message}", "ERROR");
        LogarInformacao($"Inner Stack Trace: {ex.InnerException.StackTrace}", "ERROR");
    }
    
    // Exibir mensagem para o usuário
    MessageBox.Show($"Ocorreu um erro: {ex.Message}\n\nConsulte o arquivo de log para mais detalhes.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
}
```

---

Este guia de solução de problemas aborda os problemas mais comuns encontrados durante o uso do sistema de preprocessamento de documentos PDF do projeto GabIA. Se você encontrar problemas não abordados aqui, consulte a documentação completa ou entre em contato com a equipe de desenvolvimento.

Para mais informações, consulte o [README do Preprocessamento de PDF](README_PREPROCESSAMENTO_PDF.md).
