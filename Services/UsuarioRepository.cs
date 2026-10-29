using EcommerceInmobiliaria.Data;
using EcommerceInmobiliaria.Models;
using Microsoft.Data.SqlClient;
using static EcommerceInmobiliaria.Data.Db;

namespace EcommerceInmobiliaria.Services;

public class UsuarioRepository
{
    private readonly Db _db;
    public UsuarioRepository(Db db) => _db = db;

    private const string SelectBase = @"
        SELECT u.UsuarioId, u.Nombre, u.Apellido, u.Email, u.PasswordHash, u.RolId, r.Nombre,
               u.Activo, u.IntentosFallidos, u.BloqueadoHasta, u.FechaRegistro
        FROM Usuarios u JOIN Roles r ON r.RolId = u.RolId ";

    private static Usuario Map(SqlDataReader r) => new()
    {
        UsuarioId = r.GetInt32(0),
        Nombre = r.GetString(1),
        Apellido = r.GetString(2),
        Email = r.GetString(3),
        PasswordHash = r.GetString(4),
        RolId = r.GetInt32(5),
        RolNombre = r.GetString(6),
        Activo = r.GetBoolean(7),
        IntentosFallidos = r.GetInt32(8),
        BloqueadoHasta = r.IsDBNull(9) ? null : r.GetDateTime(9),
        FechaRegistro = r.GetDateTime(10)
    };

    public Task<Usuario?> GetByEmailAsync(string email) =>
        _db.QueryFirstAsync(SelectBase + "WHERE u.Email = @e", Map, P("@e", email));

    public Task<Usuario?> GetByIdAsync(int id) =>
        _db.QueryFirstAsync(SelectBase + "WHERE u.UsuarioId = @id", Map, P("@id", id));

    public Task<List<Usuario>> ListAsync() =>
        _db.QueryAsync(SelectBase + "ORDER BY u.FechaRegistro DESC", Map);

    public async Task<bool> ExisteEmailAsync(string email) =>
        await _db.ScalarAsync<int>("SELECT COUNT(1) FROM Usuarios WHERE Email = @e", P("@e", email)) > 0;

    public Task<int> CrearAsync(string nombre, string apellido, string email, string hash, int rolId) =>
        _db.ScalarAsync<int>(@"
            INSERT INTO Usuarios (Nombre, Apellido, Email, PasswordHash, RolId)
            OUTPUT INSERTED.UsuarioId
            VALUES (@n, @a, @e, @h, @r)",
            P("@n", nombre), P("@a", apellido), P("@e", email), P("@h", hash), P("@r", rolId));

    /// <summary>Suma un intento fallido; al llegar a 5 bloquea la cuenta 10 minutos. Devuelve true si quedó bloqueada.</summary>
    public async Task<bool> RegistrarFalloAsync(int usuarioId)
    {
        var bloqueado = await _db.ScalarAsync<int>(@"
            UPDATE Usuarios SET IntentosFallidos = IntentosFallidos + 1 WHERE UsuarioId = @id;
            UPDATE Usuarios SET BloqueadoHasta = DATEADD(MINUTE, 10, SYSUTCDATETIME()), IntentosFallidos = 0
             WHERE UsuarioId = @id AND IntentosFallidos >= 5;
            SELECT CASE WHEN BloqueadoHasta > SYSUTCDATETIME() THEN 1 ELSE 0 END FROM Usuarios WHERE UsuarioId = @id;",
            P("@id", usuarioId));
        return bloqueado == 1;
    }

    public Task ResetearIntentosAsync(int usuarioId) =>
        _db.ExecAsync("UPDATE Usuarios SET IntentosFallidos = 0, BloqueadoHasta = NULL WHERE UsuarioId = @id", P("@id", usuarioId));

    public Task ActualizarRolYEstadoAsync(int usuarioId, int rolId, bool activo) =>
        _db.ExecAsync("UPDATE Usuarios SET RolId = @r, Activo = @a WHERE UsuarioId = @id",
            P("@r", rolId), P("@a", activo), P("@id", usuarioId));
}