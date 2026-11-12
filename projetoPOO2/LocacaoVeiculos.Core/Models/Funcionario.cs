using LocacaoVeiculos.Core.Utils;

namespace LocacaoVeiculos.Core.Models;

public enum PerfilAcesso
{
    Atendente = 1,
    Gerente = 2
}

/// <summary>Funcionário que opera o sistema no balcão da locadora.</summary>
public class Funcionario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public PerfilAcesso Perfil { get; set; }

    // 1x1 (composição): cada funcionário tem exatamente uma credencial de acesso
    public Credencial? Credencial { get; set; }

    // 1xN: um funcionário registra várias locações
    public ICollection<Locacao> Locacoes { get; set; } = new List<Locacao>();
}

/// <summary>Dados de login do funcionário. Lado dependente do 1x1 com Funcionario.</summary>
public class Credencial
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;

    public int FuncionarioId { get; set; }
    public Funcionario? Funcionario { get; set; }

    public bool ConfereSenha(string senha) => SenhaHasher.Confere(senha, SenhaHash);
}
