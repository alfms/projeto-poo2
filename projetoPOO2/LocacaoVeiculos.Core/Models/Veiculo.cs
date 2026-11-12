namespace LocacaoVeiculos.Core.Models;

public enum StatusVeiculo
{
    Disponivel = 1,
    Alugado = 2
}

/// <summary>Contrato de cálculo da diária. Cada tipo de veículo calcula do seu jeito.</summary>
public interface ITarifa
{
    decimal CalcularDiaria();
}

/// <summary>Categoria de veículo (Econômico, SUV...). Agregação: o veículo existe sem ela.</summary>
public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal ValorDiariaBase { get; set; }

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();

    public override string ToString() => Nome;
}

/// <summary>Veículo da frota. Abstrata: só existe como Carro ou Moto (herança).</summary>
public abstract class Veiculo : ITarifa
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public int Quilometragem { get; set; }
    public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

    // 1xN agregação: categoria agrupa veículos
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    // NxN com Locacao, através da classe associativa LocacaoVeiculo
    public ICollection<LocacaoVeiculo> Locacoes { get; set; } = new List<LocacaoVeiculo>();

    public abstract string Tipo { get; }

    public bool EstaDisponivel() => Status == StatusVeiculo.Disponivel;

    public abstract decimal CalcularDiaria();

    protected decimal DiariaBase() =>
        Categoria?.ValorDiariaBase
        ?? throw new InvalidOperationException($"Categoria do veículo {Placa} não foi carregada.");
}

public class Carro : Veiculo
{
    private const decimal AcrescimoCambioAutomatico = 0.15m;

    public int QuantidadePortas { get; set; }
    public bool CambioAutomatico { get; set; }

    public override string Tipo => "Carro";

    public override decimal CalcularDiaria()
    {
        var diaria = DiariaBase();
        return CambioAutomatico ? diaria * (1 + AcrescimoCambioAutomatico) : diaria;
    }
}

public class Moto : Veiculo
{
    private const int LimiteBaixaCilindrada = 300;
    private const decimal FatorBaixaCilindrada = 0.80m;
    private const decimal FatorAltaCilindrada = 1.10m;

    public int Cilindradas { get; set; }

    public override string Tipo => "Moto";

    public override decimal CalcularDiaria()
    {
        var fator = Cilindradas <= LimiteBaixaCilindrada ? FatorBaixaCilindrada : FatorAltaCilindrada;
        return DiariaBase() * fator;
    }
}
