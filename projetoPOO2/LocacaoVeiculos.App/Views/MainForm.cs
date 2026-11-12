using LocacaoVeiculos.Core.Models;

namespace LocacaoVeiculos.App.Views;

/// <summary>Janela principal: menu lateral + área onde cada tela (UserControl) é exibida.</summary>
public sealed class MainForm : Form
{
    private const int LarguraMenu = 190;
    private const int LarguraItemMenu = 165;
    private const int AlturaItemMenu = 38;

    private readonly Funcionario _funcionario;
    private readonly ControllersDaAplicacao _controllers;
    private readonly Panel _conteudo = new() { Dock = DockStyle.Fill, Padding = new Padding(Tema.Espaco), BackColor = Tema.Fundo };
    private readonly List<Button> _itensMenu = new();

    public MainForm(Funcionario funcionario, ControllersDaAplicacao controllers)
    {
        _funcionario = funcionario;
        _controllers = controllers;
        MontarLayout();
        AbrirDashboard();
        Destacar(_itensMenu[0]);
    }

    private void MontarLayout()
    {
        Text = $"Sistema de Locação · {_funcionario.Nome} ({_funcionario.Perfil})";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 700);
        BackColor = Tema.Fundo;

        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = LarguraMenu,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(Tema.Espaco),
            BackColor = Tema.Superficie
        };
        menu.Controls.Add(Tema.CriarRotulo(_funcionario.Nome, Tema.Subtitulo));
        menu.Controls.Add(CriarItemMenu("Dashboard", AbrirDashboard));
        menu.Controls.Add(CriarItemMenu("Veículos", AbrirVeiculos));
        menu.Controls.Add(CriarItemMenu("Locação", AbrirLocacao));
        menu.Controls.Add(CriarItemMenu("Sair", Close));

        Controls.Add(_conteudo);
        Controls.Add(menu);
        _conteudo.BringToFront(); // garante que o conteúdo ocupa o espaço que sobra ao lado do menu
    }

    private Button CriarItemMenu(string texto, Action acao)
    {
        var item = Tema.CriarBotao(texto);
        item.AutoSize = false;
        item.Size = new Size(LarguraItemMenu, AlturaItemMenu);
        item.TextAlign = ContentAlignment.MiddleLeft;
        item.Click += (_, _) =>
        {
            Destacar(item);
            acao();
        };
        _itensMenu.Add(item);
        return item;
    }

    private void Destacar(Button selecionado)
    {
        foreach (var item in _itensMenu)
            item.BackColor = item == selecionado ? Tema.PrimariaClara : Tema.Superficie;
    }

    private void AbrirDashboard() => Exibir(new DashboardView(_controllers.Dashboard, _funcionario));

    private void AbrirVeiculos() => Exibir(new VeiculosView(_controllers.Veiculos, _controllers.Categorias));

    private void AbrirLocacao() => Exibir(new LocacaoView(_controllers.Locacoes, _funcionario));

    private void Exibir(UserControl tela)
    {
        var anteriores = _conteudo.Controls.Cast<Control>().ToArray();
        _conteudo.Controls.Clear();
        foreach (var anterior in anteriores)
            anterior.Dispose();

        tela.Dock = DockStyle.Fill;
        _conteudo.Controls.Add(tela);
    }
}
