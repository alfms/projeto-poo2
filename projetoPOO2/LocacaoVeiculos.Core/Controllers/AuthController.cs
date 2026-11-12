using LocacaoVeiculos.Core.Models;
using LocacaoVeiculos.Core.Repositories;

namespace LocacaoVeiculos.Core.Controllers;

public class AuthController
{
    private readonly IFuncionarioRepository _funcionarios;

    public AuthController(IFuncionarioRepository funcionarios)
    {
        _funcionarios = funcionarios;
    }

    /// <summary>Retorna o funcionário autenticado ou null se login/senha não conferem.</summary>
    public Funcionario? Autenticar(string login, string senha)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(senha))
            return null;

        var funcionario = _funcionarios.ObterPorLogin(login.Trim());
        var senhaConfere = funcionario?.Credencial?.ConfereSenha(senha) ?? false;

        return senhaConfere ? funcionario : null;
    }
}
