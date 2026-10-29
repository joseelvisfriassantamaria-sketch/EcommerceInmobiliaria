namespace EcommerceInmobiliaria.Services;

/// <summary>Crea el administrador inicial (y usuarios demo opcionales) con contraseña hasheada.</summary>
public class DbSeeder
{
    private readonly UsuarioRepository _usuarios;
    private readonly RolRepository _roles;
    private readonly IConfiguration _config;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(UsuarioRepository usuarios, RolRepository roles, IConfiguration config, ILogger<DbSeeder> logger)
    {
        _usuarios = usuarios;
        _roles = roles;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var adminRol = await _roles.GetIdPorNombreAsync("Administrador");
        if (adminRol == 0)
            throw new InvalidOperationException("No existen los roles. Ejecute primero Database/01_schema.sql");

        var adminEmail = (_config["Seed:AdminEmail"] ?? "admin@inmobiliaria.com").ToLowerInvariant();
        var adminPass = _config["Seed:AdminPassword"] ?? "Admin123*";
        await CrearSiNoExiste("Administrador", "General", adminEmail, adminPass, adminRol);

        if (_config.GetValue<bool>("Seed:CrearUsuariosDemo"))
        {
            await CrearSiNoExiste("Ana", "Agente", "agente@inmobiliaria.com", "Agente123*", await _roles.GetIdPorNombreAsync("Agente Inmobiliario"));
            await CrearSiNoExiste("Arturo", "Arquitecto", "arquitecto@inmobiliaria.com", "Arquitecto123*", await _roles.GetIdPorNombreAsync("Arquitecto"));
            await CrearSiNoExiste("Carla", "Cliente", "cliente@inmobiliaria.com", "Cliente123*", await _roles.GetIdPorNombreAsync("Cliente"));
        }
    }

    private async Task CrearSiNoExiste(string nombre, string apellido, string email, string password, int rolId)
    {
        if (rolId == 0 || await _usuarios.ExisteEmailAsync(email)) return;
        await _usuarios.CrearAsync(nombre, apellido, email, PasswordHasher.Hash(password), rolId);
        _logger.LogInformation("Usuario inicial creado: {Email}", email);
    }
}