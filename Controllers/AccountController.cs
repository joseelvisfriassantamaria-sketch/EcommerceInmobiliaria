using System.Security.Claims;
using EcommerceInmobiliaria.Models;
using EcommerceInmobiliaria.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInmobiliaria.Controllers;

public class AccountController : Controller
{
    private readonly UsuarioRepository _usuarios;
    private readonly RolRepository _roles;
    private readonly AccesoLogger _log;

    public AccountController(UsuarioRepository usuarios, RolRepository roles, AccesoLogger log)
    {
        _usuarios = usuarios;
        _roles = roles;
        _log = log;
    }

    // ---------- HU-01 Registro ----------
    [HttpGet]
    public IActionResult Registro() =>
        User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Home") : View(new RegistroViewModel());

    [HttpPost]
    public async Task<IActionResult> Registro(RegistroViewModel m)
    {
        if (!ModelState.IsValid) return View(m);

        var email = m.Email.Trim().ToLowerInvariant();
        if (await _usuarios.ExisteEmailAsync(email))
        {
            ModelState.AddModelError(nameof(m.Email), "Ya existe una cuenta con ese correo");
            return View(m);
        }

        // Todo registro público entra con el rol de menor privilegio: Cliente
        var rolId = await _roles.GetIdPorNombreAsync("Cliente");
        if (rolId == 0)
        {
            ModelState.AddModelError("", "Configuración incompleta: no existe el rol Cliente.");
            return View(m);
        }

        var id = await _usuarios.CrearAsync(m.Nombre.Trim(), m.Apellido.Trim(), email, PasswordHasher.Hash(m.Password), rolId);
        await _log.RegistrarAsync(id, email, "REGISTRO", "Registro de nuevo usuario con rol Cliente");

        TempData["Ok"] = "Cuenta creada correctamente. Ya puede iniciar sesión.";
        return RedirectToAction(nameof(Login));
    }

    // ---------- HU-02 Inicio de sesión ----------
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Home")
            : View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel m)
    {
        if (!ModelState.IsValid) return View(m);

        const string errorGenerico = "Correo o contraseña incorrectos.";
        var email = m.Email.Trim().ToLowerInvariant();
        var u = await _usuarios.GetByEmailAsync(email);

        if (u == null)
        {
            await _log.RegistrarAsync(null, email, "LOGIN_FAIL", "El usuario no existe");
            ModelState.AddModelError("", errorGenerico);
            return View(m);
        }

        if (u.BloqueadoHasta.HasValue && u.BloqueadoHasta.Value > DateTime.UtcNow)
        {
            await _log.RegistrarAsync(u.UsuarioId, email, "LOGIN_FAIL", "Cuenta bloqueada temporalmente");
            ModelState.AddModelError("", "Cuenta bloqueada temporalmente por intentos fallidos. Intente de nuevo en unos minutos.");
            return View(m);
        }

        if (!PasswordHasher.Verify(m.Password, u.PasswordHash))
        {
            var bloqueada = await _usuarios.RegistrarFalloAsync(u.UsuarioId);
            await _log.RegistrarAsync(u.UsuarioId, email, "LOGIN_FAIL", "Contraseña incorrecta");
            if (bloqueada)
                await _log.RegistrarAsync(u.UsuarioId, email, "CUENTA_BLOQUEADA", "5 intentos fallidos: bloqueo de 10 minutos");
            ModelState.AddModelError("", errorGenerico);
            return View(m);
        }

        if (!u.Activo)
        {
            await _log.RegistrarAsync(u.UsuarioId, email, "LOGIN_FAIL", "Cuenta desactivada");
            ModelState.AddModelError("", "Su cuenta está desactivada. Contacte al administrador.");
            return View(m);
        }

        await _usuarios.ResetearIntentosAsync(u.UsuarioId);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, u.UsuarioId.ToString()),
            new(ClaimTypes.Name, u.NombreCompleto),
            new(ClaimTypes.Email, u.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        await _log.RegistrarAsync(u.UsuarioId, email, "LOGIN_OK", $"Inicio de sesión con rol {u.RolNombre}");

        if (!string.IsNullOrEmpty(m.ReturnUrl) && Url.IsLocalUrl(m.ReturnUrl))
            return Redirect(m.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    // ---------- HU-03 Cierre de sesión ----------
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _log.RegistrarAsync(User.GetUsuarioId(), User.FindFirstValue(ClaimTypes.Email), "LOGOUT", "Cierre de sesión");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Ok"] = "Sesión cerrada correctamente.";
        return RedirectToAction(nameof(Login));
    }

    // ---------- HU-06 Acceso denegado ----------
    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
