using System;

namespace GabIA.ENT
{
    public class PessoaENT
    {
        public int IdPessoa { get; set; }  // Mudança para manter a consistência de nomenclatura
        public string Nome { get; set; }
        public string Iniciais { get; set; }

        private string _genero;
        public string Genero
        {
            get { return _genero; }
            set
            {
                // Exemplo de validação simples
                if (value == "Masculino" || value == "Feminino" || value == "Outro")
                {
                    _genero = value;
                }
                else
                {
                    throw new ArgumentException("Gênero inválido");
                }
            }
        }

        public DateTime DataNascimento { get; set; } // Renomeado para melhor clareza
        public int Tipo { get; set; } // Considere usar um enum se houver um conjunto fixo de tipos

        // Representação e Procurador podem ser tipos complexos ou IDs que referenciam outras entidades
        public object Representacao { get; set; }
        public object Procurador { get; set; }

        // Construtor, métodos adicionais, etc.
    }
}
