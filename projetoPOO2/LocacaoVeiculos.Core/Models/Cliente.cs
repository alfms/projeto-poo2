namespace LocacaoVeiculos.Core.Models;

/// <summary>
/// Cliente da locadora. Classe abstrata: só existe como Pessoa Física ou Jurídica (herança).
/// Os clientes já vêm cadastrados no banco (seed), o sistema apenas os consulta.
/// </summary>
public abstract class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // 1xN: um cliente realiza várias locações
    public ICollection<Locacao> Locacoes { get; set; } = new List<Locacao>();

    public abstract string Documento { get; }

    public string Descricao => $"{Nome} · {Documento}";

    /// <summary>Cada tipo de cliente valida o próprio documento (polimorfismo).</summary>
    public abstract bool PodeLocar(DateTime hoje);
}

public class PessoaFisica : Cliente
{
    private const int TamanhoCpf = 11;

    public string Cpf { get; set; } = string.Empty;
    public string Cnh { get; set; } = string.Empty;
    public DateTime ValidadeCnh { get; set; }

    public override string Documento => Cpf;

    public override bool PodeLocar(DateTime hoje) =>
        ApenasDigitos(Cpf).Length == TamanhoCpf
        && !string.IsNullOrWhiteSpace(Cnh)
        && ValidadeCnh.Date >= hoje.Date;

    private static string ApenasDigitos(string texto) => new(texto.Where(char.IsDigit).ToArray());
}

public class PessoaJuridica : Cliente
{
    private const int TamanhoCnpj = 14;

    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;

    public override string Documento => Cnpj;

    public override bool PodeLocar(DateTime hoje) =>
        new string(Cnpj.Where(char.IsDigit).ToArray()).Length == TamanhoCnpj;
}
