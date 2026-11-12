using LocacaoVeiculos.Core.Models;
using LocacaoVeiculos.Core.Repositories;

namespace LocacaoVeiculos.Core.Controllers;

public class CategoriaController
{
    private readonly ICategoriaRepository _categorias;

    public CategoriaController(ICategoriaRepository categorias)
    {
        _categorias = categorias;
    }

    public IReadOnlyList<Categoria> Listar() =>
        _categorias.ListarTodos().OrderBy(c => c.Nome).ToList();

    public ResultadoOperacao Salvar(Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria.Nome))
            return ResultadoOperacao.Falha("Informe o nome da categoria.");

        if (categoria.ValorDiariaBase <= 0)
            return ResultadoOperacao.Falha("A diária base precisa ser maior que zero.");

        categoria.Nome = categoria.Nome.Trim();

        if (categoria.Id == 0)
            _categorias.Adicionar(categoria);
        else
            _categorias.Atualizar(categoria);

        return ResultadoOperacao.Ok("Categoria salva.");
    }

    public ResultadoOperacao Excluir(int categoriaId)
    {
        // Agregação: não apagamos veículos junto com a categoria
        if (_categorias.PossuiVeiculos(categoriaId))
            return ResultadoOperacao.Falha("Existem veículos nesta categoria. Mova-os antes de excluir.");

        _categorias.Remover(categoriaId);
        return ResultadoOperacao.Ok("Categoria excluída.");
    }
}
