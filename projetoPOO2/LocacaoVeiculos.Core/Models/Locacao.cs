namespace LocacaoVeiculos.Core.Models;

public enum StatusLocacao
{
    Ativa = 1,
    Finalizada = 2,
    Cancelada = 3
}

/// <summary>Locação de um ou mais veículos por um cliente.</summary>
public class Locacao
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int FuncionarioId { get; set; }
    public Funcionario? Funcionario { get; set; }

    public DateTime DataRetirada { get; set; }
    public DateTime DataPrevistaDevolucao { get; set; }
    public DateTime? DataDevolucao { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusLocacao Status { get; set; } = StatusLocacao.Ativa;

    // Composição: os itens não existem sem a locação
    public ICollection<LocacaoVeiculo> Itens { get; set; } = new List<LocacaoVeiculo>();

    public static int CalcularDiarias(DateTime retirada, DateTime devolucao) =>
        Math.Max(1, (devolucao.Date - retirada.Date).Days);
}

/// <summary>Classe associativa do NxN Locacao ↔ Veiculo. Guarda a diária praticada.</summary>
public class LocacaoVeiculo
{
    public int Id { get; set; }

    public int LocacaoId { get; set; }
    public Locacao? Locacao { get; set; }

    public int VeiculoId { get; set; }
    public Veiculo? Veiculo { get; set; }

    public decimal ValorDiaria { get; set; }
}
