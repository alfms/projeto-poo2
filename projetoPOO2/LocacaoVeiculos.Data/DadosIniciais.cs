using LocacaoVeiculos.Core.Models;
using LocacaoVeiculos.Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace LocacaoVeiculos.Data;

/// <summary>
/// Seed do banco (vai para a migration). Os clientes ficam pré-cadastrados aqui:
/// o sistema não tem tela de cadastro de cliente.
/// Dados FICTÍCIOS para teste. Troquem à vontade.
/// </summary>
internal static class DadosIniciais
{
    private static readonly DateTime CnhValida = new(2030, 12, 31);

    public static void Aplicar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Funcionario>().HasData(
            new Funcionario { Id = 1, Nome = "Administrador", Perfil = PerfilAcesso.Gerente },
            new Funcionario { Id = 2, Nome = "Atendente Balcão", Perfil = PerfilAcesso.Atendente });

        modelBuilder.Entity<Credencial>().HasData(
            new Credencial { Id = 1, FuncionarioId = 1, Login = "admin", SenhaHash = SenhaHasher.GerarHash("admin123") },
            new Credencial { Id = 2, FuncionarioId = 2, Login = "atendente", SenhaHash = SenhaHasher.GerarHash("atend123") });

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nome = "Econômico", ValorDiariaBase = 120m },
            new Categoria { Id = 2, Nome = "SUV", ValorDiariaBase = 250m },
            new Categoria { Id = 3, Nome = "Moto Urbana", ValorDiariaBase = 80m });

        modelBuilder.Entity<Carro>().HasData(
            new Carro { Id = 1, Placa = "ABC1D23", Marca = "Volkswagen", Modelo = "Gol", Ano = 2022, Quilometragem = 35000, CategoriaId = 1, QuantidadePortas = 4, CambioAutomatico = false, Status = StatusVeiculo.Disponivel },
            new Carro { Id = 2, Placa = "JKL7M89", Marca = "Jeep", Modelo = "Compass", Ano = 2024, Quilometragem = 12000, CategoriaId = 2, QuantidadePortas = 4, CambioAutomatico = true, Status = StatusVeiculo.Disponivel },
            new Carro { Id = 3, Placa = "QWE4R56", Marca = "Fiat", Modelo = "Mobi", Ano = 2023, Quilometragem = 20000, CategoriaId = 1, QuantidadePortas = 4, CambioAutomatico = false, Status = StatusVeiculo.Disponivel });

        modelBuilder.Entity<Moto>().HasData(
            new Moto { Id = 4, Placa = "XYZ9A87", Marca = "Honda", Modelo = "CG 160", Ano = 2023, Quilometragem = 8000, CategoriaId = 3, Cilindradas = 160, Status = StatusVeiculo.Disponivel });

        modelBuilder.Entity<PessoaFisica>().HasData(
            new PessoaFisica { Id = 1, Nome = "Cliente Teste PF Um", Telefone = "(27) 90000-0001", Email = "pf1@exemplo.com", Cpf = "00000000001", Cnh = "00000000001", ValidadeCnh = CnhValida },
            new PessoaFisica { Id = 2, Nome = "Cliente Teste PF Dois", Telefone = "(27) 90000-0002", Email = "pf2@exemplo.com", Cpf = "00000000002", Cnh = "00000000002", ValidadeCnh = CnhValida },
            new PessoaFisica { Id = 3, Nome = "Cliente Teste CNH Vencida", Telefone = "(27) 90000-0003", Email = "pf3@exemplo.com", Cpf = "00000000003", Cnh = "00000000003", ValidadeCnh = new DateTime(2020, 1, 31) });

        modelBuilder.Entity<PessoaJuridica>().HasData(
            new PessoaJuridica { Id = 4, Nome = "Empresa Teste PJ", Telefone = "(27) 3000-0000", Email = "pj@exemplo.com", Cnpj = "00000000000100", RazaoSocial = "Empresa Teste PJ Ltda" });
    }
}
