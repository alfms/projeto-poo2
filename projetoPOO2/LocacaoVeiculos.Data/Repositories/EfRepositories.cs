using LocacaoVeiculos.Core.Models;
using LocacaoVeiculos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocacaoVeiculos.Data.Repositories;

public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(AppDbContext contexto) : base(contexto) { }

    public bool PossuiVeiculos(int categoriaId) =>
        Contexto.Veiculos.Any(v => v.CategoriaId == categoriaId);
}

public class VeiculoRepository : Repository<Veiculo>, IVeiculoRepository
{
    public VeiculoRepository(AppDbContext contexto) : base(contexto) { }

    public IReadOnlyList<Veiculo> ListarComCategoria() =>
        Tabela.AsNoTracking().Include(v => v.Categoria).ToList();

    public IReadOnlyList<Veiculo> ListarDisponiveis() =>
        Tabela.AsNoTracking()
            .Include(v => v.Categoria)
            .Where(v => v.Status == StatusVeiculo.Disponivel)
            .ToList();

    public bool PlacaJaCadastrada(string placa, int ignorarVeiculoId) =>
        Tabela.Any(v => v.Placa == placa && v.Id != ignorarVeiculoId);

    public bool PossuiLocacoes(int veiculoId) =>
        Contexto.LocacaoVeiculos.Any(i => i.VeiculoId == veiculoId);

    public int ContarPorStatus(StatusVeiculo status) =>
        Tabela.Count(v => v.Status == status);

    // A tela manda a Categoria junto (para calcular a diária).
    // Soltamos a navegação para o EF não tentar gravar a categoria de novo.
    public override void Adicionar(Veiculo entidade)
    {
        entidade.Categoria = null;
        base.Adicionar(entidade);
    }

    public override void Atualizar(Veiculo entidade)
    {
        entidade.Categoria = null;
        base.Atualizar(entidade);
    }
}

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _contexto;

    public ClienteRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public IReadOnlyList<Cliente> ListarTodos() => _contexto.Clientes.AsNoTracking().ToList();

    public Cliente? ObterPorId(int id) => _contexto.Clientes.AsNoTracking().FirstOrDefault(c => c.Id == id);
}

public class FuncionarioRepository : IFuncionarioRepository
{
    private readonly AppDbContext _contexto;

    public FuncionarioRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Funcionario? ObterPorLogin(string login) =>
        _contexto.Funcionarios
            .AsNoTracking()
            .Include(f => f.Credencial)
            .FirstOrDefault(f => f.Credencial != null && f.Credencial.Login == login);
}
