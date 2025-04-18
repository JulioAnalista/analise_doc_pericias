using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.WPF
{
    public class RemoverTextoInutil
    {
        private static RemoverTextoInutil instance;
        private Dictionary<string, string> textosPermanentes;
        private Dictionary<string, string> textosEspecificos;

        public static RemoverTextoInutil Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RemoverTextoInutil();
                }
                return instance;
            }
        }

        private RemoverTextoInutil()
        {
            textosPermanentes = new Dictionary<string, string>();
            textosEspecificos = new Dictionary<string, string>();
        }

        public void CarregarTextosPermanentes(string arquivoTextoPermanente)
        {
            string[] linhas = File.ReadAllLines(arquivoTextoPermanente);
            foreach (string linha in linhas)
            {
                string[] partes = linha.Split('\'');
                if (linha != "" ) { textosPermanentes[partes[1]] = partes[3]; }
                
            }
        }

        public void AdicionarTextoEspecifico(string textoEspecifico, string textoSubstituto)
        {
            textosEspecificos[textoEspecifico] = textoSubstituto;
        }

        public string Remover_E_OU_Substituir_Textos(string texto)
        {
            // Divida a string de entrada em linhas
            List<string> linhas = new List<string>(texto.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));

            // Remover quebras de linha
            linhas = RemoverQuebrasDeLinha(linhas);

            foreach (var par in textosPermanentes)
            {
                linhas = linhas.Select(l => l.Replace(par.Key, par.Value, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            foreach (var par in textosEspecificos)
            {
                linhas = linhas.Select(l => l.Replace(par.Key, par.Value, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Junte as linhas de volta em uma única string
            return string.Join("\n", linhas);
        }


        public void Remover_E_OU_Substituir_Textos_old(string arquivoTexto, string arquivoTextoLimpo)
        {
            List<string> linhas = File.ReadAllLines(arquivoTexto).ToList();

            // Remover quebras de linha
            linhas = RemoverQuebrasDeLinha(linhas);

            foreach (var par in textosPermanentes)
            {
                linhas = linhas.Select(l => l.Replace(par.Key, par.Value)).ToList();
            }

            foreach (var par in textosEspecificos)
            {
                linhas = linhas.Select(l => l.Replace(par.Key, par.Value)).ToList();
            }

            File.WriteAllLines(arquivoTextoLimpo, linhas);
        }

        private List<string> RemoverQuebrasDeLinha(List<string> linhas)
        {
            List<string> novasLinhas = new List<string>();

            foreach (string linha in linhas)
            {
                string linhaSemQuebra = RemoverQuebrasDeLinhaInicio(linha);
                novasLinhas.Add(linhaSemQuebra);
            }

            return novasLinhas;
        }

        private string RemoverQuebrasDeLinhaInicio(string texto)
        {
            string textoLimpo = texto.TrimStart('\n');

            if (textoLimpo.Length > 0)
            {
                char ultimoCaractere = textoLimpo[textoLimpo.Length - 1];
                if (ultimoCaractere != '.' && ultimoCaractere != '!' && ultimoCaractere != '?' && ultimoCaractere != ':' && ultimoCaractere != ';')
                {
                    textoLimpo = textoLimpo.Replace("\n", " ");
                }
            }
            return textoLimpo;
        }
    }
}
