using LocacaoVeiculos.Core.Controllers;
using LocacaoVeiculos.Core.Models;

namespace LocacaoVeiculos.App.Views;

/// <summary>Tela 04 · Locação: aba "Nova locação" e aba "Consultar locações".</summary>
public sealed class LocacaoView : UserControl
{
    private const int LarguraResumo = 280;
    private const string TextoAtrasada = "Atrasada";
    private static readonly string[] OpcoesStatus = { "Todas", "Ativa", "Finalizada", "Cancelada" };

    private readonly LocacaoController _controller;
    private readonly Funcionario _funcionario;
    private readonly TabControl _abas = new() { Dock = DockStyle.Fill, Font = Tema.Texto };
    private readonly TabPage _abaConsulta = new("Consultar locações") { BackColor = Tema.Fundo, Padding = new Padding(Tema.Espaco) };

    // ---- Nova locação ----
    private readonly ComboBox _cbCliente = Tema.CriarLista(360);
    private readonly DateTimePicker _dtRetirada = new() { Format = DateTimePickerFormat.Short, Width = 130 };
    private readonly DateTimePicker _dtDevolucao = new() { Format = DateTimePickerFormat.Short, Width = 130 };
    private readonly CheckedListBox _listaVeiculos = new() { Dock = DockStyle.Fill, CheckOnClick = true, Font = Tema.Texto };
    private readonly ListBox _lstResumo = new() { Dock = DockStyle.Fill, Font = Tema.Texto, BorderStyle = BorderStyle.None };
    private readonly Label _lblDiarias = Tema.CriarRotulo(string.Empty);
    private readonly Label _lblTotal = Tema.CriarRotulo(string.Empty, Tema.Titulo);

    // ---- Consulta ----
    private readonly ComboBox _cbFiltroStatus = Tema.CriarLista(130);
    private readonly TextBox _txtFiltroCliente = Tema.CriarTexto(200);
    private readonly DateTimePicker _dtDe = new() { Format = DateTimePickerFormat.Short, Width = 140, ShowCheckBox = true, Checked = false };
    private readonly DateTimePicker _dtAte = new() { Format = DateTimePickerFormat.Short, Width = 140, ShowCheckBox = true, Checked = false };
    private readonly DataGridView _gradeLocacoes = Tema.CriarGrade();

    /// <summary>Item da lista de veículos disponíveis.</summary>
    private record OpcaoVeiculo(int Id, string Texto)
    {
        public override string ToString() => Texto;
    }

    private record LinhaLocacao(int Id, string Numero, string Cliente, int Veiculos,
        string Retirada, string DevolucaoPrevista, string Status, string Total);

    public LocacaoView(LocacaoController controller, Funcionario funcionario)
    {
        _controller = controller;
        _funcionario = funcionario;

        _abas.TabPages.Add(CriarAbaNovaLocacao());
        _abas.TabPages.Add(CriarAbaConsulta());
        Controls.Add(_abas);

        CarregarNovaLocacao();
        Buscar();
    }

    // =================== ABA NOVA LOCAÇÃO ===================

    private TabPage CriarAbaNovaLocacao()
    {
        _cbCliente.DisplayMember = nameof(Cliente.Descricao);
        _dtRetirada.ValueChanged += (_, _) => AtualizarResumo();
        _dtDevolucao.ValueChanged += (_, _) => AtualizarResumo();
        // ItemCheck dispara ANTES de marcar: atualiza o resumo logo depois
        _listaVeiculos.ItemCheck += (_, _) => BeginInvoke(new Action(AtualizarResumo));

        var cabecalho = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        cabecalho.Controls.Add(Tema.Campo("1. Cliente (já cadastrado no banco)", _cbCliente));
        cabecalho.Controls.Add(Tema.Linha(Tema.Campo("2. Retirada", _dtRetirada), Tema.Campo("Devolução prevista", _dtDevolucao)));
        cabecalho.Controls.Add(Tema.CriarRotulo("3. Veículos disponíveis (marque um ou mais)", Tema.Subtitulo));

        var selecao = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, Tema.Espaco, 0) };
        selecao.Controls.Add(_listaVeiculos);
        selecao.Controls.Add(cabecalho);

        var aba = new TabPage("Nova locação") { BackColor = Tema.Fundo, Padding = new Padding(Tema.Espaco) };
        aba.Controls.Add(selecao);
        aba.Controls.Add(CriarPainelResumo());
        return aba;
    }

    private Control CriarPainelResumo()
    {
        var botaoConfirmar = Tema.CriarBotao("Confirmar locação", Tema.Sucesso);
        botaoConfirmar.Dock = DockStyle.Bottom;
        botaoConfirmar.AutoSize = false;
        botaoConfirmar.Height = 44;
        botaoConfirmar.Click += (_, _) => ConfirmarLocacao();

        var titulo = Tema.CriarRotulo("Resumo", Tema.Subtitulo);
        titulo.Dock = DockStyle.Top;
        _lblDiarias.Dock = DockStyle.Top;
        _lblTotal.Dock = DockStyle.Bottom;

        var painel = new Panel
        {
            Dock = DockStyle.Right,
            Width = LarguraResumo,
            BackColor = Tema.AvisoClaro,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(Tema.Espaco)
        };
        _lstResumo.BackColor = Tema.AvisoClaro;
        painel.Controls.Add(_lstResumo);
        painel.Controls.Add(_lblDiarias);
        painel.Controls.Add(titulo);
        painel.Controls.Add(_lblTotal);
        painel.Controls.Add(botaoConfirmar);
        return painel;
    }

    private void CarregarNovaLocacao()
    {
        _cbCliente.DataSource = _controller.ListarClientes().ToList();
        _cbCliente.SelectedIndex = -1;

        _dtRetirada.Value = DateTime.Today;
        _dtDevolucao.Value = DateTime.Today.AddDays(1);

        _listaVeiculos.Items.Clear();
        foreach (var veiculo in _controller.ListarVeiculosDisponiveis())
        {
            var texto = $"{veiculo.Placa} · {veiculo.Marca} {veiculo.Modelo} ({veiculo.Tipo}) · {Tema.Moeda(veiculo.CalcularDiaria())}/dia";
            _listaVeiculos.Items.Add(new OpcaoVeiculo(veiculo.Id, texto));
        }

        AtualizarResumo();
    }

    private List<int> VeiculosMarcados() =>
        _listaVeiculos.CheckedItems.Cast<OpcaoVeiculo>().Select(o => o.Id).ToList();

    private void AtualizarResumo()
    {
        var orcamento = _controller.CalcularOrcamento(VeiculosMarcados(), _dtRetirada.Value, _dtDevolucao.Value);

        _lstResumo.Items.Clear();
        foreach (var item in orcamento.Itens)
            _lstResumo.Items.Add($"{item.Descricao}: {Tema.Moeda(item.Subtotal)}");

        _lblDiarias.Text = $"Diárias: {orcamento.Diarias}";
        _lblTotal.Text = $"Total {Tema.Moeda(orcamento.Total)}";
    }

    private void ConfirmarLocacao()
    {
        var cliente = _cbCliente.SelectedItem as Cliente;
        var resultado = _controller.Registrar(
            clienteId: cliente?.Id ?? 0,
            funcionarioId: _funcionario.Id,
            retirada: _dtRetirada.Value,
            devolucao: _dtDevolucao.Value,
            veiculoIds: VeiculosMarcados());

        Tema.Mostrar(resultado);
        if (!resultado.Sucesso)
            return;

        CarregarNovaLocacao();
        Buscar();
        _abas.SelectedTab = _abaConsulta;
    }

    // =================== ABA CONSULTA ===================

    private TabPage CriarAbaConsulta()
    {
        _cbFiltroStatus.Items.AddRange(OpcoesStatus);
        _cbFiltroStatus.SelectedIndex = 0;
        _gradeLocacoes.DataBindingComplete += (_, _) => DestacarAtrasadas();

        var botaoBuscar = Tema.CriarBotao("Buscar", Tema.Primaria);
        botaoBuscar.Click += (_, _) => Buscar();
        var botaoFinalizar = Tema.CriarBotao("Finalizar locação", Tema.Sucesso);
        botaoFinalizar.Click += (_, _) => EncerrarSelecionada(finalizar: true);
        var botaoCancelar = Tema.CriarBotao("Cancelar locação", Tema.Perigo);
        botaoCancelar.Click += (_, _) => EncerrarSelecionada(finalizar: false);

        var filtros = Tema.Linha(
            Tema.Campo("Status", _cbFiltroStatus),
            Tema.Campo("Cliente", _txtFiltroCliente),
            Tema.Campo("Retirada de", _dtDe),
            Tema.Campo("até", _dtAte),
            botaoBuscar);
        filtros.Dock = DockStyle.Top;

        var acoes = Tema.Linha(botaoFinalizar, botaoCancelar);
        acoes.Dock = DockStyle.Bottom;

        _abaConsulta.Controls.Add(_gradeLocacoes);
        _abaConsulta.Controls.Add(filtros);
        _abaConsulta.Controls.Add(acoes);
        return _abaConsulta;
    }

    private void Buscar()
    {
        StatusLocacao? status = _cbFiltroStatus.SelectedIndex switch
        {
            1 => StatusLocacao.Ativa,
            2 => StatusLocacao.Finalizada,
            3 => StatusLocacao.Cancelada,
            _ => null
        };

        var filtro = new FiltroLocacao(
            status,
            _txtFiltroCliente.Text,
            _dtDe.Checked ? _dtDe.Value : null,
            _dtAte.Checked ? _dtAte.Value : null);

        var hoje = DateTime.Today;
        _gradeLocacoes.DataSource = _controller.Consultar(filtro)
            .Select(l => new LinhaLocacao(
                l.Id,
                $"#{l.Id:0000}",
                l.Cliente,
                l.QuantidadeVeiculos,
                Tema.Data(l.DataRetirada),
                Tema.Data(l.DataPrevistaDevolucao),
                l.EstaAtrasada(hoje) ? TextoAtrasada : l.Status.ToString(),
                Tema.Moeda(l.ValorTotal)))
            .ToList();
    }

    private void DestacarAtrasadas()
    {
        foreach (DataGridViewRow linha in _gradeLocacoes.Rows)
        {
            if (linha.DataBoundItem is LinhaLocacao { Status: TextoAtrasada })
                linha.DefaultCellStyle.BackColor = Tema.PerigoClaro;
        }
    }

    private void EncerrarSelecionada(bool finalizar)
    {
        var linha = Tema.ItemSelecionado<LinhaLocacao>(_gradeLocacoes);
        if (linha is null)
        {
            Tema.Mostrar(ResultadoOperacao.Falha("Selecione uma locação na lista."));
            return;
        }

        var acao = finalizar ? "Finalizar" : "Cancelar";
        if (!Tema.Confirmar($"{acao} a locação {linha.Numero}?"))
            return;

        var resultado = finalizar ? _controller.Finalizar(linha.Id) : _controller.Cancelar(linha.Id);
        Tema.Mostrar(resultado);

        if (!resultado.Sucesso)
            return;

        Buscar();
        CarregarNovaLocacao(); // veículos liberados voltam para a lista de disponíveis
    }
}
