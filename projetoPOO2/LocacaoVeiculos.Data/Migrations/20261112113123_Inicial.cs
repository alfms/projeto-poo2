using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LocacaoVeiculos.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    ValorDiariaBase = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    TipoCliente = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", nullable: true),
                    Cnh = table.Column<string>(type: "TEXT", nullable: true),
                    ValidadeCnh = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Cnpj = table.Column<string>(type: "TEXT", nullable: true),
                    RazaoSocial = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Funcionarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Perfil = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Veiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Placa = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    Marca = table.Column<string>(type: "TEXT", nullable: false),
                    Modelo = table.Column<string>(type: "TEXT", nullable: false),
                    Ano = table.Column<int>(type: "INTEGER", nullable: false),
                    Quilometragem = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoVeiculo = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    QuantidadePortas = table.Column<int>(type: "INTEGER", nullable: true),
                    CambioAutomatico = table.Column<bool>(type: "INTEGER", nullable: true),
                    Cilindradas = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Veiculos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Credenciais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Login = table.Column<string>(type: "TEXT", nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", nullable: false),
                    FuncionarioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credenciais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Credenciais_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Locacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    FuncionarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataRetirada = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataDevolucao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValorTotal = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locacoes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Locacoes_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocacaoVeiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocacaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    VeiculoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorDiaria = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocacaoVeiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocacaoVeiculos_Locacoes_LocacaoId",
                        column: x => x.LocacaoId,
                        principalTable: "Locacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocacaoVeiculos_Veiculos_VeiculoId",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Nome", "ValorDiariaBase" },
                values: new object[,]
                {
                    { 1, "Econômico", 120m },
                    { 2, "SUV", 250m },
                    { 3, "Moto Urbana", 80m }
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Cnh", "Cpf", "Email", "Nome", "Telefone", "TipoCliente", "ValidadeCnh" },
                values: new object[,]
                {
                    { 1, "00000000001", "00000000001", "pf1@exemplo.com", "Cliente Teste PF Um", "(27) 90000-0001", "PF", new DateTime(2030, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "00000000002", "00000000002", "pf2@exemplo.com", "Cliente Teste PF Dois", "(27) 90000-0002", "PF", new DateTime(2030, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "00000000003", "00000000003", "pf3@exemplo.com", "Cliente Teste CNH Vencida", "(27) 90000-0003", "PF", new DateTime(2020, 1, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Cnpj", "Email", "Nome", "RazaoSocial", "Telefone", "TipoCliente" },
                values: new object[] { 4, "00000000000100", "pj@exemplo.com", "Empresa Teste PJ", "Empresa Teste PJ Ltda", "(27) 3000-0000", "PJ" });

            migrationBuilder.InsertData(
                table: "Funcionarios",
                columns: new[] { "Id", "Nome", "Perfil" },
                values: new object[,]
                {
                    { 1, "Administrador", 2 },
                    { 2, "Atendente Balcão", 1 }
                });

            migrationBuilder.InsertData(
                table: "Credenciais",
                columns: new[] { "Id", "FuncionarioId", "Login", "SenhaHash" },
                values: new object[,]
                {
                    { 1, 1, "admin", "240BE518FABD2724DDB6F04EEB1DA5967448D7E831C08C8FA822809F74C720A9" },
                    { 2, 2, "atendente", "C04DA6F490DE9D8636799261074A1F54E0135A0CFD4F2961E2CB29974D84F4FD" }
                });

            migrationBuilder.InsertData(
                table: "Veiculos",
                columns: new[] { "Id", "Ano", "CambioAutomatico", "CategoriaId", "Marca", "Modelo", "Placa", "QuantidadePortas", "Quilometragem", "Status", "TipoVeiculo" },
                values: new object[,]
                {
                    { 1, 2022, false, 1, "Volkswagen", "Gol", "ABC1D23", 4, 35000, 1, "Carro" },
                    { 2, 2024, true, 2, "Jeep", "Compass", "JKL7M89", 4, 12000, 1, "Carro" },
                    { 3, 2023, false, 1, "Fiat", "Mobi", "QWE4R56", 4, 20000, 1, "Carro" }
                });

            migrationBuilder.InsertData(
                table: "Veiculos",
                columns: new[] { "Id", "Ano", "CategoriaId", "Cilindradas", "Marca", "Modelo", "Placa", "Quilometragem", "Status", "TipoVeiculo" },
                values: new object[] { 4, 2023, 3, 160, "Honda", "CG 160", "XYZ9A87", 8000, 1, "Moto" });

            migrationBuilder.CreateIndex(
                name: "IX_Credenciais_FuncionarioId",
                table: "Credenciais",
                column: "FuncionarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Credenciais_Login",
                table: "Credenciais",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocacaoVeiculos_LocacaoId",
                table: "LocacaoVeiculos",
                column: "LocacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_LocacaoVeiculos_VeiculoId",
                table: "LocacaoVeiculos",
                column: "VeiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Locacoes_ClienteId",
                table: "Locacoes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Locacoes_FuncionarioId",
                table: "Locacoes",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_CategoriaId",
                table: "Veiculos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_Placa",
                table: "Veiculos",
                column: "Placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Credenciais");

            migrationBuilder.DropTable(
                name: "LocacaoVeiculos");

            migrationBuilder.DropTable(
                name: "Locacoes");

            migrationBuilder.DropTable(
                name: "Veiculos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Funcionarios");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
