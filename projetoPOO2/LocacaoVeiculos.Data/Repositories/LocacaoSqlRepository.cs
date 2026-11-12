using System.Data;
using System.Data.Common;
using LocacaoVeiculos.Core.Models;
using LocacaoVeiculos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocacaoVeiculos.Data.Repositories;

/// <summary>
/// FUNCIONALIDADE COM SQL PURO exigida pelo enunciado.
/// A locação inteira (INSERT, UPDATE e SELECT) é feita com comandos SQL escritos à mão,
/// sempre com parâmetros (nunca concatenando texto, para evitar SQL Injection).
/// Usa a mesma conexão do EF; o restante do sistema continua no EF.
/// </summary>
public class LocacaoSqlRepository : ILocacaoRepository
{
    private const string SqlInserirLocacao = @"
        INSERT INTO Locacoes (ClienteId, FuncionarioId, DataRetirada, DataPrevistaDevolucao, DataDevolucao, ValorTotal, Status)
        VALUES (@clienteId, @funcionarioId, @retirada, @prevista, NULL, @valorTotal, @status);
        SELECT last_insert_rowid();";

    private const string SqlInserirItem = @"
        INSERT INTO LocacaoVeiculos (LocacaoId, VeiculoId, ValorDiaria)
        VALUES (@locacaoId, @veiculoId, @valorDiaria);";

    private const string SqlAlugarVeiculo = @"
        UPDATE Veiculos SET Status = @alugado
        WHERE Id = @veiculoId AND Status = @disponivel;";

    private const string SqlEncerrarLocacao = @"
        UPDATE Locacoes SET Status = @novoStatus, DataDevolucao = @dataDevolucao
        WHERE Id = @locacaoId AND Status = @ativa;";

    private const string SqlLiberarVeiculos = @"
        UPDATE Veiculos SET Status = @disponivel
        WHERE Id IN (SELECT VeiculoId FROM LocacaoVeiculos WHERE LocacaoId = @locacaoId);";

    private const string SqlSelectResumo = @"
        SELECT l.Id, c.Nome, COUNT(lv.Id) AS QuantidadeVeiculos,
               l.DataRetirada, l.DataPrevistaDevolucao, l.Status, l.ValorTotal
        FROM Locacoes l
        INNER JOIN Clientes c ON c.Id = l.ClienteId
        LEFT JOIN LocacaoVeiculos lv ON lv.LocacaoId = l.Id";

    private const string SqlAgruparResumo = @"
        GROUP BY l.Id, c.Nome, l.DataRetirada, l.DataPrevistaDevolucao, l.Status, l.ValorTotal
        ORDER BY l.Id DESC;";

    private readonly AppDbContext _contexto;

    public LocacaoSqlRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    // ---------- INSERT ----------

    public int Registrar(Locacao locacao)
    {
        var conexao = AbrirConexao();
        using var transacao = conexao.BeginTransaction();

        try
        {
            var locacaoId = InserirLocacao(conexao, transacao, locacao);

            foreach (var item in locacao.Itens)
            {
                InserirItem(conexao, transacao, locacaoId, item);
                MarcarVeiculoComoAlugado(conexao, transacao, item.VeiculoId);
            }

            transacao.Commit();
            return locacaoId;
        }
        catch
        {
            transacao.Rollback();
            throw;
        }
    }

    private static int InserirLocacao(DbConnection conexao, DbTransaction transacao, Locacao locacao)
    {
        using var comando = CriarComando(conexao, transacao, SqlInserirLocacao,
            ("@clienteId", locacao.ClienteId),
            ("@funcionarioId", locacao.FuncionarioId),
            ("@retirada", locacao.DataRetirada),
            ("@prevista", locacao.DataPrevistaDevolucao),
            ("@valorTotal", locacao.ValorTotal),
            ("@status", (int)StatusLocacao.Ativa));

        return Convert.ToInt32(comando.ExecuteScalar());
    }

    private static void InserirItem(DbConnection conexao, DbTransaction transacao, int locacaoId, LocacaoVeiculo item)
    {
        using var comando = CriarComando(conexao, transacao, SqlInserirItem,
            ("@locacaoId", locacaoId),
            ("@veiculoId", item.VeiculoId),
            ("@valorDiaria", item.ValorDiaria));

        comando.ExecuteNonQuery();
    }

    private static void MarcarVeiculoComoAlugado(DbConnection conexao, DbTransaction transacao, int veiculoId)
    {
        using var comando = CriarComando(conexao, transacao, SqlAlugarVeiculo,
            ("@alugado", (int)StatusVeiculo.Alugado),
            ("@disponivel", (int)StatusVeiculo.Disponivel),
            ("@veiculoId", veiculoId));

        // Se nenhuma linha mudou, outro atendente alugou o veículo antes: desfaz tudo
        if (comando.ExecuteNonQuery() != 1)
            throw new InvalidOperationException($"O veículo {veiculoId} não está mais disponível.");
    }

    // ---------- UPDATE ----------

    public bool Finalizar(int locacaoId, DateTime dataDevolucao) =>
        Encerrar(locacaoId, StatusLocacao.Finalizada, dataDevolucao.Date);

    public bool Cancelar(int locacaoId) =>
        Encerrar(locacaoId, StatusLocacao.Cancelada, dataDevolucao: null);

    private bool Encerrar(int locacaoId, StatusLocacao novoStatus, DateTime? dataDevolucao)
    {
        var conexao = AbrirConexao();
        using var transacao = conexao.BeginTransaction();

        try
        {
            using var encerrar = CriarComando(conexao, transacao, SqlEncerrarLocacao,
                ("@novoStatus", (int)novoStatus),
                ("@dataDevolucao", dataDevolucao),
                ("@locacaoId", locacaoId),
                ("@ativa", (int)StatusLocacao.Ativa));

            if (encerrar.ExecuteNonQuery() == 0)
            {
                transacao.Rollback();
                return false;
            }

            using var liberar = CriarComando(conexao, transacao, SqlLiberarVeiculos,
                ("@disponivel", (int)StatusVeiculo.Disponivel),
                ("@locacaoId", locacaoId));
            liberar.ExecuteNonQuery();

            transacao.Commit();
            return true;
        }
        catch
        {
            transacao.Rollback();
            throw;
        }
    }

    // ---------- SELECT ----------

    public IReadOnlyList<LocacaoResumo> Consultar(FiltroLocacao filtro)
    {
        const string sql = SqlSelectResumo + @"
        WHERE (@status IS NULL OR l.Status = @status)
          AND (@cliente IS NULL OR c.Nome LIKE '%' || @cliente || '%')
          AND (@de IS NULL OR l.DataRetirada >= @de)
          AND (@ate IS NULL OR l.DataRetirada < @ate)" + SqlAgruparResumo;

        return LerResumos(sql,
            ("@status", filtro.Status.HasValue ? (int)filtro.Status.Value : (object?)null),
            ("@cliente", string.IsNullOrWhiteSpace(filtro.Cliente) ? (object?)null : filtro.Cliente.Trim()),
            ("@de", filtro.De?.Date),
            ("@ate", filtro.Ate?.Date.AddDays(1)));
    }

    public IReadOnlyList<LocacaoResumo> ListarDevolucoesPrevistas(DateTime dia)
    {
        const string sql = SqlSelectResumo + @"
        WHERE l.Status = @ativa
          AND l.DataPrevistaDevolucao >= @inicio
          AND l.DataPrevistaDevolucao < @fim" + SqlAgruparResumo;

        return LerResumos(sql,
            ("@ativa", (int)StatusLocacao.Ativa),
            ("@inicio", dia.Date),
            ("@fim", dia.Date.AddDays(1)));
    }

    public int ContarAtivas() =>
        ContarLocacoes("SELECT COUNT(*) FROM Locacoes WHERE Status = @ativa;",
            ("@ativa", (int)StatusLocacao.Ativa));

    public int ContarAtrasadas(DateTime hoje) =>
        ContarLocacoes("SELECT COUNT(*) FROM Locacoes WHERE Status = @ativa AND DataPrevistaDevolucao < @hoje;",
            ("@ativa", (int)StatusLocacao.Ativa),
            ("@hoje", hoje.Date));

    private int ContarLocacoes(string sql, params (string Nome, object? Valor)[] parametros)
    {
        using var comando = CriarComando(AbrirConexao(), null, sql, parametros);
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    private List<LocacaoResumo> LerResumos(string sql, params (string Nome, object? Valor)[] parametros)
    {
        using var comando = CriarComando(AbrirConexao(), null, sql, parametros);
        using var leitor = comando.ExecuteReader();

        var resumos = new List<LocacaoResumo>();
        while (leitor.Read())
        {
            resumos.Add(new LocacaoResumo(
                Id: leitor.GetInt32(0),
                Cliente: leitor.GetString(1),
                QuantidadeVeiculos: leitor.GetInt32(2),
                DataRetirada: leitor.GetDateTime(3),
                DataPrevistaDevolucao: leitor.GetDateTime(4),
                Status: (StatusLocacao)leitor.GetInt32(5),
                ValorTotal: leitor.GetDecimal(6)));
        }

        return resumos;
    }

    // ---------- Infra ----------

    private DbConnection AbrirConexao()
    {
        var conexao = _contexto.Database.GetDbConnection();
        if (conexao.State != ConnectionState.Open)
            conexao.Open();

        return conexao;
    }

    private static DbCommand CriarComando(DbConnection conexao, DbTransaction? transacao,
        string sql, params (string Nome, object? Valor)[] parametros)
    {
        var comando = conexao.CreateCommand();
        comando.CommandText = sql;
        comando.Transaction = transacao;

        foreach (var (nome, valor) in parametros)
        {
            var parametro = comando.CreateParameter();
            parametro.ParameterName = nome;
            parametro.Value = valor ?? DBNull.Value;
            comando.Parameters.Add(parametro);
        }

        return comando;
    }
}
