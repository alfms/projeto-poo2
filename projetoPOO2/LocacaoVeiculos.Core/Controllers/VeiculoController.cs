using System.Text.RegularExpressions;
using LocacaoVeiculos.Core.Models;
using LocacaoVeiculos.Core.Repositories;

namespace LocacaoVeiculos.Core.Controllers;

public class VeiculoController
{
    private const int AnoMinimo = 1990;
    private static readonly Regex FormatoPlaca = new("^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$");

    private readonly IVeiculoRepository _veiculos;
    private readonly ICategoriaRepository _categorias;

    public VeiculoController(IVeiculoRepository veiculos, ICategoriaRepository categorias)
    {
        _veiculos = veiculos;
        _categorias = categorias;
    }

    public IReadOnlyList<Categoria> ListarCategorias() =>
        _categorias.ListarTodos().OrderBy(c => c.Nome).ToList();

    public IReadOnlyList<Veiculo> Listar(string? busca, StatusVeiculo? status)
    {
        IEnumerable<Veiculo> veiculos = _veiculos.ListarComCategoria();

        if (!string.IsNullOrWhiteSpace(busca))
            veiculos = veiculos.Where(v => ContemTexto(v, busca.Trim()));

        if (status.HasValue)
            veiculos = veiculos.Where(v => v.Status == status.Value);

        return veiculos.OrderBy(v => v.Placa).ToList();
    }

    public ResultadoOperacao Salvar(Veiculo veiculo)
    {
        veiculo.Placa = NormalizarPlaca(veiculo.Placa);

        var erro = Validar(veiculo);
        if (erro is not null)
            return ResultadoOperacao.Falha(erro);

        if (veiculo.Id == 0)
        {
            veiculo.Status = StatusVeiculo.Disponivel;
            _veiculos.Adicionar(veiculo);
            return ResultadoOperacao.Ok("Veículo cadastrado.");
        }

        // O status só muda pela locação, nunca pela tela de cadastro
        var atual = _veiculos.ObterPorId(veiculo.Id);
        if (atual is null)
            return ResultadoOperacao.Falha("Veículo não encontrado.");

        veiculo.Status = atual.Status;
        _veiculos.Atualizar(veiculo);
        return ResultadoOperacao.Ok("Veículo atualizado.");
    }

    public ResultadoOperacao Excluir(int veiculoId)
    {
        var veiculo = _veiculos.ObterPorId(veiculoId);
        if (veiculo is null)
            return ResultadoOperacao.Falha("Veículo não encontrado.");

        if (!veiculo.EstaDisponivel())
            return ResultadoOperacao.Falha("Veículo alugado não pode ser excluído.");

        if (_veiculos.PossuiLocacoes(veiculoId))
            return ResultadoOperacao.Falha("Veículo tem histórico de locações e não pode ser excluído.");

        _veiculos.Remover(veiculoId);
        return ResultadoOperacao.Ok("Veículo excluído.");
    }

    private string? Validar(Veiculo veiculo)
    {
        if (!FormatoPlaca.IsMatch(veiculo.Placa))
            return "Placa inválida. Use o formato ABC1234 ou ABC1D23.";

        if (string.IsNullOrWhiteSpace(veiculo.Marca) || string.IsNullOrWhiteSpace(veiculo.Modelo))
            return "Informe marca e modelo.";

        var anoMaximo = DateTime.Today.Year + 1;
        if (veiculo.Ano < AnoMinimo || veiculo.Ano > anoMaximo)
            return $"Ano deve estar entre {AnoMinimo} e {anoMaximo}.";

        if (veiculo.CategoriaId == 0 || _categorias.ObterPorId(veiculo.CategoriaId) is null)
            return "Selecione uma categoria.";

        if (_veiculos.PlacaJaCadastrada(veiculo.Placa, veiculo.Id))
            return "Já existe um veículo com esta placa.";

        return veiculo switch
        {
            Carro { QuantidadePortas: < 2 } => "Carro precisa ter pelo menos 2 portas.",
            Moto { Cilindradas: <= 0 } => "Informe as cilindradas da moto.",
            _ => null
        };
    }

    private static string NormalizarPlaca(string placa) =>
        placa.Replace("-", string.Empty).Trim().ToUpperInvariant();

    private static bool ContemTexto(Veiculo veiculo, string busca) =>
        veiculo.Placa.Contains(busca, StringComparison.OrdinalIgnoreCase)
        || veiculo.Modelo.Contains(busca, StringComparison.OrdinalIgnoreCase)
        || veiculo.Marca.Contains(busca, StringComparison.OrdinalIgnoreCase);
}
