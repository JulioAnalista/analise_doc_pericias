using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GabIA.WPF
{
    public class RodaAplicacaoConsoleJava
    {
        private string javaPath;
        private string javaApp;
        private List<string> args;

        public RodaAplicacaoConsoleJava(string javaPath, string javaApp, List<string> args)
        {
            this.javaPath = javaPath;
            this.javaApp = javaApp;
            this.args = args;
        }

        public async Task RunProcessAsync(string javaApplication, string jarFile, string pythonScript, string pdfFile, string outputDirectory)
        {
            string commandLineArgs = $"-jar {jarFile} {pythonScript} {pdfFile} {outputDirectory}";
            var startInfo = new ProcessStartInfo(javaApplication, commandLineArgs)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                await Task.Run(() => process.WaitForExit());
            }
        }

        public async Task RunProcessPDFAsync(string javaApplication, string jarFile, string pythonScript, string pdfFile, string outputDirectory, string logFile, int initialPage)
        {
            string commandLineArgs = $"-jar {jarFile} {pythonScript} {pdfFile} {outputDirectory} {logFile} {initialPage}";
            var startInfo = new ProcessStartInfo(javaApplication, commandLineArgs)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                await Task.Run(() => process.WaitForExit());
            }
        }

        public async Task RunAsync()
        {
            string commandLineArgs = $"-jar {javaApp}";
            foreach (string arg in args)
            {
                commandLineArgs += " " + arg;
            }
            Debug.WriteLine(commandLineArgs);

            var procStartInfo = new ProcessStartInfo(javaPath, commandLineArgs)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var proc = new Process
            {
                StartInfo = procStartInfo
            };

            try
            {
                proc.Start();

                var outputTask = Task.Factory.StartNew(() =>
                {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        var line = proc.StandardOutput.ReadLine();
                        Console.WriteLine(line);
                    }
                });

                var errorTask = Task.Factory.StartNew(() =>
                {
                    while (!proc.StandardError.EndOfStream)
                    {
                        var line = proc.StandardError.ReadLine();
                        Console.Error.WriteLine(line);
                    }
                });

                await Task.WhenAll(outputTask, errorTask);
                await Task.Run(() => proc.WaitForExit());
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", e.Message, e.StackTrace?.ToString());
            }
        }


        public void Run_new()
        {
            string commandLineArgs = $"-jar {javaApp}";
            foreach (string arg in args)
            {
                commandLineArgs += " " + arg;
            }

            var procStartInfo = new ProcessStartInfo(javaPath, commandLineArgs)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var proc = new Process
            {
                StartInfo = procStartInfo
            };

            var outputFileCreated = new ManualResetEventSlim(false);

            try
            {
                proc.Start();

                var outputTask = Task.Factory.StartNew(() =>
                {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        var line = proc.StandardOutput.ReadLine();
                        Console.WriteLine(line);

                        // replace "outputFilePath" with the path of your output file
                        string outputFilePath = "";
                        if (File.Exists(outputFilePath))
                        {
                            outputFileCreated.Set();
                        }
                    }
                });

                var errorTask = Task.Factory.StartNew(() =>
                {
                    while (!proc.StandardError.EndOfStream)
                    {
                        var line = proc.StandardError.ReadLine();
                        Console.Error.WriteLine(line);
                    }
                });

                var success = outputFileCreated.Wait(TimeSpan.FromMinutes(5)); // replace 5 with the maximum time you want to wait

                if (!success)
                {
                    // output file wasn't created in time, kill the process
                    proc.Kill();
                }

                Task.WaitAll(outputTask, errorTask);
                proc.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", e.Message, e.StackTrace?.ToString());
            }
        }



        public void Run_old()
        {
            string commandLineArgs = $"-jar {javaApp}";
            foreach (string arg in args)
            {
                commandLineArgs += " " + arg;
            }
            //Debug.WriteLine("rodando Java");
            //MessageBox.Show(commandLineArgs);
            var procStartInfo = new ProcessStartInfo(javaPath, commandLineArgs)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var proc = new Process
            {
                StartInfo = procStartInfo
            };

            try
            {
                proc.Start();

                var outputTask = Task.Factory.StartNew(() =>
                {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        var line = proc.StandardOutput.ReadLine();
                        Console.WriteLine(line);
                    }
                });

                var errorTask = Task.Factory.StartNew(() =>
                {
                    while (!proc.StandardError.EndOfStream)
                    {
                        var line = proc.StandardError.ReadLine();
                        Console.Error.WriteLine(line);
                    }
                });

                Task.WaitAll(outputTask, errorTask);
                proc.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", e.Message, e.StackTrace?.ToString());
            }
        }

        public void Run()
        {
            string commandLineArgs = $"-jar {javaApp}";
            foreach (string arg in args)
            {
                commandLineArgs += " " + arg;
            }
            Debug.WriteLine(commandLineArgs);
            //MessageBox.Show(commandLineArgs);
            
            
            
            var procStartInfo = new ProcessStartInfo(javaPath, commandLineArgs)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var proc = new Process
            {
                StartInfo = procStartInfo
            };

            try
            {

                proc.Start();

                int timeout = 60000;  // 60,000 milliseconds = 60 seconds

                var outputTask = Task.Factory.StartNew(() =>
                {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        var line = proc.StandardOutput.ReadLine();
                        Console.WriteLine(line);
                    }
                });

                var errorTask = Task.Factory.StartNew(() =>
                {
                    while (!proc.StandardError.EndOfStream)
                    {
                        var line = proc.StandardError.ReadLine();
                        Console.Error.WriteLine(line);
                    }
                });

                bool outputCompleted = outputTask.Wait(timeout);
                bool errorCompleted = errorTask.Wait(timeout);

                if (!outputCompleted || !errorCompleted || !proc.WaitForExit(timeout))
                {
                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }
                }
                else
                {
                    proc.WaitForExit();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", e.Message, e.StackTrace?.ToString());
            }
        }
    }
}