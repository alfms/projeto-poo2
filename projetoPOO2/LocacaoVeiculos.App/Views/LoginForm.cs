using LocacaoVeiculos.Core.Controllers;
using LocacaoVeiculos.Core.Models;

namespace LocacaoVeiculos.App.Views;

/// <summary>Tela 01 · Login do funcionário.</summary>
public sealed class LoginForm : Form
{
    private readonly AuthController _auth;
    private readonly TextBox _txtLogin = Tema.CriarTexto(260);
    private readonly TextBox _txtSenha = new() { Width = 260, UseSystemPasswordChar = true };
    private readonly Label _lblErro = new() { AutoSize = true, ForeColor = Tema.Perigo, Font = Tema.Texto };

    public Funcionario? FuncionarioLogado { get; private set; }

    public LoginForm(AuthController auth)
    {
        _auth = auth;
        MontarLayout();
    }

    private void MontarLayout()
    {
        Text = "Sistema de Locação · Login";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(380, 330);
        BackColor = Tema.Fundo;

        var botaoEntrar = Tema.CriarBotao("Entrar", Tema.Primaria);
        botaoEntrar.Click += (_, _) => Entrar();
        AcceptButton = botaoEntrar;

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(50, 30, 40, 20)
        };
        layout.Controls.Add(Tema.CriarRotulo("Sistema de Locação", Tema.Titulo));
        layout.Controls.Add(Tema.CriarRotulo("Acesso do funcionário"));
        layout.Controls.Add(Tema.Campo("Login", _txtLogin));
        layout.Controls.Add(Tema.Campo("Senha", _txtSenha));
        layout.Controls.Add(botaoEntrar);
        layout.Controls.Add(_lblErro);
        Controls.Add(layout);

        Shown += (_, _) => _txtLogin.Focus();
    }

    private void Entrar()
    {
        FuncionarioLogado = _auth.Autenticar(_txtLogin.Text, _txtSenha.Text);

        if (FuncionarioLogado is null)
        {
            _lblErro.Text = "Login ou senha inválidos.";
            _txtSenha.Clear();
            _txtSenha.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
