using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInmobiliaria.Controllers;

public class HomeController : Controller
{
    [Authorize]
    public IActionResult Index() => View();

    [AllowAnonymous]
    public IActionResult Error() => View();
}