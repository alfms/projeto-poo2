using LocacaoVeiculos.Core.Models;
using LocacaoVeiculos.Core.Repositories;

namespace LocacaoVeiculos.Core.Controllers;

public class LocacaoController
{
    private readonly ILocacaoRepository _locacoes;
    private readonly IClienteRepository _clientes;
    private readonly IVeiculoRepository _veiculos;

    public LocacaoController(ILocacaoRepository locacoes, IClienteRepository clientes, IVeiculoRepository veiculos)
    {
        _locacoes = locacoes;
        _clientes = clientes;
        _veiculos = veiculos;
    }

    public IReadOnlyList<Cliente> ListarClientes() =>
        _clientes.ListarTodos().OrderBy(c => c.Nome).ToList();

    public IReadOnlyList<Veiculo> ListarVeiculosDisponiveis() =>
        _veiculos.ListarDisponiveis().OrderBy(v => v.Placa).ToList();

    /// <summary>Calcula o valor da locação usando ITarifa de cada veículo (polimorfismo).</summary>
    public Orcamento CalcularOrcamento(IEnumerable<int> veiculoIds, DateTime retirada, DateTime devolucao)
    {
        var selecionados = BuscarDisponiveis(veiculoIds);
        if (selecionados.Count == 0)
            return Orcamento.Vazio;

        var diarias = Locacao.CalcularDiarias(retirada, devolucao);
        var itens = selecionados
            .Select(v => CriarItem(v, diarias))
            .ToList();

        return new Orcamento(diarias, itens, itens.Sum(i => i.Subtotal));
    }

    public ResultadoOperacao Registrar(int clienteId, int funcionarioId, DateTime retirada,
        DateTime devolucao, IReadOnlyCollection<int> veiculoIds)
    {
        var cliente = _clientes.ObterPorId(clienteId);
        if (cliente is null)
            return ResultadoOperacao.Falha("Selecione um cliente.");

        if (!cliente.PodeLocar(DateTime.Today))
            return ResultadoOperacao.Falha("Cliente com documento inválido ou CNH vencida.");

        if (devolucao.Date <= retirada.Date)
            return ResultadoOperacao.Falha("A devolução precisa ser depois da retirada.");

        if (veiculoIds.Count == 0)
            return ResultadoOperacao.Falha("Selecione pelo menos um veículo.");

        var orcamento = CalcularOrcamento(veiculoIds, retirada, devolucao);
        if (orcamento.Itens.Count != veiculoIds.Count)
            return ResultadoOperacao.Falha("Algum veículo selecionado não está mais disponível.");

        var locacao = new Locacao
        {
            ClienteId = clienteId,
            FuncionarioId = funcionarioId,
            DataRetirada = retirada.Date,
            DataPrevistaDevolucao = devolucao.Date,
            ValorTotal = orcamento.Total,
            Status = StatusLocacao.Ativa,
            Itens = orcamento.Itens
                .Select(i => new LocacaoVeiculo { VeiculoId = i.VeiculoId, ValorDiaria = i.ValorDiaria })
                .ToList()
        };

        try
        {
            var id = _locacoes.Registrar(locacao);
            return ResultadoOperacao.Ok($"Locação #{id} registrada.");
        }
        catch (InvalidOperationException ex)
        {
            return ResultadoOperacao.Falha(ex.Message);
        }
    }

    public IReadOnlyList<LocacaoResumo> Consultar(FiltroLocacao filtro) => _locacoes.Consultar(filtro);

    public ResultadoOperacao Finalizar(int locacaoId) =>
        _locacoes.Finalizar(locacaoId, DateTime.Today)
            ? ResultadoOperacao.Ok($"Locação #{locacaoId} finalizada. Veículos liberados.")
            : ResultadoOperacao.Falha("Só é possível finalizar locações ativas.");

    public ResultadoOperacao Cancelar(int locacaoId) =>
        _locacoes.Cancelar(locacaoId)
            ? ResultadoOperacao.Ok($"Locação #{locacaoId} cancelada. Veículos liberados.")
            : ResultadoOperacao.Falha("Só é possível cancelar locações ativas.");

    private List<Veiculo> BuscarDisponiveis(IEnumerable<int> veiculoIds)
    {
        var ids = veiculoIds.ToHashSet();
        return _veiculos.ListarDisponiveis().Where(v => ids.Contains(v.Id)).ToList();
    }

    private static ItemOrcamento CriarItem(Veiculo veiculo, int diarias)
    {
        var diaria = Math.Round(veiculo.CalcularDiaria(), 2);
        return new ItemOrcamento(veiculo.Id, $"{veiculo.Placa} · {veiculo.Modelo}", diaria, diaria * diarias);
    }
}
