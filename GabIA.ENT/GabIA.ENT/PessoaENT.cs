using System;

namespace GabIA.ENT
{
    public class PessoaENT
    {
        public int IdPessoa { get; set; }  // Mudan�a para manter a consist�ncia de nomenclatura
        public string Nome { get; set; }
        public string Iniciais { get; set; }

        private string _genero;
        public string Genero
        {
            get { return _genero; }
            set
            {
                // Exemplo de valida��o simples
                if (value == "Masculino" || value == "Feminino" || value == "Outro")
                {
                    _genero = value;
                }
                else
                {
                    throw new ArgumentException("G�nero inv�lido");
                }
            }
        }

        public DateTime DataNascimento { get; set; } // Renomeado para melhor clareza
        public int Tipo { get; set; } // Considere usar um enum se houver um conjunto fixo de tipos

        // Representa��o e Procurador podem ser tipos complexos ou IDs que referenciam outras entidades
        public object Representacao { get; set; }
        public object Procurador { get; set; }

        // Construtor, m�todos adicionais, etc.
    }
}
