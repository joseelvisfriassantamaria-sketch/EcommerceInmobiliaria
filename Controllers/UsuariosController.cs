using EcommerceInmobiliaria.Models;
using EcommerceInmobiliaria.Security;
using EcommerceInmobiliaria.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceInmobiliaria.Controllers;

[Permiso("USUARIOS_VER")]
public class UsuariosController : Controller
{
    private readonly UsuarioRepository _usuarios;
    private readonly RolRepository _roles;
    private readonly AccesoLogger _log;

    public UsuariosController(UsuarioRepository usuarios, RolRepository roles, AccesoLogger log)
    {
        _usuarios = usuarios;
        _roles = roles;
        _log = log;
    }

    public async Task<IActionResult> Index() => View(await _usuarios.ListAsync());

    // ---------- HU-05 Asignación de roles ----------
    [Permiso("USUARIOS_GESTIONAR")]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var u = await _usuarios.GetByIdAsync(id);
        if (u == null) return NotFound();
        return View(new EditarUsuarioViewModel
        {
            Usuario = u,
            Roles = (await _roles.ListAsync()).Where(r => r.Activo || r.RolId == u.RolId).ToList()
        });
    }

    [Permiso("USUARIOS_GESTIONAR")]
    [HttpPost]
    public async Task<IActionResult> Editar(int id, int rolId, bool activo)
    {
        var u = await _usuarios.GetByIdAsync(id);
        var rol = await _roles.GetAsync(rolId);
        if (u == null || rol == null) return NotFound();

        // Evita que el administrador se bloquee a sí mismo
        if (id == User.GetUsuarioId() && (!activo || rolId != u.RolId))
        {
            TempData["Error"] = "No puede cambiar su propio rol ni desactivar su propia cuenta.";
            return RedirectToAction(nameof(Index));
        }

        await _usuarios.ActualizarRolYEstadoAsync(id, rolId, activo);

        var admin = User.FindFirstValue(ClaimTypes.Email);
        if (rolId != u.RolId)
            await _log.RegistrarAsync(User.GetUsuarioId(), admin, "ROL_ASIGNADO",
                $"{u.Email}: {u.RolNombre} -> {rol.Nombre}");
        if (activo != u.Activo)
            await _log.RegistrarAsync(User.GetUsuarioId(), admin, activo ? "USUARIO_ACTIVADO" : "USUARIO_DESACTIVADO", u.Email);

        TempData["Ok"] = $"Usuario {u.Email} actualizado.";
        return RedirectToAction(nameof(Index));
    }
}