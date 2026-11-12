using LocacaoVeiculos.Core.Controllers;
using LocacaoVeiculos.Core.Models;

namespace LocacaoVeiculos.App.Views;

/// <summary>Tela 02 · Dashboard com os números do dia.</summary>
public sealed class DashboardView : UserControl
{
    private const int AlturaCards = 120;
    private const int QuantidadeCards = 4;

    private readonly DashboardController _controller;
    private readonly Label _lblDisponiveis = CriarNumero();
    private readonly Label _lblAlugados = CriarNumero();
    private readonly Label _lblAtivas = CriarNumero();
    private readonly Label _lblAtrasadas = CriarNumero();
    private readonly DataGridView _gradeDevolucoes = Tema.CriarGrade();

    public DashboardView(DashboardController controller, Funcionario funcionario)
    {
        _controller = controller;
        MontarLayout(funcionario);
        Atualizar();
    }

    private void MontarLayout(Funcionario funcionario)
    {
        BackColor = Tema.Fundo;

        var cabecalho = Tema.CriarRotulo($"Olá, {funcionario.Nome} · {Tema.Data(DateTime.Today)}", Tema.Titulo);
        cabecalho.Dock = DockStyle.Top;

        var cards = new TableLayoutPanel { Dock = DockStyle.Top, Height = AlturaCards, ColumnCount = QuantidadeCards, RowCount = 1 };
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (var coluna = 0; coluna < QuantidadeCards; coluna++)
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / QuantidadeCards));

        cards.Controls.Add(CriarCard("Veículos disponíveis", _lblDisponiveis, Tema.SucessoClaro), 0, 0);
        cards.Controls.Add(CriarCard("Veículos alugados", _lblAlugados, Tema.PrimariaClara), 1, 0);
        cards.Controls.Add(CriarCard("Locações ativas", _lblAtivas, Tema.Superficie), 2, 0);
        cards.Controls.Add(CriarCard("Devoluções atrasadas", _lblAtrasadas, Tema.PerigoClaro), 3, 0);

        var tituloGrade = Tema.CriarRotulo("Devoluções previstas para hoje", Tema.Subtitulo);
        tituloGrade.Dock = DockStyle.Top;
        tituloGrade.Padding = new Padding(0, Tema.Espaco, 0, Tema.Espaco / 2);

        // Dock: o último adicionado fica mais em cima
        Controls.Add(_gradeDevolucoes);
        Controls.Add(tituloGrade);
        Controls.Add(cards);
        Controls.Add(cabecalho);
    }

    private void Atualizar()
    {
        var resumo = _controller.ObterResumo(DateTime.Today);

        _lblDisponiveis.Text = resumo.VeiculosDisponiveis.ToString();
        _lblAlugados.Text = resumo.VeiculosAlugados.ToString();
        _lblAtivas.Text = resumo.LocacoesAtivas.ToString();
        _lblAtrasadas.Text = resumo.DevolucoesAtrasadas.ToString();

        _gradeDevolucoes.DataSource = resumo.DevolucoesHoje
            .Select(l => new { Locação = $"#{l.Id}", l.Cliente, Veículos = l.QuantidadeVeiculos, Total = Tema.Moeda(l.ValorTotal) })
            .ToList();
    }

    private static Label CriarNumero() => new()
    {
        Text = "0",
        AutoSize = true,
        Font = Tema.NumeroGrande
    };

    private static Control CriarCard(string titulo, Label valor, Color corDeFundo)
    {
        var card = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            BackColor = corDeFundo,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(Tema.Espaco),
            Margin = new Padding(0, 0, Tema.Espaco, Tema.Espaco)
        };
        card.Controls.Add(Tema.CriarRotulo(titulo));
        card.Controls.Add(valor);
        return card;
    }
}
