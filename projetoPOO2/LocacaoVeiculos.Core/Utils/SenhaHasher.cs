using System.Security.Cryptography;
using System.Text;

namespace LocacaoVeiculos.Core.Utils;

/// <summary>
/// Hash simples de senha (SHA-256). Suficiente para o trabalho acadêmico;
/// em produção usaríamos um algoritmo com salt e custo (ex.: PBKDF2, BCrypt).
/// </summary>
public static class SenhaHasher
{
    public static string GerarHash(string senha) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(senha)));

    public static bool Confere(string senha, string hash) =>
        string.Equals(GerarHash(senha), hash, StringComparison.OrdinalIgnoreCase);
}
