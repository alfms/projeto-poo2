using LocacaoVeiculos.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LocacaoVeiculos.Data;

/// <summary>
/// Contexto do EF Core. O banco é gerado a partir destas classes (Code First)
/// com: dotnet ef migrations add Inicial (ver README).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Credencial> Credenciais => Set<Credencial>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Locacao> Locacoes => Set<Locacao>();
    public DbSet<LocacaoVeiculo> LocacaoVeiculos => Set<LocacaoVeiculo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurarClientes(modelBuilder);
        ConfigurarFuncionarios(modelBuilder);
        ConfigurarFrota(modelBuilder);
        ConfigurarLocacoes(modelBuilder);

        DadosIniciais.Aplicar(modelBuilder);
    }

    // Herança TPH: PF e PJ na mesma tabela, diferenciadas pela coluna TipoCliente
    private static void ConfigurarClientes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(cliente =>
        {
            cliente.ToTable("Clientes");
            cliente.Property(c => c.Nome).HasMaxLength(120).IsRequired();
            cliente.Ignore(c => c.Documento);
            cliente.Ignore(c => c.Descricao);
            cliente.HasDiscriminator<string>("TipoCliente")
                .HasValue<PessoaFisica>("PF")
                .HasValue<PessoaJuridica>("PJ");
        });
    }

    private static void ConfigurarFuncionarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Funcionario>().ToTable("Funcionarios");

        // 1x1: Funcionario ↔ Credencial (a credencial morre junto com o funcionário)
        modelBuilder.Entity<Credencial>(credencial =>
        {
            credencial.ToTable("Credenciais");
            credencial.HasIndex(c => c.Login).IsUnique();
            credencial.HasOne(c => c.Funcionario)
                .WithOne(f => f.Credencial)
                .HasForeignKey<Credencial>(c => c.FuncionarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurarFrota(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(categoria =>
        {
            categoria.ToTable("Categorias");
            categoria.Property(c => c.Nome).HasMaxLength(60).IsRequired();
            categoria.Property(c => c.ValorDiariaBase).HasPrecision(10, 2);
        });

        // Herança TPH: Carro e Moto na tabela Veiculos, coluna TipoVeiculo
        modelBuilder.Entity<Veiculo>(veiculo =>
        {
            veiculo.ToTable("Veiculos");
            veiculo.HasIndex(v => v.Placa).IsUnique();
            veiculo.Property(v => v.Placa).HasMaxLength(7).IsRequired();
            veiculo.Ignore(v => v.Tipo);
            veiculo.HasDiscriminator<string>("TipoVeiculo")
                .HasValue<Carro>("Carro")
                .HasValue<Moto>("Moto");

            // Agregação 1xN: categoria não apaga veículos
            veiculo.HasOne(v => v.Categoria)
                .WithMany(c => c.Veiculos)
                .HasForeignKey(v => v.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarLocacoes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Locacao>(locacao =>
        {
            locacao.ToTable("Locacoes");
            locacao.Property(l => l.ValorTotal).HasPrecision(10, 2);

            locacao.HasOne(l => l.Cliente)
                .WithMany(c => c.Locacoes)
                .HasForeignKey(l => l.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            locacao.HasOne(l => l.Funcionario)
                .WithMany(f => f.Locacoes)
                .HasForeignKey(l => l.FuncionarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // NxN Locacao ↔ Veiculo com classe associativa
        modelBuilder.Entity<LocacaoVeiculo>(item =>
        {
            item.ToTable("LocacaoVeiculos");
            item.Property(i => i.ValorDiaria).HasPrecision(10, 2);

            item.HasOne(i => i.Locacao)
                .WithMany(l => l.Itens)
                .HasForeignKey(i => i.LocacaoId)
                .OnDelete(DeleteBehavior.Cascade); // composição

            item.HasOne(i => i.Veiculo)
                .WithMany(v => v.Locacoes)
                .HasForeignKey(i => i.VeiculoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
