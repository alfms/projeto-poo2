using System.Globalization;
using LocacaoVeiculos.App.Views;
using LocacaoVeiculos.Core.Controllers;
using LocacaoVeiculos.Data;
using LocacaoVeiculos.Data.Repositories;

namespace LocacaoVeiculos.App;

/// <summary>Controllers que as telas usam, montados uma única vez no Program.</summary>
public record ControllersDaAplicacao(
    DashboardController Dashboard,
    VeiculoController Veiculos,
    CategoriaController Categorias,
    LocacaoController Locacoes);

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var portugues = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = portugues;
        CultureInfo.DefaultThreadCurrentUICulture = portugues;
        ApplicationConfiguration.Initialize();

        using var contexto = BancoDeDados.CriarContexto();
        try
        {
            BancoDeDados.AplicarMigrations(contexto);
        }
        catch (Exception erro)
        {
            MessageBox.Show(erro.Message, "Banco de dados", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Composition root: o ÚNICO lugar que conhece as classes concretas de repositório (DIP).
        var veiculos = new VeiculoRepository(contexto);
        var categorias = new CategoriaRepository(contexto);
        var locacoes = new LocacaoSqlRepository(contexto);
        var clientes = new ClienteRepository(contexto);
        var funcionarios = new FuncionarioRepository(contexto);

        var controllers = new ControllersDaAplicacao(
            Dashboard: new DashboardController(veiculos, locacoes),
            Veiculos: new VeiculoController(veiculos, categorias),
            Categorias: new CategoriaController(categorias),
            Locacoes: new LocacaoController(locacoes, clientes, veiculos));

        using var login = new LoginForm(new AuthController(funcionarios));
        if (login.ShowDialog() != DialogResult.OK || login.FuncionarioLogado is null)
            return;

        Application.Run(new MainForm(login.FuncionarioLogado, controllers));
    }
}
