using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;
using System.Windows;

namespace GabIA.WPF
{
    public class MonitoraExcecucaoAssincrona
    {
        private readonly string _path;
        private readonly string _file;
        private readonly System.Timers.Timer _timer;
        private FileSystemWatcher _watcher;
        private TaskCompletionSource<bool> _tcs;
        private static DateTime _startTime;

        public MonitoraExcecucaoAssincrona(string path, string file, double timeoutMilliseconds)
        {
            _path = path;
            _file = file;
            _timer = new System.Timers.Timer(timeoutMilliseconds);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = false;
        }

        private void IniciarMonitoramento(string directoryToMonitor, string fileName)
        {
            _watcher = new FileSystemWatcher
            {
                Path = directoryToMonitor,
                Filter = fileName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _watcher.Renamed += OnChanged;
            _watcher.EnableRaisingEvents = true;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            TimeSpan timeElapsed = DateTime.Now - _startTime;  // Calcular a diferença de tempo

            // Mostrar o tempo decorrido em um MessageBox
            MessageBox.Show($"Evento detectado: {e.ChangeType} no arquivo {e.FullPath}\nTempo decorrido: {timeElapsed.TotalSeconds} segundos",
                            "Notificação de Evento de Arquivo");
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _timer.Stop();
            _tcs?.SetResult(false);
        }

        public void RodaProcessoCMD(string[] args)
        {
            if (args == null || args.Length < 4)
            {
                throw new ArgumentException("Número insuficiente de argumentos.");
            }
            string pythonExecutable = args[0];
            string scriptPath = args[1];
            string[] scriptArguments = args.Skip(2).Take(args.Length - 2).ToArray();
            string scriptArgs = string.Join(" ", scriptArguments);
            string _file = args[args.Length - 1];

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = pythonExecutable,
                Arguments = $"{scriptPath} {scriptArgs}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Debug.WriteLine(result);
                }
            }

            // Tempo de espera máximo para a conclusão do script Python
            TimeSpan timeout = TimeSpan.FromMinutes(1); // Exemplo: 5 minutos

            // Chama a função para aguardar a conclusão do processo
            bool completionStatus = WaitForCompletion(_file, timeout);

            if (completionStatus)
            {
                Debug.WriteLine("Processo Python concluído com sucesso.");
                // Aqui você pode adicionar mais lógica conforme necessário
            }
            else
            {
                MessageBox.Show("Timeout atingido ou falha na execução do script Python.");
                // Lógica de tratamento de erro ou timeout
            }
        }

        bool WaitForCompletion(string filePath, TimeSpan timeout)
        {
            DateTime start = DateTime.Now;
            while (DateTime.Now - start < timeout)
            {
                if (File.Exists(filePath))
                {
                    // Verificar o conteúdo do arquivo para determinar se foi um sucesso ou erro
                    return true;
                }
                Thread.Sleep(1000); // Espera 1 segundo antes de verificar novamente
            }
            return false; // Timeout alcançado
        }



        public Task<bool> StartMonitoring()
        {
            _tcs = new TaskCompletionSource<bool>();

            _watcher = new FileSystemWatcher
            {
                Path = _path,
                Filter = _file,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            _watcher.Created += (source, e) =>
            {
                _timer.Stop();
                _tcs.SetResult(true);
            };

            _watcher.EnableRaisingEvents = true;
            _timer.Start();

            return _tcs.Task;
        }
    }
}
