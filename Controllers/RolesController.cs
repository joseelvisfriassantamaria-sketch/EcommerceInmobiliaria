using System.Security.Claims;
using EcommerceInmobiliaria.Models;
using EcommerceInmobiliaria.Security;
using EcommerceInmobiliaria.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInmobiliaria.Controllers;

[Permiso("ROLES_GESTIONAR")]
public class RolesController : Controller
{
    private const string RolAdmin = "Administrador";

    private readonly RolRepository _roles;
    private readonly AccesoLogger _log;

    public RolesController(RolRepository roles, AccesoLogger log)
    {
        _roles = roles;
        _log = log;
    }

    private Task Bitacora(string evento, string detalle) =>
        _log.RegistrarAsync(User.GetUsuarioId(), User.FindFirstValue(ClaimTypes.Email), evento, detalle);

    // ---------- HU-04 Gestión de roles ----------
    public async Task<IActionResult> Index() => View(await _roles.ListAsync());

    [HttpGet]
    public IActionResult Crear() => View(new RolFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Crear(RolFormViewModel m)
    {
        if (!ModelState.IsValid) return View(m);
        var nombre = m.Nombre.Trim();
        if (await _roles.ExisteNombreAsync(nombre))
        {
            ModelState.AddModelError(nameof(m.Nombre), "Ya existe un rol con ese nombre");
            return View(m);
        }
        await _roles.CrearAsync(nombre, m.Descripcion?.Trim());
        await Bitacora("ROL_CREADO", nombre);
        TempData["Ok"] = $"Rol '{nombre}' creado. Ahora asígnele permisos.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var r = await _roles.GetAsync(id);
        if (r == null) return NotFound();
        return View(new RolFormViewModel { RolId = r.RolId, Nombre = r.Nombre, Descripcion = r.Descripcion, Activo = r.Activo });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(RolFormViewModel m)
    {
        var actual = await _roles.GetAsync(m.RolId);
        if (actual == null) return NotFound();
        if (!ModelState.IsValid) return View(m);

        var nombre = m.Nombre.Trim();
        if (actual.Nombre == RolAdmin && (nombre != RolAdmin || !m.Activo))
        {
            ModelState.AddModelError("", "El rol Administrador no se puede renombrar ni desactivar.");
            return View(m);
        }
        if (await _roles.ExisteNombreAsync(nombre, m.RolId))
        {
            ModelState.AddModelError(nameof(m.Nombre), "Ya existe un rol con ese nombre");
            return View(m);
        }

        await _roles.ActualizarAsync(m.RolId, nombre, m.Descripcion?.Trim(), m.Activo);
        await Bitacora("ROL_EDITADO", $"{actual.Nombre} -> {nombre} (activo: {m.Activo})");
        TempData["Ok"] = "Rol actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- HU-05 Asignación de permisos a un rol ----------
    [HttpGet]
    public async Task<IActionResult> Permisos(int id)
    {
        var rol = await _roles.GetAsync(id);
        if (rol == null) return NotFound();
        return View(new PermisosRolViewModel
        {
            Rol = rol,
            Permisos = await _roles.ListPermisosAsync(),
            Asignados = await _roles.GetPermisoIdsAsync(id)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Permisos(int rolId, int[]? permisoIds)
    {
        var rol = await _roles.GetAsync(rolId);
        if (rol == null) return NotFound();
        if (rol.Nombre == RolAdmin)
        {
            TempData["Error"] = "El rol Administrador siempre conserva todos los permisos.";
            return RedirectToAction(nameof(Index));
        }

        await _roles.SetPermisosAsync(rolId, permisoIds ?? Array.Empty<int>());
        await Bitacora("PERMISOS_ACTUALIZADOS", $"Rol {rol.Nombre}: {(permisoIds?.Length ?? 0)} permisos asignados");
        TempData["Ok"] = $"Permisos del rol '{rol.Nombre}' actualizados. Se aplican de inmediato.";
        return RedirectToAction(nameof(Index));
    }
}
