using EcommerceInmobiliaria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EcommerceInmobiliaria.Security;

/// <summary>
/// Restringe una acción/controlador a usuarios cuyo rol tenga el permiso indicado.
/// Sin sesión → login. Sin permiso → bitácora ACCESO_DENEGADO y página "Acceso denegado".
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PermisoAttribute : TypeFilterAttribute
{
    public PermisoAttribute(string codigo) : base(typeof(PermisoFilter))
    {
        Arguments = new object[] { codigo };
    }
}

public class PermisoFilter : IAsyncAuthorizationFilter
{
    private readonly string _codigo;
    private readonly AutorizacionService _auth;
    private readonly AccesoLogger _log;

    public PermisoFilter(string codigo, AutorizacionService auth, AccesoLogger log)
    {
        _codigo = codigo;
        _auth = auth;
        _log = log;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        if (await _auth.TienePermisoAsync(user, _codigo)) return;

        var email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        await _log.RegistrarAsync(user.GetUsuarioId(), email, "ACCESO_DENEGADO",
            $"{context.HttpContext.Request.Path} requiere {_codigo}");
        context.Result = new RedirectToActionResult("AccesoDenegado", "Account", null);
    }
}
