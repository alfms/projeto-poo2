namespace LocacaoVeiculos.Core.Models;

/// <summary>Resultado padrão das operações dos controllers, exibido pela View.</summary>
public record ResultadoOperacao(bool Sucesso, string Mensagem)
{
    public static ResultadoOperacao Ok(string mensagem) => new(true, mensagem);
    public static ResultadoOperacao Falha(string mensagem) => new(false, mensagem);
}

public record FiltroLocacao(StatusLocacao? Status, string? Cliente, DateTime? De, DateTime? Ate);

/// <summary>Linha da consulta de locações (vem do SQL puro).</summary>
public record LocacaoResumo(
    int Id,
    string Cliente,
    int QuantidadeVeiculos,
    DateTime DataRetirada,
    DateTime DataPrevistaDevolucao,
    StatusLocacao Status,
    decimal ValorTotal)
{
    public bool EstaAtrasada(DateTime hoje) =>
        Status == StatusLocacao.Ativa && DataPrevistaDevolucao.Date < hoje.Date;
}

public record ItemOrcamento(int VeiculoId, string Descricao, decimal ValorDiaria, decimal Subtotal);

public record Orcamento(int Diarias, IReadOnlyList<ItemOrcamento> Itens, decimal Total)
{
    public static Orcamento Vazio => new(0, Array.Empty<ItemOrcamento>(), 0m);
}

public record DashboardResumo(
    int VeiculosDisponiveis,
    int VeiculosAlugados,
    int LocacoesAtivas,
    int DevolucoesAtrasadas,
    IReadOnlyList<LocacaoResumo> DevolucoesHoje);
