using EcommerceInmobiliaria.Data;
using static EcommerceInmobiliaria.Data.Db;

namespace EcommerceInmobiliaria.Services;

/// <summary>Registra eventos de acceso en la bitácora (trazabilidad de las operaciones).</summary>
public class AccesoLogger
{
    private readonly Db _db;
    private readonly IHttpContextAccessor _http;

    public AccesoLogger(Db db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public Task RegistrarAsync(int? usuarioId, string? email, string evento, string? detalle = null)
    {
        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
        if (detalle is { Length: > 300 }) detalle = detalle[..300];
        if (email is { Length: > 150 }) email = email[..150];
        return _db.ExecAsync(
            "INSERT INTO RegistroAcceso (UsuarioId, Email, Evento, Detalle, Ip) VALUES (@u, @e, @ev, @d, @ip)",
            P("@u", usuarioId), P("@e", email), P("@ev", evento), P("@d", detalle), P("@ip", ip));
    }
}
