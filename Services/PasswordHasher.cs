using System.Security.Cryptography;

namespace EcommerceInmobiliaria.Services;

/// <summary>Hash de contraseñas con PBKDF2-SHA256 y sal aleatoria. Formato: iteraciones.sal.hash (base64).</summary>
public static class PasswordHasher
{
    private const int Iteraciones = 210_000;
    private const int TamSal = 16;
    private const int TamHash = 32;

    public static string Hash(string password)
    {
        var sal = RandomNumberGenerator.GetBytes(TamSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, sal, Iteraciones, HashAlgorithmName.SHA256, TamHash);
        return $"{Iteraciones}.{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string almacenado)
    {
        var partes = almacenado.Split('.');
        if (partes.Length != 3 || !int.TryParse(partes[0], out var iter)) return false;
        try
        {
            var sal = Convert.FromBase64String(partes[1]);
            var esperado = Convert.FromBase64String(partes[2]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(password, sal, iter, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
