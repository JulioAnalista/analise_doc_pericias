using MySqlX.XDevAPI;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
//using Azure.AI.Language.Conversations;
using static System.Environment;

namespace GabIA.WPF
{
    
    public class AzureOpenai
    {
        public async Task GetChatCompletionsAsync()
        {
            string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            string key = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");

            endpoint = "https://alx.openai.azure.com/";
            key = "105af24142b940ab842b9759bc67ffcf";

            OpenAIClient client = new(new Uri(endpoint), new AzureKeyCredential(key));

            var chatCompletionsOptions = new ChatCompletionsOptions
            {
                DeploymentName = "alx_turbo16", // Use o nome da implantação personalizada do seu modelo
                Messages =
                {
                    new ChatRequestSystemMessage("You are a helpful assistant."),
                    new ChatRequestUserMessage("Azure tem suporte para quais modelos da OpenAI?"),
                    // Continue adicionando mensagens conforme necessário
                },
                MaxTokens = 2000
            };

            Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);

            Debug.WriteLine(response.Value.Choices[0].Message.Content);
            Debug.WriteLine("feito!");
        }
        // Constructor da classe
        public AzureOpenai()
        {
            // Configuração do cliente pode ser feita aqui ou em um método separado
        }
    }
}
