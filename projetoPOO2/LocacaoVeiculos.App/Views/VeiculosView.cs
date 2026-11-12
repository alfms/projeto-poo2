using LocacaoVeiculos.Core.Controllers;
using LocacaoVeiculos.Core.Models;

namespace LocacaoVeiculos.App.Views;

/// <summary>Tela 03 · Veículos e Categorias (duas abas).</summary>
public sealed class VeiculosView : UserControl
{
    private const int NovoRegistro = 0;
    private static readonly string[] OpcoesStatus = { "Todos", "Disponível", "Alugado" };

    private readonly VeiculoController _veiculoController;
    private readonly CategoriaController _categoriaController;

    // ---- Aba Veículos ----
    private readonly TextBox _txtBusca = Tema.CriarTexto(180);
    private readonly ComboBox _cbFiltroStatus = Tema.CriarLista(130);
    private readonly DataGridView _gradeVeiculos = Tema.CriarGrade();
    private readonly RadioButton _rbCarro = new() { Text = "Carro", Checked = true, AutoSize = true };
    private readonly RadioButton _rbMoto = new() { Text = "Moto", AutoSize = true };
    private readonly TextBox _txtPlaca = Tema.CriarTexto(100);
    private readonly TextBox _txtMarca = Tema.CriarTexto(150);
    private readonly TextBox _txtModelo = Tema.CriarTexto(150);
    private readonly NumericUpDown _numAno = Tema.CriarNumero(1990, 2100);
    private readonly NumericUpDown _numKm = Tema.CriarNumero(0, 2_000_000);
    private readonly ComboBox _cbCategoria = Tema.CriarLista();
    private readonly NumericUpDown _numPortas = Tema.CriarNumero(2, 5);
    private readonly CheckBox _chkAutomatico = new() { Text = "Câmbio automático", AutoSize = true };
    private readonly NumericUpDown _numCilindradas = Tema.CriarNumero(50, 2000);
    private readonly FlowLayoutPanel _painelCarro = Tema.Linha();
    private readonly FlowLayoutPanel _painelMoto = Tema.Linha();
    private readonly Label _lblDiaria = Tema.CriarRotulo(string.Empty, Tema.Subtitulo);
    private IReadOnlyList<Veiculo> _veiculos = Array.Empty<Veiculo>();
    private int _veiculoEmEdicaoId = NovoRegistro;

    // ---- Aba Categorias ----
    private readonly TextBox _txtCategoriaNome = Tema.CriarTexto();
    private readonly NumericUpDown _numCategoriaDiaria = Tema.CriarNumero(0, 10_000, casasDecimais: 2);
    private readonly DataGridView _gradeCategorias = Tema.CriarGrade();
    private int _categoriaEmEdicaoId = NovoRegistro;

    private record LinhaVeiculo(int Id, string Placa, string Modelo, string Tipo, string Categoria, string Status);
    private record LinhaCategoria(int Id, string Nome, string DiariaBase);

    public VeiculosView(VeiculoController veiculoController, CategoriaController categoriaController)
    {
        _veiculoController = veiculoController;
        _categoriaController = categoriaController;

        var abas = new TabControl { Dock = DockStyle.Fill, Font = Tema.Texto };
        abas.TabPages.Add(CriarAbaVeiculos());
        abas.TabPages.Add(CriarAbaCategorias());
        Controls.Add(abas);

        CarregarCategorias();
        CarregarVeiculos();
        LimparFormularioVeiculo();
    }

    // =================== ABA VEÍCULOS ===================

    private TabPage CriarAbaVeiculos()
    {
        _cbFiltroStatus.Items.AddRange(OpcoesStatus);
        _cbFiltroStatus.SelectedIndex = 0;
        _txtBusca.TextChanged += (_, _) => CarregarVeiculos();
        _cbFiltroStatus.SelectedIndexChanged += (_, _) => CarregarVeiculos();
        _gradeVeiculos.CellDoubleClick += (_, _) => EditarVeiculoSelecionado();

        var botaoEditar = Tema.CriarBotao("Editar selecionado");
        botaoEditar.Click += (_, _) => EditarVeiculoSelecionado();

        var lista = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, Tema.Espaco, 0) };
        var filtros = Tema.Linha(Tema.Campo("Buscar (placa, marca, modelo)", _txtBusca), Tema.Campo("Status", _cbFiltroStatus));
        filtros.Dock = DockStyle.Top;
        var acoes = Tema.Linha(botaoEditar);
        acoes.Dock = DockStyle.Bottom;
        lista.Controls.Add(_gradeVeiculos);
        lista.Controls.Add(filtros);
        lista.Controls.Add(acoes);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(Tema.Espaco) };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        layout.Controls.Add(lista, 0, 0);
        layout.Controls.Add(CriarFormularioVeiculo(), 1, 0);

        var aba = new TabPage("Veículos") { BackColor = Tema.Fundo };
        aba.Controls.Add(layout);
        return aba;
    }

    private Control CriarFormularioVeiculo()
    {
        _rbCarro.CheckedChanged += (_, _) => AlternarCamposPorTipo();
        _cbCategoria.SelectedIndexChanged += (_, _) => AtualizarDiaria();
        _chkAutomatico.CheckedChanged += (_, _) => AtualizarDiaria();
        _numCilindradas.ValueChanged += (_, _) => AtualizarDiaria();

        _painelCarro.Controls.Add(Tema.Campo("Portas", _numPortas));
        _painelCarro.Controls.Add(_chkAutomatico);
        _painelMoto.Controls.Add(Tema.Campo("Cilindradas", _numCilindradas));

        var botaoSalvar = Tema.CriarBotao("Salvar", Tema.Sucesso);
        botaoSalvar.Click += (_, _) => SalvarVeiculo();
        var botaoNovo = Tema.CriarBotao("Novo / Limpar");
        botaoNovo.Click += (_, _) => LimparFormularioVeiculo();
        var botaoExcluir = Tema.CriarBotao("Excluir", Tema.Perigo);
        botaoExcluir.Click += (_, _) => ExcluirVeiculo();

        var campos = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };
        campos.Controls.Add(Tema.Linha(Tema.CriarRotulo("Tipo:"), _rbCarro, _rbMoto));
        campos.Controls.Add(Tema.Linha(Tema.Campo("Placa", _txtPlaca), Tema.Campo("Marca", _txtMarca), Tema.Campo("Modelo", _txtModelo)));
        campos.Controls.Add(Tema.Linha(Tema.Campo("Ano", _numAno), Tema.Campo("Km", _numKm)));
        campos.Controls.Add(Tema.Campo("Categoria", _cbCategoria));
        campos.Controls.Add(_painelCarro);
        campos.Controls.Add(_painelMoto);
        campos.Controls.Add(_lblDiaria);
        campos.Controls.Add(Tema.Linha(botaoSalvar, botaoNovo, botaoExcluir));

        var grupo = new GroupBox { Text = "Cadastro de veículo", Dock = DockStyle.Fill, Font = Tema.Texto, Padding = new Padding(Tema.Espaco) };
        grupo.Controls.Add(campos);
        return grupo;
    }

    private void CarregarVeiculos()
    {
        StatusVeiculo? status = _cbFiltroStatus.SelectedIndex switch
        {
            1 => StatusVeiculo.Disponivel,
            2 => StatusVeiculo.Alugado,
            _ => null
        };

        _veiculos = _veiculoController.Listar(_txtBusca.Text, status);
        _gradeVeiculos.DataSource = _veiculos
            .Select(v => new LinhaVeiculo(v.Id, v.Placa, $"{v.Marca} {v.Modelo}", v.Tipo,
                v.Categoria?.Nome ?? "-", v.EstaDisponivel() ? "Disponível" : "Alugado"))
            .ToList();
    }

    private void EditarVeiculoSelecionado()
    {
        var linha = Tema.ItemSelecionado<LinhaVeiculo>(_gradeVeiculos);
        var veiculo = _veiculos.FirstOrDefault(v => v.Id == linha?.Id);
        if (veiculo is null)
            return;

        _veiculoEmEdicaoId = veiculo.Id;
        _txtPlaca.Text = veiculo.Placa;
        _txtMarca.Text = veiculo.Marca;
        _txtModelo.Text = veiculo.Modelo;
        _numAno.Value = veiculo.Ano;
        _numKm.Value = veiculo.Quilometragem;
        SelecionarCategoria(veiculo.CategoriaId);

        switch (veiculo)
        {
            case Carro carro:
                _rbCarro.Checked = true;
                _numPortas.Value = carro.QuantidadePortas;
                _chkAutomatico.Checked = carro.CambioAutomatico;
                break;
            case Moto moto:
                _rbMoto.Checked = true;
                _numCilindradas.Value = moto.Cilindradas;
                break;
        }

        // O tipo (Carro/Moto) não muda depois de cadastrado: é a subclasse no banco
        _rbCarro.Enabled = _rbMoto.Enabled = false;
        AlternarCamposPorTipo();
    }

    private void LimparFormularioVeiculo()
    {
        _veiculoEmEdicaoId = NovoRegistro;
        _txtPlaca.Clear();
        _txtMarca.Clear();
        _txtModelo.Clear();
        _numAno.Value = DateTime.Today.Year;
        _numKm.Value = 0;
        _numPortas.Value = 4;
        _chkAutomatico.Checked = false;
        _numCilindradas.Value = 160;
        _rbCarro.Enabled = _rbMoto.Enabled = true;
        _rbCarro.Checked = true;
        AlternarCamposPorTipo();
    }

    private void AlternarCamposPorTipo()
    {
        _painelCarro.Visible = _rbCarro.Checked;
        _painelMoto.Visible = _rbMoto.Checked;
        AtualizarDiaria();
    }

    /// <summary>Monta Carro OU Moto conforme o tipo marcado (herança na prática).</summary>
    private Veiculo MontarVeiculoDoFormulario()
    {
        Veiculo veiculo = _rbCarro.Checked
            ? new Carro { QuantidadePortas = (int)_numPortas.Value, CambioAutomatico = _chkAutomatico.Checked }
            : new Moto { Cilindradas = (int)_numCilindradas.Value };

        var categoria = _cbCategoria.SelectedItem as Categoria;
        veiculo.Id = _veiculoEmEdicaoId;
        veiculo.Placa = _txtPlaca.Text;
        veiculo.Marca = _txtMarca.Text.Trim();
        veiculo.Modelo = _txtModelo.Text.Trim();
        veiculo.Ano = (int)_numAno.Value;
        veiculo.Quilometragem = (int)_numKm.Value;
        veiculo.CategoriaId = categoria?.Id ?? 0;
        veiculo.Categoria = categoria;
        return veiculo;
    }

    private void AtualizarDiaria()
    {
        var veiculo = MontarVeiculoDoFormulario();
        _lblDiaria.Text = veiculo.Categoria is null
            ? "Diária: selecione a categoria"
            : $"Diária calculada (ITarifa): {Tema.Moeda(veiculo.CalcularDiaria())}";
    }

    private void SalvarVeiculo()
    {
        var resultado = _veiculoController.Salvar(MontarVeiculoDoFormulario());
        Tema.Mostrar(resultado);

        if (!resultado.Sucesso)
            return;

        LimparFormularioVeiculo();
        CarregarVeiculos();
    }

    private void ExcluirVeiculo()
    {
        if (_veiculoEmEdicaoId == NovoRegistro)
        {
            Tema.Mostrar(ResultadoOperacao.Falha("Selecione um veículo na lista e clique em Editar."));
            return;
        }

        if (!Tema.Confirmar("Excluir este veículo?"))
            return;

        var resultado = _veiculoController.Excluir(_veiculoEmEdicaoId);
        Tema.Mostrar(resultado);

        if (!resultado.Sucesso)
            return;

        LimparFormularioVeiculo();
        CarregarVeiculos();
    }

    private void SelecionarCategoria(int categoriaId)
    {
        _cbCategoria.SelectedItem = _cbCategoria.Items
            .Cast<Categoria>()
            .FirstOrDefault(c => c.Id == categoriaId);
    }

    // =================== ABA CATEGORIAS ===================

    private TabPage CriarAbaCategorias()
    {
        var botaoSalvar = Tema.CriarBotao("Salvar", Tema.Sucesso);
        botaoSalvar.Click += (_, _) => SalvarCategoria();
        var botaoNovo = Tema.CriarBotao("Novo / Limpar");
        botaoNovo.Click += (_, _) => LimparFormularioCategoria();
        var botaoEditar = Tema.CriarBotao("Editar selecionada");
        botaoEditar.Click += (_, _) => EditarCategoriaSelecionada();
        var botaoExcluir = Tema.CriarBotao("Excluir selecionada", Tema.Perigo);
        botaoExcluir.Click += (_, _) => ExcluirCategoriaSelecionada();
        _gradeCategorias.CellDoubleClick += (_, _) => EditarCategoriaSelecionada();

        var formulario = Tema.Linha(
            Tema.Campo("Nome da categoria", _txtCategoriaNome),
            Tema.Campo("Diária base (R$)", _numCategoriaDiaria),
            botaoSalvar,
            botaoNovo);
        formulario.Dock = DockStyle.Top;

        var acoes = Tema.Linha(botaoEditar, botaoExcluir);
        acoes.Dock = DockStyle.Bottom;

        var aba = new TabPage("Categorias") { BackColor = Tema.Fundo, Padding = new Padding(Tema.Espaco) };
        aba.Controls.Add(_gradeCategorias);
        aba.Controls.Add(formulario);
        aba.Controls.Add(acoes);
        return aba;
    }

    private void CarregarCategorias()
    {
        var categorias = _categoriaController.Listar();

        _gradeCategorias.DataSource = categorias
            .Select(c => new LinhaCategoria(c.Id, c.Nome, Tema.Moeda(c.ValorDiariaBase)))
            .ToList();

        // A lista de categorias também alimenta o formulário de veículo
        var categoriaAtual = (_cbCategoria.SelectedItem as Categoria)?.Id ?? 0;
        _cbCategoria.Items.Clear();
        _cbCategoria.Items.AddRange(categorias.Cast<object>().ToArray());
        SelecionarCategoria(categoriaAtual);
    }

    private void EditarCategoriaSelecionada()
    {
        var linha = Tema.ItemSelecionado<LinhaCategoria>(_gradeCategorias);
        var categoria = _categoriaController.Listar().FirstOrDefault(c => c.Id == linha?.Id);
        if (categoria is null)
            return;

        _categoriaEmEdicaoId = categoria.Id;
        _txtCategoriaNome.Text = categoria.Nome;
        _numCategoriaDiaria.Value = categoria.ValorDiariaBase;
    }

    private void LimparFormularioCategoria()
    {
        _categoriaEmEdicaoId = NovoRegistro;
        _txtCategoriaNome.Clear();
        _numCategoriaDiaria.Value = 0;
    }

    private void SalvarCategoria()
    {
        var categoria = new Categoria
        {
            Id = _categoriaEmEdicaoId,
            Nome = _txtCategoriaNome.Text,
            ValorDiariaBase = _numCategoriaDiaria.Value
        };

        var resultado = _categoriaController.Salvar(categoria);
        Tema.Mostrar(resultado);

        if (!resultado.Sucesso)
            return;

        LimparFormularioCategoria();
        CarregarCategorias();
        CarregarVeiculos();
    }

    private void ExcluirCategoriaSelecionada()
    {
        var linha = Tema.ItemSelecionado<LinhaCategoria>(_gradeCategorias);
        if (linha is null || !Tema.Confirmar($"Excluir a categoria {linha.Nome}?"))
            return;

        var resultado = _categoriaController.Excluir(linha.Id);
        Tema.Mostrar(resultado);

        if (resultado.Sucesso)
            CarregarCategorias();
    }
}
