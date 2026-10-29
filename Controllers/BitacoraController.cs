using EcommerceInmobiliaria.Security;
using EcommerceInmobiliaria.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInmobiliaria.Controllers;

[Permiso("BITACORA_VER")]
public class BitacoraController : Controller
{
    private readonly BitacoraRepository _bitacora;
    public BitacoraController(BitacoraRepository bitacora) => _bitacora = bitacora;

    public async Task<IActionResult> Index(string? evento, string? email)
    {
        ViewBag.Evento = evento;
        ViewBag.Email = email;
        return View(await _bitacora.ListAsync(evento, email));
    }
}
