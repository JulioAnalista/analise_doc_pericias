using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO.Enumeration;
using Adobe.PDFServicesSDK.io.pdfproperties;
using static GabIA.WPF.APIRequest;
using System.Threading;
using Azure.AI.OpenAI;
using Azure;

namespace GabIA.WPF
{


    public class APIRequest
    {
        private static readonly HttpClient client = new HttpClient();

        public int TaskId { get; set; }
        public RequestData Data { get; set; } = new RequestData();
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        public int AttemptsLeft { get; set; } = 3;
        public List<string> Result { get; set; } = new List<string>();

        public class RequestData
        {
            public string Model { get; set; }
            public string SystemContent { get; set; }
            public JsonData JsonData { get; set; } = new JsonData();
            public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        }

        public class JsonData
        {
            public List<Linha> Linhas { get; set; } = new List<Linha>();
        }

        public class Linha
        {
            public string linha { get; set; }
        }

        public class TempJsonData
        {
            public string tpo_ato { get; set; }
            public string nr_proc { get; set; }
            public int id { get; set; }
            public List<Linha> Linhas { get; set; }
        }

        private static OpenAIClient CreateOpenAIClient(string endpoint, string apiKey)
        {
            return new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }

        public static async Task<(string, Dictionary<string, string>)> CallApiAzure(string deploymentName, RequestData data, int maxAttempts)
        {
            string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");

            endpoint = "https://alx.openai.azure.com/";
            apiKey = "105af24142b940ab842b9759bc67ffcf";

            var client = CreateOpenAIClient(endpoint, apiKey);

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {

                //Debug.WriteLine(JsonConvert.SerializeObject(data.JsonData.Linhas));
                // Supondo que data.JsonData.Linhas é uma List<Linha> onde Linha é uma classe com a propriedade 'linha'
                var linhasConcatenadas = String.Join("\n", data.JsonData.Linhas.Select(l => l.linha));

                // Agora linhasConcatenadas contém todas as linhas, cada uma separada por uma nova linha

                //Debug.WriteLine(linhasConcatenadas);
                try
                {
                    var chatCompletionsOptions = new ChatCompletionsOptions
                    {
                        DeploymentName = deploymentName,
                        Messages =
                        {
                            new ChatRequestSystemMessage(data.SystemContent),
                            //new ChatRequestUserMessage(string.Join(" ", data.JsonData.Linhas.Select(l => l.linha)))
                            new ChatRequestUserMessage(linhasConcatenadas.ToString())
                        },
                        MaxTokens = 8000
                    };

                    var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                    string jsonResponse = JsonConvert.SerializeObject(response.Value);

                    if (jsonResponse.Contains("error"))
                    {
                        throw new Exception($"Request failed with error: {jsonResponse}");
                    }

                    return (jsonResponse, data.Metadata);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Exception on attempt {attempt}: {e.Message}");
                    if (attempt == maxAttempts)
                    {
                        throw new Exception("Max retry attempts reached.");
                    }

                    int delay = CalculateExponentialBackoff(attempt);
                    Debug.WriteLine($"Attempt {attempt} of {maxAttempts} failed. Retrying after {delay} ms...");
                    await Task.Delay(delay);
                }
            }

            throw new Exception("Max retry attempts reached.");
        }

        public async Task<(string, Dictionary<string, string>)> CallApi(string requestUrl, string apiKey, int maxAttempts, string saidaLog)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            Debug.WriteLine(this.Data.SystemContent);

            var requestJson = new
            {
                model = this.Data.Model,
                messages = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string> { { "role", "system" }, { "content", this.Data.SystemContent } },
                    new Dictionary<string, string> { { "role", "user" }, { "content", string.Join(" ", this.Data.JsonData.Linhas.Select(l => l.linha)) } }
                }
            };

            var requestJsonString = JsonConvert.SerializeObject(requestJson);

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var content = new StringContent(requestJsonString, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(requestUrl, content);
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode || jsonResponse.Contains("error"))
                    {
                        throw new Exception($"Request failed with error: {jsonResponse}");
                    }

                    return (jsonResponse, this.Metadata);
                }
                catch (HttpRequestException e)
                {
                    Debug.WriteLine($"HttpRequestException on attempt {attempt}: {e.Message}");
                    int delay = CalculateExponentialBackoff(attempt);
                    Debug.WriteLine($"Attempt {attempt} of {maxAttempts} failed. Retrying after {delay} ms...");
                    await Task.Delay(delay);
                }
            }
            throw new Exception("Max retry attempts reached.");
        }

        private static int CalculateExponentialBackoff(int attempt)
        {
            return (int)(Math.Pow(2, attempt) * 1000); // Ajuste conforme necessário
        }

        public static async Task ProcessDirectory_Azure(string inputDirectory, string outputDirectory, string apiKeyFilePath, string systemContent)
        {
            var files = Directory.EnumerateFiles(inputDirectory);
            List<Task> tasks = new List<Task>();
            string apiKey = LoadApiKey(apiKeyFilePath); // Implemente LoadApiKey conforme necessário
            string endpoint = "https://api.openai.com/v1/chat/completions"; // Substitua pelo endpoint correto, se necessário

            endpoint = "https://alx.openai.azure.com/";
            apiKey = "105af24142b940ab842b9759bc67ffcf";

            foreach (var file in files)
            {
                var outputFile = Path.Combine(outputDirectory, Path.GetFileName(file));
                var logFileName = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(file) + ".Log");

                // Pausa a aplicação
                Thread.Sleep(5000);

                if (File.Exists(outputFile))
                {
                    Console.WriteLine($"Já processado arquivo: {file}");
                    continue;
                }


                string jsonContent1 = File.ReadAllText(file, Encoding.UTF8);
                var tempJsonData1 = JsonConvert.DeserializeObject<TempJsonData>(jsonContent1);


                var metadata1 = new Dictionary<string, string>
                {
                    { "tpo_ato", tempJsonData1.tpo_ato },
                    { "id", tempJsonData1.id.ToString() },
                    { "nr_proc", tempJsonData1.nr_proc }
                };

                RequestData requestData1 = new RequestData
                {
                    SystemContent = systemContent,
                    JsonData = new JsonData { Linhas = tempJsonData1.Linhas },
                    Metadata = metadata1
                };

                /*Debug.WriteLine(JsonConvert.SerializeObject(requestData1.JsonData.Linhas));
                //Debug.WriteLine(JsonConvert.SerializeObject(requestData1));
                var linhasConcatenadas = String.Join("\n", requestData1.JsonData.Linhas.Select(l => l.linha));
                Debug.WriteLine(linhasConcatenadas.ToString());

                var result1 = await CallApiAzure("alx_turbo16", requestData1, 3);
                string jsonResponse1 = result1.Item1;
                Dictionary<string, string> responseMetadata1 = result1.Item2; */

                try
                {
                    string jsonContent = File.ReadAllText(file, Encoding.UTF8);
                    var tempJsonData = JsonConvert.DeserializeObject<TempJsonData>(jsonContent);

                    var metadata = new Dictionary<string, string>
                {
                    { "tpo_ato", tempJsonData.tpo_ato },
                    { "id", tempJsonData.id.ToString() },
                    { "nr_proc", tempJsonData.nr_proc }
                };

                tasks.Add(Task.Run(async () =>
                {
                    RequestData requestData = new RequestData
                    {
                        SystemContent = systemContent,
                        JsonData = new JsonData { Linhas = tempJsonData.Linhas },
                        Metadata = metadata
                    };

                    // Ajuste para corresponder à assinatura do método CallApiAzure
                    var result = await CallApiAzure("alx_turbo16", requestData, 3);
                    string jsonResponse = result.Item1;
                    Dictionary<string, string> responseMetadata = result.Item2;

                    var outputData = new
                    {
                        response = jsonResponse,
                        metadata = responseMetadata
                    };
                    var outputJson = JsonConvert.SerializeObject(outputData);
                    await File.WriteAllTextAsync(outputFile, outputJson);
                }));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred while processing file {file}: {e.Message}");
                }
            }

            await Task.WhenAll(tasks);
        }
        public async Task<(string, Dictionary<string, string>)> CallApiNonParallel(string requestUrl, string apiKey)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var requestJson = new
            {
                model = this.Data.Model,
                messages = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string> { { "role", "system" }, { "content", this.Data.SystemContent } },
                    new Dictionary<string, string> { { "role", "user" }, { "content", string.Join(" ", this.Data.JsonData.Linhas.Select(l => l.linha)) } }
                }
            };

            var requestJsonString = JsonConvert.SerializeObject(requestJson);

            Debug.WriteLine(requestJsonString);

            using var content = new StringContent(requestJsonString, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(requestUrl, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (jsonResponse.Contains("error"))
            {
                throw new Exception($"Request failed with error {jsonResponse}");
            }

            return (jsonResponse, this.Metadata);
        }
        public static async Task ProcessDirectory(string inputDirectory, string outputDirectory, string apiKeyFilePath, string systemContent)
        {
            var files = Directory.EnumerateFiles(inputDirectory);
            string apiKey = LoadApiKey(apiKeyFilePath);
            int taskId = 1;

            foreach (var file in files)
            {
                try
                {
                    string jsonContent;
                    try
                    {
                        jsonContent = System.IO.File.ReadAllText(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading file: " + ex.Message);
                        continue;
                    }

                    var jsonData = JsonConvert.DeserializeObject<APIRequest.TempJsonData>(jsonContent);
                    var tempJsonData = JsonConvert.DeserializeObject<TempJsonData>(jsonContent);

                    var origem = string.Join(" ", tempJsonData.Linhas.Select(l => l.linha));

                    var request = new APIRequest
                    {
                        TaskId = taskId++,
                        Data = new APIRequest.RequestData
                        {
                            Model = IdentificarModelo(origem),
                            SystemContent = systemContent,
                            JsonData = new APIRequest.JsonData { Linhas = tempJsonData.Linhas }
                        }
                        // Metadata não é mais necessário aqui
                    };

                    // Chamada ao método CallApiNonParallel ajustada para a nova assinatura
                    var jsonResponse = await request.CallApiNonParallel("https://api.openai.com/v1/chat/completions", apiKey);

                    var outputFile = Path.Combine(outputDirectory, Path.GetFileName(file));

                    var outputData = new
                    {
                        response = jsonResponse,
                        //removido daqui também
                    };

                    var outputJson = JsonConvert.SerializeObject(outputData);

                    await File.WriteAllTextAsync(outputFile, outputJson);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred while processing file {file}: {e.Message}");
                }
            }
        }

        public static async Task ProcessDirectory_Para(string inputDirectory, string outputDirectory, string apiKeyFilePath, string systemContent)
        {
            var files = Directory.EnumerateFiles(inputDirectory);
            List<Task> tasks = new List<Task>();
            string apiKey = LoadApiKey(apiKeyFilePath);

            foreach (var file in files)
            {
                var outputFile = Path.Combine(outputDirectory, Path.GetFileName(file));
                var logFileName = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(file) + ".Log");

                // Pausa a aplicação
                Thread.Sleep(512);

                if (File.Exists(outputFile))
                {
                    Console.WriteLine($"Já processado arquivo: {file}");
                    continue;
                }

                try
                {
                    string jsonContent = System.IO.File.ReadAllText(file);
                    var tempJsonData = JsonConvert.DeserializeObject<TempJsonData>(jsonContent);

                    var metadata = new Dictionary<string, string>
                    {
                        { "tpo_ato", tempJsonData.tpo_ato },
                        { "id", tempJsonData.id.ToString() },
                        { "nr_proc", tempJsonData.nr_proc }
                    };

                    var origem = string.Join(" ", tempJsonData.Linhas.Select(l => l.linha));

                    var request = new APIRequest
                    {
                        Data = new RequestData
                        {
                            Model = IdentificarModelo(origem),
                            SystemContent = systemContent,
                            JsonData = new APIRequest.JsonData { Linhas = tempJsonData.Linhas }
                        },
                        Metadata = metadata
                    };

                    tasks.Add(Task.Run(async () =>
                    {
                        var (jsonResponse, responseMetadata) = await request.CallApi("https://api.openai.com/v1/chat/completions", apiKey, 3, logFileName);
                        var outputData = new
                        {
                            response = jsonResponse,
                            metadata = responseMetadata
                        };
                        var outputJson = JsonConvert.SerializeObject(outputData);
                        await File.WriteAllTextAsync(outputFile, outputJson);
                    }));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred while processing file {file}: {e.Message}");
                }
            }
            await Task.WhenAll(tasks);
        }



        public string ReadPromptFromFile(string filePath)
        {
            //filePath = Path.GetFullPath("D:\\PJe\\Dados\\Templates\\sumarizacaoAvançada.txt");
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        public static string LoadApiKey(string apiKeyPath)
        {
            string apiKey = File.ReadLines(apiKeyPath).First().Trim();
            apiKey = apiKey.Split('"')[1];
            return apiKey;
        }
        private static string IdentificarModelo(string conteudo)
        {
            // Obtendo o tamanho da string em bytes
            long tamanhoDoConteudo = System.Text.Encoding.UTF8.GetByteCount(conteudo);

            // Retornando o modelo baseado no tamanho do conteudo
            if (tamanhoDoConteudo < 16000)
            {
                return "gpt-3.5-turbo-1106";
            }
            else
            {
                return "gpt-4-1106-preview";
            }
        }
    }
}
