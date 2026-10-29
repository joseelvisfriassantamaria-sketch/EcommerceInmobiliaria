using EcommerceInmobiliaria.Security;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInmobiliaria.Controllers;

[Permiso("PROPIEDADES_VER")]
public class PropiedadesController : Controller
{
    public IActionResult Index() => View();

    [Permiso("PROPIEDADES_GESTIONAR")]
    public IActionResult Publicar() => View();
}

[Permiso("DISENOS_VER")]
public class DisenosController : Controller
{
    public IActionResult Index() => View();

    [Permiso("DISENOS_GESTIONAR")]
    public IActionResult Nuevo() => View();
}
