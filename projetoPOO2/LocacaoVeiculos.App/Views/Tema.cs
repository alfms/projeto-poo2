using LocacaoVeiculos.Core.Models;

namespace LocacaoVeiculos.App.Views;

/// <summary>
/// Cores, fontes e fábricas de componentes reutilizados por todas as telas.
/// Centralizar aqui evita repetir estilo em cada View (DRY) e "números mágicos".
/// </summary>
internal static class Tema
{
    public const int Espaco = 12;
    public const int LarguraCampo = 220;

    public static readonly Color Fundo = Color.FromArgb(250, 249, 245);
    public static readonly Color Superficie = Color.White;
    public static readonly Color Borda = Color.FromArgb(60, 60, 60);
    public static readonly Color Primaria = Color.FromArgb(25, 113, 194);
    public static readonly Color PrimariaClara = Color.FromArgb(208, 235, 255);
    public static readonly Color Sucesso = Color.FromArgb(47, 158, 68);
    public static readonly Color SucessoClaro = Color.FromArgb(211, 249, 216);
    public static readonly Color Perigo = Color.FromArgb(201, 42, 42);
    public static readonly Color PerigoClaro = Color.FromArgb(255, 227, 227);
    public static readonly Color AvisoClaro = Color.FromArgb(255, 249, 219);

    public static readonly Font Titulo = new("Segoe UI", 18F, FontStyle.Bold);
    public static readonly Font Subtitulo = new("Segoe UI", 11F, FontStyle.Bold);
    public static readonly Font Texto = new("Segoe UI", 10F);
    public static readonly Font NumeroGrande = new("Segoe UI", 24F, FontStyle.Bold);

    public static Button CriarBotao(string texto, Color? corDeFundo = null)
    {
        var botao = new Button
        {
            Text = texto,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            Font = Texto,
            BackColor = corDeFundo ?? Superficie,
            ForeColor = corDeFundo.HasValue ? Color.White : Color.Black,
            Padding = new Padding(8, 2, 8, 2),
            Margin = new Padding(0, 0, Espaco / 2, Espaco / 2),
            Cursor = Cursors.Hand
        };
        botao.FlatAppearance.BorderColor = Borda;
        return botao;
    }

    public static Label CriarRotulo(string texto, Font? fonte = null) => new()
    {
        Text = texto,
        AutoSize = true,
        Font = fonte ?? Texto,
        Margin = new Padding(0, 0, 0, Espaco / 2)
    };

    /// <summary>Rótulo em cima, controle embaixo.</summary>
    public static Control Campo(string rotulo, Control controle)
    {
        controle.Font = Texto;
        var painel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 0, Espaco, Espaco / 2)
        };
        painel.Controls.Add(new Label { Text = rotulo, AutoSize = true, Font = Texto, Margin = new Padding(0, 0, 0, 2) });
        painel.Controls.Add(controle);
        return painel;
    }

    /// <summary>Agrupa controles lado a lado.</summary>
    public static FlowLayoutPanel Linha(params Control[] controles)
    {
        var linha = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            Margin = new Padding(0)
        };
        linha.Controls.AddRange(controles);
        return linha;
    }

    public static TextBox CriarTexto(int largura = LarguraCampo) => new() { Width = largura };

    public static NumericUpDown CriarNumero(decimal minimo, decimal maximo, int casasDecimais = 0) => new()
    {
        Maximum = maximo,
        Minimum = minimo,
        DecimalPlaces = casasDecimais,
        ThousandsSeparator = true,
        Width = LarguraCampo / 2
    };

    public static ComboBox CriarLista(int largura = LarguraCampo) => new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = largura
    };

    /// <summary>Grade só leitura. A coluna "Id" fica escondida, mas disponível no item selecionado.</summary>
    public static DataGridView CriarGrade()
    {
        var grade = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Superficie,
            Font = Texto
        };
        grade.DataBindingComplete += (_, _) =>
        {
            if (grade.Columns.Contains("Id"))
                grade.Columns["Id"]!.Visible = false;
        };
        return grade;
    }

    public static T? ItemSelecionado<T>(DataGridView grade) where T : class =>
        grade.CurrentRow?.DataBoundItem as T;

    public static string Moeda(decimal valor) => valor.ToString("C2");

    public static string Data(DateTime data) => data.ToString("dd/MM/yyyy");

    public static void Mostrar(ResultadoOperacao resultado) =>
        MessageBox.Show(
            resultado.Mensagem,
            resultado.Sucesso ? "Sucesso" : "Atenção",
            MessageBoxButtons.OK,
            resultado.Sucesso ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

    public static bool Confirmar(string pergunta) =>
        MessageBox.Show(pergunta, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}
