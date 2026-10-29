using EcommerceInmobiliaria.Data;
using EcommerceInmobiliaria.Models;
using static EcommerceInmobiliaria.Data.Db;

namespace EcommerceInmobiliaria.Services;

public class BitacoraRepository
{
    private readonly Db _db;
    public BitacoraRepository(Db db) => _db = db;

    public Task<List<RegistroAcceso>> ListAsync(string? evento, string? email, int top = 200) =>
        _db.QueryAsync($@"
            SELECT TOP ({top}) RegistroId, UsuarioId, Email, Evento, Detalle, Ip, Fecha
            FROM RegistroAcceso
            WHERE (@ev IS NULL OR Evento = @ev)
              AND (@em IS NULL OR Email LIKE '%' + @em + '%')
            ORDER BY Fecha DESC, RegistroId DESC",
            r => new RegistroAcceso
            {
                RegistroId = r.GetInt64(0),
                UsuarioId = r.IsDBNull(1) ? null : r.GetInt32(1),
                Email = r.IsDBNull(2) ? null : r.GetString(2),
                Evento = r.GetString(3),
                Detalle = r.IsDBNull(4) ? null : r.GetString(4),
                Ip = r.IsDBNull(5) ? null : r.GetString(5),
                Fecha = r.GetDateTime(6)
            },
            P("@ev", string.IsNullOrWhiteSpace(evento) ? null : evento),
            P("@em", string.IsNullOrWhiteSpace(email) ? null : email.Trim()));
}
