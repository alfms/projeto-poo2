using LocacaoVeiculos.Core.Models;

namespace LocacaoVeiculos.Core.Repositories;

// Os controllers dependem SÓ destas interfaces (DIP).
// Quem implementa é o projeto LocacaoVeiculos.Data (EF Core e SQL puro).

/// <summary>Operações básicas de CRUD, reaproveitadas por várias entidades.</summary>
public interface IRepository<T> where T : class
{
    IReadOnlyList<T> ListarTodos();
    T? ObterPorId(int id);
    void Adicionar(T entidade);
    void Atualizar(T entidade);
    void Remover(int id);
}

public interface ICategoriaRepository : IRepository<Categoria>
{
    bool PossuiVeiculos(int categoriaId);
}

public interface IVeiculoRepository : IRepository<Veiculo>
{
    IReadOnlyList<Veiculo> ListarComCategoria();
    IReadOnlyList<Veiculo> ListarDisponiveis();
    bool PlacaJaCadastrada(string placa, int ignorarVeiculoId);
    bool PossuiLocacoes(int veiculoId);
    int ContarPorStatus(StatusVeiculo status);
}

/// <summary>Clientes são pré-cadastrados: a interface é só de leitura (ISP).</summary>
public interface IClienteRepository
{
    IReadOnlyList<Cliente> ListarTodos();
    Cliente? ObterPorId(int id);
}

public interface IFuncionarioRepository
{
    Funcionario? ObterPorLogin(string login);
}

/// <summary>Locação: implementada com SQL puro (INSERT, UPDATE e SELECT).</summary>
public interface ILocacaoRepository
{
    int Registrar(Locacao locacao);
    bool Finalizar(int locacaoId, DateTime dataDevolucao);
    bool Cancelar(int locacaoId);
    IReadOnlyList<LocacaoResumo> Consultar(FiltroLocacao filtro);
    IReadOnlyList<LocacaoResumo> ListarDevolucoesPrevistas(DateTime dia);
    int ContarAtivas();
    int ContarAtrasadas(DateTime hoje);
}
