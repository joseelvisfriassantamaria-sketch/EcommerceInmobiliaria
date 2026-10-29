
using EcommerceInmobiliaria.Data;
using EcommerceInmobiliaria.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o =>
{
    // Protección CSRF en todos los POST
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddHttpContextAccessor();

// Acceso a datos y servicios
builder.Services.AddSingleton<Db>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<RolRepository>();
builder.Services.AddScoped<BitacoraRepository>();
builder.Services.AddScoped<AccesoLogger>();
builder.Services.AddScoped<AutorizacionService>();
builder.Services.AddTransient<DbSeeder>();

// Autenticación por cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccesoDenegado";
        o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        o.SlidingExpiration = true;
        o.Cookie.HttpOnly = true;
        o.Cookie.Name = "Inmobiliaria.Auth";
    });

var app = builder.Build();

// Crea el usuario administrador (y usuarios demo) si no existen
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "No se pudo inicializar la base de datos. ¿Ejecutó Database/01_schema.sql y revisó la cadena de conexión?");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();