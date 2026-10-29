using System.Security.Claims;
using EcommerceInmobiliaria.Data;
using static EcommerceInmobiliaria.Data.Db;

namespace EcommerceInmobiliaria.Services;

public record ContextoAcceso(string Rol, HashSet<string> Permisos);

public static class ClaimsExtensions
{
    public static int? GetUsuarioId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}

/// <summary>
/// Consulta el rol y los permisos VIGENTES del usuario directamente en la BD (una vez por petición),
/// de modo que un cambio hecho por el administrador se aplica de inmediato, sin volver a iniciar sesión.
/// </summary>
public class AutorizacionService
{
    private readonly Db _db;
    private ContextoAcceso? _cache;
    private int? _cacheUsuarioId;

    public AutorizacionService(Db db) => _db = db;

    public async Task<ContextoAcceso> ObtenerContextoAsync(ClaimsPrincipal user)
    {
        var id = user.GetUsuarioId();
        if (id == null) return new ContextoAcceso("", new HashSet<string>());
        if (_cache != null && _cacheUsuarioId == id) return _cache;

        var filas = await _db.QueryAsync(@"
            SELECT r.Nombre, p.Codigo
            FROM Usuarios u
            JOIN Roles r ON r.RolId = u.RolId
            LEFT JOIN RolPermiso rp ON rp.RolId = r.RolId
            LEFT JOIN Permisos p ON p.PermisoId = rp.PermisoId
            WHERE u.UsuarioId = @id AND u.Activo = 1 AND r.Activo = 1",
            r => (Rol: r.GetString(0), Codigo: r.IsDBNull(1) ? null : r.GetString(1)),
            P("@id", id.Value));

        var rol = filas.FirstOrDefault().Rol ?? "";
        var permisos = filas.Where(f => f.Codigo != null).Select(f => f.Codigo!).ToHashSet();
        _cache = new ContextoAcceso(rol, permisos);
        _cacheUsuarioId = id;
        return _cache;
    }

    public async Task<bool> TienePermisoAsync(ClaimsPrincipal user, string codigo) =>
        (await ObtenerContextoAsync(user)).Permisos.Contains(codigo);
}