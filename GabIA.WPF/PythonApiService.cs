using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

public class PythonApiService : IDisposable
{
    private readonly HttpClient _client;
    private bool _isPythonAppRunning;
    private Task _monitoringTask;
    private CancellationTokenSource _cancellationTokenSource;

    public PythonApiService()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8000"), // Endereço da API Python
            Timeout = TimeSpan.FromSeconds(30) // Timeout após 30 segundos
        };
        _isPythonAppRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        _monitoringTask = MonitorPythonAppStatusAsync(_cancellationTokenSource.Token);
    }

    public async Task<string> ObterLogsAsync()
    {
        try
        {
            var response = await _client.GetAsync("/caminho_para_obter_logs");
            response.EnsureSuccessStatusCode();
            var logs = await response.Content.ReadAsStringAsync();
            return logs;
        }
        catch (HttpRequestException e)
        {
            return $"Erro na requisição: {e.Message}";
        }
    }

    public async Task<string> EnviarComandoAsync(string comando)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("/processar", new { comando });
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
            return result.ContainsKey("resultado") ? result["resultado"] : null;
        }
        catch (HttpRequestException e)
        {
            return $"Erro na requisição: {e.Message}";
        }
        catch (JsonException e)
        {
            return $"Erro ao deserializar JSON: {e.Message}";
        }
    }

    private async Task MonitorPythonAppStatusAsync(CancellationToken cancellationToken)
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            if (await CheckIfPythonAppHasFinished())
            {
                break;
            }
            await Task.Delay(1000, cancellationToken); // Verifica a cada segundo
        }
    }

    public async Task<bool> CheckIfPythonAppHasFinished()
    {
        try
        {
            var response = await _client.GetAsync("/status");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content.Contains("Aplicação Terminada"))
                {
                    _isPythonAppRunning = false;
                    StopService();
                    return true;
                }
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Erro na requisição: {e.Message}");
        }
        return false;
    }

    private void StopService()
    {
        if (_client != null)
        {
            _client.Dispose();
        }
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
        }
    }

    public void Dispose()
    {
        StopService();
        _cancellationTokenSource.Dispose();
    }
}


////using Newtonsoft.Json;
////using System;
////using System.Collections.Generic;
////using System.Net.Http;
////using System.Net.Http.Json;
////using System.Threading.Tasks;

////public class PythonApiService : IDisposable
////{
////    private readonly HttpClient _client;
////    private bool _isPythonAppRunning;

////    public PythonApiService()
////    {
////        _client = new HttpClient
////        {
////            BaseAddress = new Uri("http://localhost:8000"), // Endereço da API Python
////            Timeout = TimeSpan.FromSeconds(30) // Timeout após 30 segundos
////        };
////        _isPythonAppRunning = true;
////    }

////    public async Task<string> ObterLogsAsync()
////    {
////        try
////        {
////            var response = await _client.GetAsync("/caminho_para_obter_logs");
////            response.EnsureSuccessStatusCode();
////            var logs = await response.Content.ReadAsStringAsync();
////            return logs;
////        }
////        catch (HttpRequestException e)
////        {
////            return $"Erro na requisição: {e.Message}";
////        }
////    }

////    public async Task<string> EnviarComandoAsync(string comando)
////    {
////        try
////        {
////            var response = await _client.PostAsJsonAsync("/processar", new { comando });
////            response.EnsureSuccessStatusCode();
////            var jsonString = await response.Content.ReadAsStringAsync();
////            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
////            return result.ContainsKey("resultado") ? result["resultado"] : null;
////        }
////        catch (HttpRequestException e)
////        {
////            return $"Erro na requisição: {e.Message}";
////        }
////        catch (JsonException e)
////        {
////            return $"Erro ao deserializar JSON: {e.Message}";
////        }
////    }

////    public async Task<bool> CheckIfPythonAppHasFinished()
////    {
////        try
////        {
////            var response = await _client.GetAsync("/status");
////            if (response.IsSuccessStatusCode)
////            {
////                var content = await response.Content.ReadAsStringAsync();
////                if (content.Contains("Aplicação Terminada"))
////                {
////                    _isPythonAppRunning = false;
////                    StopService();
////                    return true;
////                }
////            }
////        }
////        catch (HttpRequestException e)
////        {
////            // Tratar erro de requisição HTTP
////            Console.WriteLine($"Erro na requisição: {e.Message}");
////        }
////        return false;
////    }

////    private void StopService()
////    {
////        if (_client != null)
////        {
////            _client.Dispose();
////        }
////    }

////    public void Dispose()
////    {
////        StopService();
////    }
////}
