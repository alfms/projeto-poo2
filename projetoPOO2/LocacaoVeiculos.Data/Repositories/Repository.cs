using LocacaoVeiculos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocacaoVeiculos.Data.Repositories;

/// <summary>
/// CRUD genérico com EF Core. As consultas usam AsNoTracking e o ChangeTracker é limpo
/// após cada gravação, porque o app desktop usa um único contexto o tempo todo.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Contexto;
    protected readonly DbSet<T> Tabela;

    public Repository(AppDbContext contexto)
    {
        Contexto = contexto;
        Tabela = contexto.Set<T>();
    }

    public virtual IReadOnlyList<T> ListarTodos() => Tabela.AsNoTracking().ToList();

    public virtual T? ObterPorId(int id) =>
        Tabela.AsNoTracking().FirstOrDefault(e => EF.Property<int>(e, "Id") == id);

    public virtual void Adicionar(T entidade)
    {
        Tabela.Add(entidade);
        Salvar();
    }

    public virtual void Atualizar(T entidade)
    {
        Tabela.Update(entidade);
        Salvar();
    }

    public virtual void Remover(int id)
    {
        var entidade = Tabela.Find(id);
        if (entidade is null)
            return;

        Tabela.Remove(entidade);
        Salvar();
    }

    protected void Salvar()
    {
        try
        {
            Contexto.SaveChanges();
        }
        finally
        {
            Contexto.ChangeTracker.Clear();
        }
    }
}
