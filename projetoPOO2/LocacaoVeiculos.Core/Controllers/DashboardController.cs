using LocacaoVeiculos.Core.Models;
using LocacaoVeiculos.Core.Repositories;

namespace LocacaoVeiculos.Core.Controllers;

public class DashboardController
{
    private readonly IVeiculoRepository _veiculos;
    private readonly ILocacaoRepository _locacoes;

    public DashboardController(IVeiculoRepository veiculos, ILocacaoRepository locacoes)
    {
        _veiculos = veiculos;
        _locacoes = locacoes;
    }

    public DashboardResumo ObterResumo(DateTime hoje) => new(
        VeiculosDisponiveis: _veiculos.ContarPorStatus(StatusVeiculo.Disponivel),
        VeiculosAlugados: _veiculos.ContarPorStatus(StatusVeiculo.Alugado),
        LocacoesAtivas: _locacoes.ContarAtivas(),
        DevolucoesAtrasadas: _locacoes.ContarAtrasadas(hoje),
        DevolucoesHoje: _locacoes.ListarDevolucoesPrevistas(hoje));
}
