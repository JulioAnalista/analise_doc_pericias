using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using System.Linq;
using CsvHelper.Configuration;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace GabIA.WPF
{
    public class ProcessoMonitor
    {
        private readonly string _caminhoBase;

        public ProcessoMonitor(string caminhoBase)
        {
            _caminhoBase = caminhoBase;
        }

        public async Task CarregarProcessos_TxtCsv(string processo)
        {
            string nomeProcesso = Path.GetFileNameWithoutExtension(processo);
            string nomeProcessoDiretorio = Path.Combine( Path.GetDirectoryName(processo), nomeProcesso);
            string caminhoDoc = Path.Combine(nomeProcessoDiretorio ,"Doc");
            string caminhoJson = Path.Combine(nomeProcessoDiretorio, "Json");
            string arquivoDocx = Path.Combine(caminhoDoc, nomeProcesso + "_P0001_Proc.json");
            string arquivoCabCsv = Path.Combine(nomeProcessoDiretorio, "Json","extracted_data", nomeProcesso + "Cab_Mov.csv");
            //string arquivoJsonCab = Path.Combine(caminhoJson, "extracted_data", nomeProcesso + "Cab_Mov.Json");
            string arquivoJsonPolos = Path.Combine(caminhoJson, nomeProcesso + "cab_Polos.Json");
            string arquivoJsonMovimentos = Path.Combine(caminhoJson, "extracted_data", nomeProcesso + "cab_Mov.Json");
            string arquivoJsonCab = Path.Combine(caminhoJson, "extracted_data");
            arquivoJsonCab = Path.Combine(arquivoJsonCab, nomeProcesso + "cab_mov.json");

            if (File.Exists(arquivoDocx))
            {
                if (File.Exists(arquivoCabCsv) && !File.Exists(Path.Combine(_caminhoBase, nomeProcesso, nomeProcesso + "Cab.ok")))
                {

                    //ExportCabecalhoToJson(arquivoDocx, arquivoJsonCab);

                    ExtractPolesFromTxt(arquivoDocx, arquivoJsonPolos);

                    ExportMovimentosToJson(arquivoCabCsv, arquivoJsonMovimentos);

                    Debug.WriteLine(arquivoCabCsv);
                    //await WriteTimestampToFile(_caminhoBase, nomeProcesso);

                }
            }
        }

        public static void ExtractPolesFromTxt(string txtFilePath, string jsonFilePath)
        {
            if (!File.Exists(txtFilePath))
            {
                Console.WriteLine("No file found.");
                return;
            }

            var lines = File.ReadAllLines(txtFilePath).ToList();
            var poleSection = new Dictionary<string, List<string>>();
            bool isPoleSection = false;
            bool isNameColumn = false;
            string currentPole = "";

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Se a linha contém a palavra "Polos", a seção de polos começa
                if (trimmedLine.Equals("Polos"))
                {
                    isPoleSection = true;
                    isNameColumn = false;
                    continue;
                }

                // Se a linha contém a palavra "Documentos", a seção de polos termina
                if (isPoleSection && trimmedLine.Equals("Documentos"))
                {
                    isPoleSection = false;
                    break;
                }

                // Se estamos na seção de polos, processar a linha
                if (isPoleSection)
                {
                    // Se a linha começa com "Ativo" ou "Passivo", muda o polo atual
                    if (trimmedLine.StartsWith("Ativo"))
                    {
                        currentPole = "Ativo";
                        poleSection[currentPole] = new List<string>();
                        isNameColumn = true;

                        // Adicionar o nome encontrado na mesma linha
                        var name = trimmedLine.Replace("Ativo", "").Trim();
                        poleSection[currentPole].Add(name);

                        continue;
                    }
                    else if (trimmedLine.StartsWith("Passivo"))
                    {
                        currentPole = "Passivo";
                        poleSection[currentPole] = new List<string>();
                        isNameColumn = true;

                        // Adicionar o nome encontrado na mesma linha
                        var name = trimmedLine.Replace("Passivo", "").Trim();
                        poleSection[currentPole].Add(name);

                        continue;
                    }

                    // Se estamos na coluna de nomes, adicionar o nome ao polo atual
                    if (isNameColumn)
                    {
                        if (string.IsNullOrWhiteSpace(trimmedLine))
                        {
                            // Pular linhas em branco na coluna de nomes
                            continue;
                        }

                        poleSection[currentPole].Add(trimmedLine);
                    }
                }
            }

            // Gravar os polos em um arquivo JSON
            var json = JsonConvert.SerializeObject(poleSection, Formatting.Indented);
            File.WriteAllText(jsonFilePath, json);
        }


        public static void ExportMovimentosToJson(string csvFilePath, string jsonFilePath)
        {
            var movimentos = new List<JObject>();
            bool foundHeader = false;

            using (var reader = new StreamReader(csvFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("Data da Assinatura"))
                    {
                        foundHeader = true;
                        continue;
                    }

                    if (foundHeader)
                    {
                        var colunas = line.Split(',');
                        //Debug.WriteLine(colunas.Length);

                        if (colunas.Length >= 5)
                        {
                            var movimento = new JObject();

                            var id = colunas[1].Trim();
                            var dataAssinatura = colunas[2].Trim();
                            var tipoDocumento = colunas[3].Trim();
                            var descricaoDocumento = colunas[4].Trim();

                            movimento["Id"] = JToken.FromObject(id);
                            movimento["DataAssinatura"] = JToken.FromObject(dataAssinatura);
                            movimento["TipoDocumento"] = JToken.FromObject(tipoDocumento);
                            movimento["DescricaoDocumento"] = JToken.FromObject(descricaoDocumento);

                            movimentos.Add(movimento);
                        }
                        else
                        {
                            Console.WriteLine($"Invalid line: {line}");
                        }
                    }
                }
            }

            // Grava o objeto JSON no arquivo
            var json = JsonConvert.SerializeObject(movimentos, Formatting.Indented);
            File.WriteAllText(jsonFilePath, json);
        }



        public static void ExportCabecalhoToJson(string filePath, string jsonFilePath)
        {
            var lines = File.ReadLines(filePath).ToList();

            string dataHoraConsulta = "";
            string numeroProcesso = "";
            string classe = "";
            string orgaoJulgador = "";
            string valorCausa = "";
            List<string> assuntos = new List<string>();
            string segredoDeJustica = "";

            string patternDataHoraConsulta = @"\d{2}/\d{2}/\d{4} \d{2}:\d{2}:\d{2}";

            foreach (var line in lines)
            {
                if (Regex.IsMatch(line, patternDataHoraConsulta))
                    dataHoraConsulta = Regex.Match(line, patternDataHoraConsulta).Value;
                else if (line.StartsWith("Número:"))
                    numeroProcesso = line.Replace("Número:", "").Trim();
                else if (line.StartsWith("Classe:"))
                    classe = line.Replace("Classe:", "").Trim();
                else if (line.StartsWith("Órgão Julgador:"))
                    orgaoJulgador = line.Replace("Órgão Julgador:", "").Trim();
                else if (line.StartsWith("Valor da Causa:"))
                    valorCausa = line.Replace("Valor da Causa:", "").Trim();
                else if (line.StartsWith("Assuntos:"))
                {
                    var assuntosString = line.Replace("Assuntos:", "").Trim();
                    if (!string.IsNullOrEmpty(assuntosString))
                    {
                        if (assuntosString.Contains(","))
                        {
                            assuntos = assuntosString.Split(',').Select(a => a.Trim()).ToList();
                        }
                        else
                        {
                            assuntos.Add(assuntosString);
                        }
                    }
                }
                else if (line.StartsWith("Segredo de Justiça:"))
                    segredoDeJustica = line.Replace("Segredo de Justiça:", "").Trim();
                else if (line.Trim() == "Polos")
                    break; // Parar a leitura quando chegar aos Polos
            }

            // Crie um objeto JSON com os valores extraídos
            JObject textoInicialJson = new JObject(
                new JProperty("DataHoraConsulta", dataHoraConsulta),
                new JProperty("NumeroProcesso", numeroProcesso),
                new JProperty("Classe", classe),
                new JProperty("OrgaoJulgador", orgaoJulgador),
                new JProperty("ValorCausa", valorCausa),
                new JProperty("Assuntos", new JArray(assuntos)),
                new JProperty("SegredoDeJustica", segredoDeJustica.Equals("Sim") ? "SEGREDO DE JUSTIÇA" : "PÚBLICO")
            );

            // Escrever o objeto JSON para um arquivo
            File.WriteAllText(jsonFilePath, textoInicialJson.ToString());
        }



        public static Dictionary<string, List<string>> ExtractPolesFromTxt(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("No file found.");
                return null;
            }

            var lines = File.ReadAllLines(filePath).ToList();
            var poleSection = new Dictionary<string, List<string>>();
            bool isPoleSection = false;
            string currentPole = "";

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Se a linha contém a palavra "Polos", a seção de polos começa
                if (trimmedLine.Equals("Polos"))
                {
                    isPoleSection = true;
                    continue;
                }

                // Se a linha é vazia, a seção de polos termina
                if (isPoleSection && string.IsNullOrWhiteSpace(trimmedLine))
                {
                    isPoleSection = false;
                    continue;
                }

                // Se estamos na seção de polos, processar a linha
                if (isPoleSection)
                {
                    // Se a linha contém "Ativo" ou "Passivo", muda o polo atual
                    if (trimmedLine.Equals("Ativo") || trimmedLine.Equals("Passivo"))
                    {
                        currentPole = trimmedLine;
                        poleSection[currentPole] = new List<string>();
                    }
                    else
                    {
                        poleSection[currentPole].Add(trimmedLine);
                    }
                }
            }

            return poleSection;
        }

        public static JObject ConverterTextoPolosEmJson(string textoPolos)
        {
            string[] linhas = textoPolos.Split('\n');
            JArray poloAtivo = new JArray();
            JArray poloPassivo = new JArray();

            // Expressão regular para identificar as partes relevantes
            Regex regex = new Regex(@"(Ativo|Passivo)\s*,\s*((?:[^,;]+(?:\s*,\s*|$))+)", RegexOptions.IgnoreCase);

            foreach (string linha in linhas)
            {
                Match match = regex.Match(linha);

                if (match.Success)
                {
                    string tipoPolo = match.Groups[1].Value.Trim();
                    string nomes = match.Groups[2].Value.Trim();

                    // Remove vírgulas extras e espaços em branco ao redor dos nomes
                    string[] nomesSeparados = nomes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(x => x.Trim())
                                                   .Where(x => !string.IsNullOrEmpty(x))
                                                   .ToArray();

                    if (tipoPolo.Equals("Ativo", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (string nome in nomesSeparados)
                        {
                            poloAtivo.Add(nome);
                        }
                    }
                    else if (tipoPolo.Equals("Passivo", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (string nome in nomesSeparados)
                        {
                            poloPassivo.Add(nome);
                        }
                    }
                }
            }

            JObject polosJson = new JObject(
                new JProperty("poloAtivo", poloAtivo),
                new JProperty("poloPassivo", poloPassivo)
            );

            return polosJson;
        }


        public async Task WriteTimestampToFile(string caminhoBase, string nomeProcesso)
        {
            // Combina o caminho base e o nome do processo com a extensão ".ok"
            string filePath = Path.Combine(caminhoBase, nomeProcesso, nomeProcesso + "Cab.ok");

            // Obtém a hora atual como string
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Grava a data e hora atuais no arquivo
            await File.WriteAllTextAsync(filePath, currentDateTime);
        }

        public class CabecalhoProcesso
        {
            public string Numero { get; set; }
            public string Classe { get; set; }
            public string OrgaoJulgador { get; set; }
            public string ValorDaCausa { get; set; }
            public List<string> Assuntos { get; set; } = new List<string>();
            public string SegredoDeJustica { get; set; }
            public List<string> PoloAtivo { get; set; } = new List<string>();
            public List<string> PoloPassivo { get; set; } = new List<string>();
        }

        public class Documento
        {
            public string Id { get; set; }
            public string DataAssinatura { get; set; }
            public string TipoDocumento { get; set; }
            public string HoraAssinatura { get; set; }
            public string DescricaoDocumento { get; set; }
        }

        public class MovimentoProcessual
        {
            public string Id { get; set; }
            public DateTime DataAssinatura { get; set; }
            public string TipoDocumento { get; set; }
            public string DescricaoDocumento { get; set; }
        }

        public static class DocxTableToJsonExporter
        {
            public static string ExtractTextFromXml(string xml)
            {

                //Debug.WriteLine(xml);

                var xdoc = XDocument.Parse(xml);
                var ns = xdoc.Root.Name.Namespace;
                var textParts = new List<string>();

                foreach (var element in xdoc.Descendants())
                {
                    if (element.Name == ns + "t")
                    {
                        textParts.Add(element.Value);
                    }

                    if (element.Name == ns + "spacing")
                    {
                        var spacingVal = element.Attribute(ns + "val")?.Value;

                        if (spacingVal == "40")
                        {
                            textParts.Add("\n");
                        }
                        else if (spacingVal != null)
                        {
                            textParts.Add(" ");
                        }
                    }

                    if (element.Name == ns + "p")
                    {
                        textParts.Add("\n");
                    }
                }
                //Debug.WriteLine(xml);
                //Debug.WriteLine(string.Join(" ", textParts).Trim());

                return string.Join(" ", textParts).Trim();
            }

            public static void SaveTableToJson(List<List<List<string>>> table, string outputFile)
            {
                // Garante que o arquivo de saída tenha a extensão correta
                outputFile = Path.ChangeExtension(outputFile, ".json");
                var json = JsonConvert.SerializeObject(table, Formatting.Indented);
                File.WriteAllText(outputFile, json);
            }

        }
    }

}
