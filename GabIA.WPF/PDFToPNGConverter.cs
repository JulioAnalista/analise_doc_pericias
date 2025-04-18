using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ghostscript.NET.Rasterizer;
using Ghostscript.NET;
using System.Drawing.Imaging;
using System.Drawing;
using System.Diagnostics;

namespace GabIA.WPF
{
    public class PDFToPNGConverter
    {
        public void ConvertPdfPageToPng(string inputPdfPath, string outputPngPath, int lowDpi, int highDpi, int pageNumber)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                string filePngLowRes = Path.Combine(outputPngPath, Path.GetFileNameWithoutExtension(inputPdfPath) + "L.png");
                string filePngHighRes = Path.Combine(outputPngPath, Path.GetFileNameWithoutExtension(inputPdfPath) + "H.png");
                GhostscriptVersionInfo gsVersionInfo = GhostscriptVersionInfo.GetLastInstalledVersion();

                using (GhostscriptRasterizer rasterizer = new GhostscriptRasterizer())
                {
                    rasterizer.Open(inputPdfPath, gsVersionInfo, false);

                    // Primeira rasterização em baixa resolução
                    using (Image imgLowRes = rasterizer.GetPage(lowDpi, pageNumber))
                    {
                        Bitmap bitmapLowRes = new Bitmap(imgLowRes);
                        bitmapLowRes.Save(filePngLowRes, ImageFormat.Png);
                    }

                    ProcessamentoDeTexto processador = new ProcessamentoDeTexto();
                    string ocrContent = processador.PerformOcr(filePngLowRes);

                    // Analisar os resultados do OCR
                    if (IsPoorQualityOcr(ocrContent) || ocrContent.Length < 20)
                    {
                        // Rasterização em alta resolução
                        using (Image imgHighRes = rasterizer.GetPage(highDpi, pageNumber))
                        {
                            Bitmap bitmapHighRes = new Bitmap(imgHighRes);

                            // Aplicar pré-processamento
                            float contrastValue = 1.5f;
                            float brightnessValue = 1.0f;
                            PreProcessImageForOCR(bitmapHighRes, contrastValue, brightnessValue);

                            // Salvar a imagem processada
                            bitmapHighRes.Save(filePngHighRes, ImageFormat.Png);
                        }
                    }
                    else
                    {
                        string eventualOCR = Path.GetFileNameWithoutExtension(inputPdfPath) + "OCR.txt";
                        string ocrFilePath = Path.GetDirectoryName(outputPngPath);
                        ocrFilePath = Path.Combine(Path.GetDirectoryName(ocrFilePath),"PecasProcessuais");
                        ocrFilePath = Path.Combine(ocrFilePath, "OCR", eventualOCR);
                        File.WriteAllText(ocrFilePath, ocrContent);
                    }
                    stopwatch.Stop();
                    Debug.WriteLine($"O tempo de conversão foi de {stopwatch.ElapsedMilliseconds} milissegundos.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocorreu um erro durante a conversão: {ex.Message}");
            }
        }
        private Bitmap PreProcessImageForOCR(Bitmap bmp, float contrast, float brightness)
        {
            // Aplicar filtro gaussiano para suavização
            ApplyGaussianBlur(bmp);

            // Ajustar contraste e brilho
            AdjustContrastAndBrightness(bmp, contrast, brightness);

            // Binarização adaptativa
            ApplyAdaptiveThresholding(bmp);

            // Remover speckles
            RemoveSpeckles(bmp);

            return bmp;
        }

        private bool IsPoorQualityOcr(string ocrContent)
        {
            int shortWordThreshold = 3; // Limite para considerar uma palavra curta
            int longWordThreshold = 4; // Limite para considerar uma palavra longa
            float ratioThreshold = 0.1f; // Limite da razão entre palavras longas e curtas
            float isolatedCharRatioThreshold = 0.05f; // Limite para a proporção de caracteres isolados

            int shortWordCount = 0;
            int longWordCount = 0;
            int isolatedCharCount = 0;
            int totalCharCount = 0;

            foreach (string word in ocrContent.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int wordLength = word.Length;
                totalCharCount += wordLength;

                if (wordLength < shortWordThreshold)
                    shortWordCount++;
                else if (wordLength > longWordThreshold)
                    longWordCount++;

                if (wordLength == 1 && !char.IsPunctuation(word[0]))
                    isolatedCharCount++;
            }

            // Calcula as razões
            float longToShortRatio = (shortWordCount > 0) ? (float)longWordCount / shortWordCount : longWordCount;
            float isolatedCharRatio = (totalCharCount > 0) ? (float)isolatedCharCount / totalCharCount : 0;

            // Verifica se as razões estão abaixo dos limiares estabelecidos
            return longToShortRatio < ratioThreshold || isolatedCharRatio > isolatedCharRatioThreshold;
        }


        private void ApplyAdaptiveThresholding(Bitmap bmp)
        {
            int blockSize = 11; // Tamanho do bloco para calcular a média local
            int offset = 10; // Offset para o limiar

            Bitmap tempBmp = (Bitmap)bmp.Clone();

            for (int i = blockSize / 2; i < bmp.Width - blockSize / 2; i++)
            {
                for (int j = blockSize / 2; j < bmp.Height - blockSize / 2; j++)
                {
                    int sum = 0;

                    for (int x = -blockSize / 2; x <= blockSize / 2; x++)
                    {
                        for (int y = -blockSize / 2; y <= blockSize / 2; y++)
                        {
                            Color pixel = tempBmp.GetPixel(i + x, j + y);
                            sum += (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                        }
                    }

                    int average = sum / (blockSize * blockSize);
                    Color currentPixel = tempBmp.GetPixel(i, j);
                    int gray = (int)(currentPixel.R * 0.3 + currentPixel.G * 0.59 + currentPixel.B * 0.11);

                    // Aplica a binarização adaptativa
                    Color newColor = gray > average - offset ? Color.White : Color.Black;
                    bmp.SetPixel(i, j, newColor);
                }
            }
        }


        private void RemoveSpeckles(Bitmap bmp)
        {
            double mmToInch = 25.4;
            int dpi = 600;
            double minSpeckleSizeMm = 0.5;
            int minSpeckleSizePixels = (int)(minSpeckleSizeMm / mmToInch * dpi);

            Bitmap tempBmp = (Bitmap)bmp.Clone();

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    // Verifica se o pixel atual é um pixel preto
                    if (tempBmp.GetPixel(i, j).R == 0)
                    {
                        // Iniciar a identificação do speckle
                        List<Point> specklePixels = new List<Point>();
                        IdentifySpeckle(tempBmp, i, j, specklePixels);

                        // Se o speckle for menor que o tamanho mínimo e estiver isolado, removê-lo
                        if (specklePixels.Count <= minSpeckleSizePixels)
                        {
                            foreach (var pixel in specklePixels)
                            {
                                bmp.SetPixel(pixel.X, pixel.Y, Color.White);
                            }
                        }
                    }
                }
            }
        }


        private void IdentifySpeckle(Bitmap bmp, int x, int y, List<Point> specklePixels)
        {
            try
            {
                // Verifica se o ponto já foi visitado ou está fora dos limites da imagem
                if (x < 0 || x >= bmp.Width || y < 0 || y >= bmp.Height || specklePixels.Contains(new Point(x, y)))
                {
                    return;
                }

                // Verifica se o pixel é preto
                if (bmp.GetPixel(x, y).R == 0) // Assumindo que a imagem já está em escala de cinza
                {
                    specklePixels.Add(new Point(x, y));

                    // Verificar os pixels adjacentes
                    IdentifySpeckle(bmp, x + 1, y, specklePixels); // Direita
                    IdentifySpeckle(bmp, x - 1, y, specklePixels); // Esquerda
                    IdentifySpeckle(bmp, x, y + 1, specklePixels); // Abaixo
                    IdentifySpeckle(bmp, x, y - 1, specklePixels); // Acima
                }
            }
            catch (Exception ex)
            {
                // Tratar a exceção aqui
                // Por exemplo, você pode registrar a exceção ou exibir uma mensagem de erro
                Console.WriteLine("Erro ao identificar speckle: " + ex.Message);
            }
        }



        private void EnhanceEdges(Bitmap bmp)
        {
            // Criar uma cópia da imagem original para não alterar a original durante o processamento
            Bitmap tempBmp = (Bitmap)bmp.Clone();

            int width = bmp.Width;
            int height = bmp.Height;

            // Definindo os kernels do filtro Sobel
            int[,] xSobel = new int[3, 3]
            {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            };

            int[,] ySobel = new int[3, 3]
            {
                {  1,  2,  1 },
                {  0,  0,  0 },
                { -1, -2, -1 }
            };

            // Aplicar o filtro Sobel
            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    int xGradient = 0;
                    int yGradient = 0;

                    // Calcular gradientes X e Y
                    for (int xi = -1; xi <= 1; xi++)
                    {
                        for (int yi = -1; yi <= 1; yi++)
                        {
                            Color pixel = tempBmp.GetPixel(i + xi, j + yi);
                            int gray = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);

                            xGradient += gray * xSobel[xi + 1, yi + 1];
                            yGradient += gray * ySobel[xi + 1, yi + 1];
                        }
                    }

                    int gradientMagnitude = (int)Math.Sqrt((xGradient * xGradient) + (yGradient * yGradient));
                    gradientMagnitude = Math.Clamp(gradientMagnitude, 0, 255);
                    Color newColor = Color.FromArgb(gradientMagnitude, gradientMagnitude, gradientMagnitude);
                    bmp.SetPixel(i, j, newColor);
                }
            }
        }
        private void ApplyGaussianBlur(Bitmap bmp)
        {
            int filterSize = 5;
            double[,] filter = GenerateGaussianKernel(filterSize, 1.0); // Sigma = 1.0

            Bitmap tempBmp = (Bitmap)bmp.Clone();

            for (int i = filterSize / 2; i < bmp.Width - filterSize / 2; i++)
            {
                for (int j = filterSize / 2; j < bmp.Height - filterSize / 2; j++)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    for (int filterX = 0; filterX < filterSize; filterX++)
                    {
                        for (int filterY = 0; filterY < filterSize; filterY++)
                        {
                            int imageX = i - filterSize / 2 + filterX;
                            int imageY = j - filterSize / 2 + filterY;
                            Color pixel = tempBmp.GetPixel(imageX, imageY);

                            red += pixel.R * filter[filterX, filterY];
                            green += pixel.G * filter[filterX, filterY];
                            blue += pixel.B * filter[filterX, filterY];
                        }
                    }

                    int r = Math.Min(Math.Max((int)(red), 0), 255);
                    int g = Math.Min(Math.Max((int)(green), 0), 255);
                    int b = Math.Min(Math.Max((int)(blue), 0), 255);

                    bmp.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }
        }



        private double[,] GenerateGaussianKernel(int length, double weight)
        {
            double[,] kernel = new double[length, length];
            double sumTotal = 0;

            int filterOffset = length / 2;
            double calculatedEuler = 1.0 / (2.0 * Math.PI * Math.Pow(weight, 2));

            for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
            {
                for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                {
                    kernel[filterY + filterOffset, filterX + filterOffset] = calculatedEuler * Math.Exp(-(filterX * filterX + filterY * filterY) / (2 * weight * weight));

                    sumTotal += kernel[filterY + filterOffset, filterX + filterOffset];
                }
            }

            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    kernel[y, x] = kernel[y, x] * (1.0 / sumTotal);
                }
            }

            return kernel;
        }



        private void ConvertToBinary(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color pixel = bmp.GetPixel(i, j);
                    int gray = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    Color newPixel = gray > 128 ? Color.White : Color.Black;
                    bmp.SetPixel(i, j, newPixel);
                }
            }
        }


        private void AdjustContrastAndBrightness(Bitmap bmp, float contrast, float brightness)
        {
            float adjustedBrightness = brightness - 1.0f;
            // Matriz de cores para ajuste de contraste e brilho
            float[][] ptsArray ={
            new float[] {contrast, 0, 0, 0, 0},
            new float[] {0, contrast, 0, 0, 0},
            new float[] {0, 0, contrast, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, imageAttributes);
        }
    }
}