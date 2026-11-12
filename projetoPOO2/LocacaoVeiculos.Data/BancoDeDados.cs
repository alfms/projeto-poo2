using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LocacaoVeiculos.Data;

/// <summary>Ponto único de criação do contexto e aplicação das migrations.</summary>
public static class BancoDeDados
{
    public const string StringConexao = "Data Source=locacao.db";

    public static AppDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(StringConexao)
            .Options;

        return new AppDbContext(opcoes);
    }

    /// <summary>Cria/atualiza o banco aplicando as migrations (Code First).</summary>
    public static void AplicarMigrations(AppDbContext contexto)
    {
        if (!contexto.Database.GetMigrations().Any())
            throw new InvalidOperationException(
                "Nenhuma migration encontrada. Rode: dotnet ef migrations add Inicial --project LocacaoVeiculos.Data");

        contexto.Database.Migrate();
    }
}

/// <summary>
/// Usado só pelo comando "dotnet ef" para gerar migrations sem depender do projeto WinForms
/// (funciona no Windows e no Mac).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) => BancoDeDados.CriarContexto();
}
