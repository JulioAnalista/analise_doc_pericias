using GabIA.ENT;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.DAL
{
    public static class DbInitializer
    {
        public static void Initialize(GabIADbContext context)
        {
            // Garante que o banco de dados seja criado
            context.Database.EnsureCreated();

            // Verifica se já existem dados no banco
            if (context.Posicoes.Any())
            {
                return; // O banco de dados já foi inicializado
            }

            // Adiciona dados iniciais

            // Posições
            var posicoes = new PosicoesENT[]
            {
                new PosicoesENT { posicao = "ativo" },
                new PosicoesENT { posicao = "passivo" },
                new PosicoesENT { posicao = "terceiro interessado" },
                new PosicoesENT { posicao = "assistente" },
                new PosicoesENT { posicao = "amicus curiae" }
            };

            foreach (var posicao in posicoes)
            {
                context.Posicoes.Add(posicao);
            }
            context.SaveChanges();

            // Tipos de Ato Processual
            var tiposAto = new TipoAtoProcessualENT[]
            {
                new TipoAtoProcessualENT { Tipo = "inicial", Descricao = "Petição Inicial" },
                new TipoAtoProcessualENT { Tipo = "contestação", Descricao = "Contestação" },
                new TipoAtoProcessualENT { Tipo = "réplica", Descricao = "Réplica" },
                new TipoAtoProcessualENT { Tipo = "decisão", Descricao = "Decisão" },
                new TipoAtoProcessualENT { Tipo = "despacho", Descricao = "Despacho" },
                new TipoAtoProcessualENT { Tipo = "sentença", Descricao = "Sentença" },
                new TipoAtoProcessualENT { Tipo = "acórdão", Descricao = "Acórdão" },
                new TipoAtoProcessualENT { Tipo = "recurso", Descricao = "Recurso" },
                new TipoAtoProcessualENT { Tipo = "parecer", Descricao = "Parecer" },
                new TipoAtoProcessualENT { Tipo = "alegações", Descricao = "Alegações Finais" },
                new TipoAtoProcessualENT { Tipo = "emenda", Descricao = "Emenda à Inicial" },
                new TipoAtoProcessualENT { Tipo = "diligência", Descricao = "Diligência" },
                new TipoAtoProcessualENT { Tipo = "mandado", Descricao = "Mandado" },
                new TipoAtoProcessualENT { Tipo = "saneamento", Descricao = "Despacho Saneador" }
            };

            foreach (var tipo in tiposAto)
            {
                context.TiposAtoProcessual.Add(tipo);
            }
            context.SaveChanges();
        }
    }
}
