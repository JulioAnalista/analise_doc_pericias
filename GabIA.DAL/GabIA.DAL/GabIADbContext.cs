using GabIA.ENT;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.DAL
{
    public class GabIADbContext : DbContext
    {
        public GabIADbContext(DbContextOptions<GabIADbContext> options) : base(options)
        {
        }

        // Definição dos DbSets para cada entidade
        public DbSet<ProcessoENT> Processos { get; set; }
        public DbSet<ProcessoJudicialENT> ProcessosJudiciais { get; set; }
        public DbSet<ProcessoCompletoENT> ProcessosCompletos { get; set; }
        public DbSet<AtoProcessualENT> AtosProcessuais { get; set; }
        public DbSet<ElementosENT> Elementos { get; set; }
        public DbSet<PartesDoProcessoENT> PartesDoProcesso { get; set; }
        public DbSet<PessoaENT> Pessoas { get; set; }
        public DbSet<JuizesENT> Juizes { get; set; }
        public DbSet<MembrosMpENT> MembrosMP { get; set; }
        public DbSet<CausaDePedirENT> CausasDePedir { get; set; }
        public DbSet<PedidoENT> Pedidos { get; set; }
        public DbSet<TipoAtoProcessualENT> TiposAtoProcessual { get; set; }
        public DbSet<EnderecoENT> Enderecos { get; set; }
        public DbSet<PosicoesENT> Posicoes { get; set; }
        public DbSet<AdvogadoENT> Advogados { get; set; }
        public DbSet<TelefoneENT> Telefones { get; set; }
        public DbSet<RedeSocialENT> RedesSociais { get; set; }
        public DbSet<DocumentodeIdentificacaoENT> DocumentosIdentificacao { get; set; }
        public DbSet<RepresentanteENT> Representantes { get; set; }
        public DbSet<RevelENT> Reveis { get; set; }
        public DbSet<ProvasENT> Provas { get; set; }
        public DbSet<FaseENT> Fases { get; set; }
        public DbSet<ParticipanteAtoENT> ParticipantesAto { get; set; }
        public DbSet<TribunalENT> Tribunais { get; set; }
        public DbSet<OrgaoJurisdicionalENT> OrgaosJurisdicionais { get; set; }
        public DbSet<RitosENT> Ritos { get; set; }
        public DbSet<ProcessoAdministrativoENT> ProcessosAdministrativos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações específicas para cada entidade

            // ProcessoENT
            modelBuilder.Entity<ProcessoENT>()
                .ToTable("processo")
                .HasKey(p => p.id_processo);

            // ProcessoJudicialENT
            modelBuilder.Entity<ProcessoJudicialENT>()
                .ToTable("processo_judicial")
                .HasKey(p => p.Id_processo_judicial);

            modelBuilder.Entity<ProcessoJudicialENT>()
                .HasMany(p => p.AtosProcessuais)
                .WithOne()
                .HasForeignKey(a => a.IdProcesso);

            modelBuilder.Entity<ProcessoJudicialENT>()
                .HasMany(p => p.Elementos)
                .WithOne(e => e.ProcessoJudicial)
                .HasForeignKey(e => e.IdProcessoJudicial);

            modelBuilder.Entity<ProcessoJudicialENT>()
                .HasMany(p => p.Juizes)
                .WithOne(j => j.ProcessoJudicial)
                .HasForeignKey(j => j.IdProcessoJudicial);

            modelBuilder.Entity<ProcessoJudicialENT>()
                .HasMany(p => p.MembrosMP)
                .WithOne(m => m.ProcessoJudicial)
                .HasForeignKey(m => m.IdProcessoJudicial);

            // ProcessoCompletoENT
            modelBuilder.Entity<ProcessoCompletoENT>()
                .ToTable("processo_completo")
                .HasKey(p => p.IdProcessoCompleto);

            modelBuilder.Entity<ProcessoCompletoENT>()
                .HasMany(p => p.AtosProcessuais)
                .WithOne()
                .HasForeignKey(a => a.IdProcesso);

            modelBuilder.Entity<ProcessoCompletoENT>()
                .HasOne(p => p.ProcessoJudicial)
                .WithMany()
                .HasForeignKey(p => p.IdPJ);

            // AtoProcessualENT
            modelBuilder.Entity<AtoProcessualENT>()
                .ToTable("ato_processual")
                .HasKey(a => a.IdAtoProcessual);

            // ElementosENT
            modelBuilder.Entity<ElementosENT>()
                .ToTable("elementos")
                .HasKey(e => e.IdElemento);

            modelBuilder.Entity<ElementosENT>()
                .HasMany(e => e.PartesDoProcesso)
                .WithOne()
                .HasForeignKey(p => p.IdElemento);

            // Configuração para CausaDePedirENT
            modelBuilder.Entity<ElementosENT>()
                .HasMany(e => e.CausasDePedir)
                .WithOne(c => c.Elemento)
                .HasForeignKey(c => c.IdElemento);

            // Configuração para PedidoENT
            modelBuilder.Entity<ElementosENT>()
                .HasMany(e => e.Pedidos)
                .WithOne(p => p.Elemento)
                .HasForeignKey(p => p.IdElemento);

            // PartesDoProcessoENT
            modelBuilder.Entity<PartesDoProcessoENT>()
                .ToTable("partes_do_processo")
                .HasKey(p => p.IdPartesDoProcesso);

            modelBuilder.Entity<PartesDoProcessoENT>()
                .HasOne(p => p.Pessoa)
                .WithMany()
                .HasForeignKey(p => p.IdPessoa);

            // PessoaENT
            modelBuilder.Entity<PessoaENT>()
                .ToTable("pessoa")
                .HasKey(p => p.IdPessoa);

            // JuizesENT
            modelBuilder.Entity<JuizesENT>()
                .ToTable("juizes")
                .HasKey(j => j.IdJuiz);

            // MembrosMpENT
            modelBuilder.Entity<MembrosMpENT>()
                .ToTable("membros_mp")
                .HasKey(m => m.IdMembro);

            // CausaDePedirENT
            modelBuilder.Entity<CausaDePedirENT>()
                .ToTable("causa_de_pedir")
                .HasKey(c => c.IdCausa);

            // PedidoENT
            modelBuilder.Entity<PedidoENT>()
                .ToTable("pedido")
                .HasKey(p => p.IdPedido);

            // TipoAtoProcessualENT
            modelBuilder.Entity<TipoAtoProcessualENT>()
                .ToTable("tipo_ato_processual")
                .HasKey(t => t.Id_Tipo_Ato_processual);

            // EnderecoENT
            modelBuilder.Entity<EnderecoENT>()
                .ToTable("endereco")
                .HasKey(e => e.IdEndereco);

            // PosicoesENT
            modelBuilder.Entity<PosicoesENT>()
                .ToTable("posicoes")
                .HasKey(p => p.id_posicao);

            // AdvogadoENT
            modelBuilder.Entity<AdvogadoENT>()
                .ToTable("advogado")
                .HasKey(a => a.IdAdvogado);

            // TelefoneENT
            modelBuilder.Entity<TelefoneENT>()
                .ToTable("telefone")
                .HasKey(t => t.Id_telefone);

            // RedeSocialENT
            modelBuilder.Entity<RedeSocialENT>()
                .ToTable("rede_social")
                .HasKey(r => r.Id_rede);

            // DocumentodeIdentificacaoENT
            modelBuilder.Entity<DocumentodeIdentificacaoENT>()
                .ToTable("documento_identificacao")
                .HasKey(d => d.IdDocumento);

            // RepresentanteENT
            modelBuilder.Entity<RepresentanteENT>()
                .ToTable("representante")
                .HasKey(r => r.id_representante);

            // RevelENT
            modelBuilder.Entity<RevelENT>()
                .ToTable("revel")
                .HasKey(r => r.id_revel);

            // ProvasENT
            modelBuilder.Entity<ProvasENT>()
                .ToTable("provas")
                .HasKey(p => p.Id_prova);

            // FaseENT
            modelBuilder.Entity<FaseENT>()
                .ToTable("fase")
                .HasKey(f => f.IdFaseKey);

            // ParticipanteAtoENT
            modelBuilder.Entity<ParticipanteAtoENT>()
                .ToTable("participante_ato")
                .HasKey(p => p.IdParticipanteAto);

            // TribunalENT
            modelBuilder.Entity<TribunalENT>()
                .ToTable("tribunal")
                .HasKey(t => t.Id_tribunal);

            // OrgaoJurisdicionalENT
            modelBuilder.Entity<OrgaoJurisdicionalENT>()
                .ToTable("orgao_jurisdicional")
                .HasKey(o => o.idorgao);

            // RitosENT
            modelBuilder.Entity<RitosENT>()
                .ToTable("ritos")
                .HasKey(r => r.id_ritos);

            // ProcessoAdministrativoENT
            modelBuilder.Entity<ProcessoAdministrativoENT>()
                .ToTable("processo_administrativo")
                .HasKey(p => p.Id_proc_adm);
        }
    }
}
