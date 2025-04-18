# Preprocessamento de Documentos PDF - GabIA

Este documento descreve o sistema de preprocessamento de documentos PDF implementado no projeto GabIA. O sistema é responsável por converter documentos PDF em texto estruturado para análise posterior por modelos de IA e outras ferramentas de processamento.

## Índice

- [Visão Geral](#visão-geral)
- [Requisitos e Dependências](#requisitos-e-dependências)
- [Estrutura de Diretórios](#estrutura-de-diretórios)
- [Fluxo de Trabalho](#fluxo-de-trabalho)
- [Componentes Principais](#componentes-principais)
- [Configuração e Parâmetros](#configuração-e-parâmetros)
- [Exemplos de Uso](#exemplos-de-uso)
- [Solução de Problemas](#solução-de-problemas)
- [Contribuição e Desenvolvimento](#contribuição-e-desenvolvimento)

## Visão Geral

O sistema de preprocessamento de documentos PDF do GabIA é um pipeline complexo que converte documentos PDF em texto estruturado através de várias etapas:

1. **Conversão de PDF para Imagem**: Transforma páginas de PDF em imagens PNG
2. **Reconhecimento Óptico de Caracteres (OCR)**: Extrai texto das imagens
3. **Pré-processamento de Imagem**: Melhora a qualidade das imagens para OCR
4. **Análise de Qualidade do OCR**: Verifica a qualidade do texto extraído
5. **Processamento do Texto**: Organiza e estrutura o texto extraído
6. **Geração de Arquivos Intermediários**: Cria arquivos de texto e JSON
7. **Combinação de Páginas**: Agrupa páginas relacionadas em documentos completos

O resultado final é um conjunto de arquivos de texto e JSON organizados em uma estrutura de diretórios específica, prontos para serem processados por modelos de IA ou outras ferramentas de análise.

## Requisitos e Dependências

### Software Necessário

- **.NET Framework 7.0** ou superior
- **Ghostscript** (versão 9.50 ou superior)
- **Tesseract OCR** (versão 4.1.1 ou superior)
- **Python 3.x** (para scripts auxiliares de OCR)

### Bibliotecas .NET

- **Ghostscript.NET**: Para conversão de PDF para imagem
- **Tesseract**: Para OCR (reconhecimento óptico de caracteres)
- **iText7**: Para manipulação de PDFs
- **System.Drawing**: Para processamento de imagens
- **Newtonsoft.Json**: Para manipulação de JSON

### Instalação

1. **Ghostscript**:
   - Baixe e instale a versão mais recente do [Ghostscript](https://www.ghostscript.com/download.html)
   - Certifique-se de que o caminho de instalação está no PATH do sistema

2. **Tesseract OCR**:
   - Baixe e instale o [Tesseract OCR](https://github.com/UB-Mannheim/tesseract/wiki)
   - Instale o pacote de idioma português (por)
   - O caminho padrão esperado é `C:\Arquivos de Programas\Tesseract-OCR\tessdata`

3. **Python e Dependências**:
   - Instale o Python 3.x
   - Instale as bibliotecas necessárias: `pip install pytesseract pillow opencv-python`

## Estrutura de Diretórios

O sistema utiliza a seguinte estrutura de diretórios para organizar os arquivos:

```
d:\PJe\Processos\[NUMERO_PROCESSO]\
├── PaginasPDF\       # Páginas individuais dos PDFs
├── PNG\              # Imagens PNG geradas a partir dos PDFs
├── OCR\              # Textos extraídos pelo OCR
├── PecasProcessuais\
│   ├── PecasTxt\     # Textos combinados por documento
│   └── PecasPDF\     # PDFs combinados por documento
└── ModeloDeLinguagem\
    └── JsonlOriginal\ # Arquivos JSON para processamento por IA
```

## Fluxo de Trabalho

O fluxo de trabalho completo do preprocessamento de documentos PDF é o seguinte:

1. **Inicialização**:
   - O processo é iniciado através do método `Separa_Pecas_PDF_Clicked`
   - Os PDFs são movidos para a pasta de destino
   - O método `separaPecasPDF()` é chamado para iniciar o processamento

2. **Conversão de PDF para Imagem**:
   - Cada página do PDF é convertida em uma imagem PNG usando `ConvertPdfPageToPng`
   - Inicialmente, uma imagem de baixa resolução (200 DPI) é gerada

3. **OCR e Avaliação de Qualidade**:
   - O OCR é realizado na imagem de baixa resolução
   - A qualidade do texto extraído é avaliada usando métricas como:
     - Proporção entre palavras longas e curtas
     - Presença de caracteres isolados
     - Comprimento total do texto
   - Se a qualidade for baixa, uma imagem de alta resolução (600 DPI) é gerada e pré-processada

4. **Pré-processamento de Imagem** (para imagens de baixa qualidade):
   - Aplicação de filtro gaussiano para redução de ruído
   - Ajuste de contraste e brilho
   - Binarização adaptativa
   - Remoção de "speckles" (pequenos pontos isolados)

5. **Processamento de Texto**:
   - O texto extraído é processado para identificar informações como número do processo, ID e página
   - Páginas relacionadas são agrupadas em documentos completos
   - Os textos são salvos em arquivos separados

6. **Preprocessamento para IA**:
   - O texto é dividido em partes menores (chunks) para processamento por modelos de IA
   - Informações sensíveis podem ser sanitizadas ou substituídas por pseudônimos
   - Os textos são convertidos para formato JSON para processamento posterior

7. **Geração de Arquivos Finais**:
   - Os PDFs originais são combinados em documentos completos usando `MergePdfFiles`
   - Os textos extraídos são organizados na estrutura de diretórios

## Componentes Principais

### PDFToPNGConverter

Classe responsável pela conversão de PDF para imagem e pelo pré-processamento de imagens.

Métodos principais:
- `ConvertPdfPageToPng`: Converte uma página de PDF em imagem PNG
- `PreProcessImageForOCR`: Aplica técnicas de pré-processamento à imagem
- `IsPoorQualityOcr`: Avalia a qualidade do texto extraído pelo OCR

### ProcessamentoDeTexto

Classe responsável pelo processamento do texto extraído e pela organização dos arquivos.

Métodos principais:
- `ProcessaArquivos`: Processa os arquivos de texto extraídos
- `PerformOcr`: Realiza OCR em uma imagem
- `MergePdfFiles`: Combina páginas de PDF em documentos completos
- `MoveConteudoIDCorreto`: Corrige problemas de conteúdo em arquivos de texto

### PreprocessamentoTextoIA

Classe responsável pelo preprocessamento do texto para modelos de IA.

Métodos principais:
- `DividirTexto`: Divide o texto em partes menores (chunks)
- `SanitizaUPPERCASE`: Sanitiza o texto para processamento
- `GerarPseudonimo`: Gera pseudônimos para substituir nomes reais
- `ProcessFilesInDirectoryToJson`: Processa arquivos de texto para formato JSON

## Configuração e Parâmetros

### Parâmetros de Conversão PDF para PNG

- **lowDpi**: Resolução baixa para primeira tentativa de OCR (padrão: 200)
- **highDpi**: Resolução alta para segunda tentativa de OCR (padrão: 600)
- **pageNumber**: Número da página a ser convertida

### Parâmetros de Pré-processamento de Imagem

- **contrastValue**: Valor de contraste para ajuste (padrão: 1.5)
- **brightnessValue**: Valor de brilho para ajuste (padrão: 1.0)
- **blockSize**: Tamanho do bloco para binarização adaptativa (padrão: 11)
- **offset**: Offset para binarização adaptativa (padrão: 10)

### Parâmetros de Avaliação de Qualidade OCR

- **shortWordThreshold**: Limite para considerar uma palavra curta (padrão: 3)
- **longWordThreshold**: Limite para considerar uma palavra longa (padrão: 4)
- **ratioThreshold**: Limite da razão entre palavras longas e curtas (padrão: 0.1)
- **isolatedCharRatioThreshold**: Limite para a proporção de caracteres isolados (padrão: 0.05)

### Parâmetros de Divisão de Texto

- **MaxChunkSize**: Tamanho máximo de cada chunk de texto (padrão: 32KB)

## Exemplos de Uso

### Preprocessamento Básico

```csharp
// Iniciar o preprocessamento de um processo
private async void Separa_Pecas_PDF_Clicked(object sender, RoutedEventArgs e)
{
    MessageBox.Show("Iniciando a conversão dos Processos PDF para Texto.\n\nAguarde a mensagem de término!", "Processamento Automatizado");
    
    await MoveDownloadedPdfToDestinationFolder();
    pdfUserControl.separaPecasPDF();
}
```

### Conversão Manual de PDF para PNG

```csharp
// Converter uma página específica de um PDF para PNG
PDFToPNGConverter converter = new PDFToPNGConverter();
converter.ConvertPdfPageToPng(
    @"d:\PJe\Processos\0123456-78.2022.8.07.0008\documento.pdf", 
    @"d:\PJe\Processos\0123456-78.2022.8.07.0008\PNG", 
    200, 600, 1);
```

### Realizar OCR em uma Imagem

```csharp
// Realizar OCR em uma imagem
ProcessamentoDeTexto processador = new ProcessamentoDeTexto();
string textoExtraido = processador.PerformOcr(@"d:\PJe\Processos\0123456-78.2022.8.07.0008\PNG\imagem.png");
```

### Dividir Texto em Chunks para IA

```csharp
// Dividir um texto em chunks para processamento por IA
string textoCompleto = File.ReadAllText(@"d:\PJe\Processos\0123456-78.2022.8.07.0008\PecasProcessuais\PecasTxt\documento.txt");
List<string> chunks = PreprocessamentoTextoIA.DividirTexto(textoCompleto, 32 * 1024);
```

## Solução de Problemas

### Problemas Comuns

1. **OCR de Baixa Qualidade**:
   - Verifique se o Tesseract está instalado corretamente
   - Verifique se o pacote de idioma português está instalado
   - Aumente a resolução (DPI) da imagem
   - Ajuste os parâmetros de pré-processamento de imagem

2. **Erro na Conversão de PDF para PNG**:
   - Verifique se o Ghostscript está instalado corretamente
   - Verifique se o caminho do Ghostscript está no PATH do sistema
   - Verifique se o PDF não está corrompido ou protegido

3. **Erro na Combinação de PDFs**:
   - Verifique se os arquivos PDF existem
   - Verifique se os arquivos PDF não estão em uso por outro processo
   - Verifique se há espaço suficiente em disco

4. **Erro no Processamento de Texto**:
   - Verifique se o formato do texto extraído está correto
   - Verifique se as expressões regulares estão capturando as informações corretamente

### Logs e Depuração

O sistema utiliza `Debug.WriteLine` para registrar informações de depuração. Para visualizar esses logs:

1. Execute o aplicativo no Visual Studio em modo de depuração
2. Abra a janela "Saída" (Output)
3. Selecione "Depurar" (Debug) no menu suspenso

## Contribuição e Desenvolvimento

### Estrutura do Código

O código do sistema de preprocessamento está organizado nas seguintes classes:

- `PDFToPNGConverter.cs`: Conversão de PDF para imagem e pré-processamento de imagens
- `ProcessamentoDeTexto.cs`: Processamento de texto e organização de arquivos
- `PreprocessamentoTextoIA.cs`: Preprocessamento de texto para modelos de IA

### Melhorias Futuras

Algumas melhorias que podem ser implementadas:

1. **Paralelização**: Implementar processamento paralelo para melhorar o desempenho
2. **Melhoria do OCR**: Integrar modelos de OCR mais avançados baseados em deep learning
3. **Detecção Automática de Idioma**: Implementar detecção automática de idioma para OCR
4. **Interface Gráfica**: Desenvolver uma interface gráfica para monitoramento e controle do processo
5. **Logging Avançado**: Implementar um sistema de logging mais avançado para facilitar a depuração

### Diretrizes de Contribuição

Para contribuir com o desenvolvimento do sistema:

1. Crie um fork do repositório
2. Crie uma branch para sua feature (`git checkout -b feature/nova-feature`)
3. Faça commit das suas alterações (`git commit -m 'Adiciona nova feature'`)
4. Faça push para a branch (`git push origin feature/nova-feature`)
5. Abra um Pull Request

---

## Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo LICENSE para detalhes.

---

Desenvolvido como parte do projeto GabIA para análise e processamento de documentos jurídicos.
