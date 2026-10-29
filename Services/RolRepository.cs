using EcommerceInmobiliaria.Data;
using EcommerceInmobiliaria.Models;
using Microsoft.Data.SqlClient;
using static EcommerceInmobiliaria.Data.Db;

namespace EcommerceInmobiliaria.Services;

public class RolRepository
{
    private readonly Db _db;
    public RolRepository(Db db) => _db = db;

    public Task<List<Rol>> ListAsync() => _db.QueryAsync(@"
        SELECT r.RolId, r.Nombre, r.Descripcion, r.Activo,
               (SELECT COUNT(1) FROM Usuarios u WHERE u.RolId = r.RolId),
               (SELECT COUNT(1) FROM RolPermiso rp WHERE rp.RolId = r.RolId)
        FROM Roles r ORDER BY r.Nombre",
        r => new Rol
        {
            RolId = r.GetInt32(0),
            Nombre = r.GetString(1),
            Descripcion = r.IsDBNull(2) ? null : r.GetString(2),
            Activo = r.GetBoolean(3),
            TotalUsuarios = r.GetInt32(4),
            TotalPermisos = r.GetInt32(5)
        });

    public async Task<Rol?> GetAsync(int id)
    {
        var lista = await ListAsync();
        return lista.FirstOrDefault(r => r.RolId == id);
    }

    public async Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null) =>
        await _db.ScalarAsync<int>("SELECT COUNT(1) FROM Roles WHERE Nombre = @n AND RolId <> @id",
            P("@n", nombre), P("@id", excluirId ?? 0)) > 0;

    public Task<int> CrearAsync(string nombre, string? descripcion) =>
        _db.ScalarAsync<int>("INSERT INTO Roles (Nombre, Descripcion) OUTPUT INSERTED.RolId VALUES (@n, @d)",
            P("@n", nombre), P("@d", descripcion));

    public Task ActualizarAsync(int id, string nombre, string? descripcion, bool activo) =>
        _db.ExecAsync("UPDATE Roles SET Nombre = @n, Descripcion = @d, Activo = @a WHERE RolId = @id",
            P("@n", nombre), P("@d", descripcion), P("@a", activo), P("@id", id));

    public Task<List<Permiso>> ListPermisosAsync() => _db.QueryAsync(
        "SELECT PermisoId, Codigo, Descripcion, Modulo FROM Permisos ORDER BY Modulo, Codigo",
        r => new Permiso
        {
            PermisoId = r.GetInt32(0),
            Codigo = r.GetString(1),
            Descripcion = r.GetString(2),
            Modulo = r.GetString(3)
        });

    public async Task<HashSet<int>> GetPermisoIdsAsync(int rolId)
    {
        var ids = await _db.QueryAsync("SELECT PermisoId FROM RolPermiso WHERE RolId = @r",
            r => r.GetInt32(0), P("@r", rolId));
        return ids.ToHashSet();
    }

    public Task SetPermisosAsync(int rolId, IEnumerable<int> permisoIds) =>
        _db.InTransactionAsync(async (cn, tx) =>
        {
            await using (var del = new SqlCommand("DELETE FROM RolPermiso WHERE RolId = @r", cn, tx))
            {
                del.Parameters.AddWithValue("@r", rolId);
                await del.ExecuteNonQueryAsync();
            }
            foreach (var pid in permisoIds.Distinct())
            {
                await using var ins = new SqlCommand(
                    "INSERT INTO RolPermiso (RolId, PermisoId) SELECT @r, PermisoId FROM Permisos WHERE PermisoId = @p", cn, tx);
                ins.Parameters.AddWithValue("@r", rolId);
                ins.Parameters.AddWithValue("@p", pid);
                await ins.ExecuteNonQueryAsync();
            }
        });

    public Task<int> GetIdPorNombreAsync(string nombre) =>
        _db.ScalarAsync<int>("SELECT RolId FROM Roles WHERE Nombre = @n", P("@n", nombre));
}
