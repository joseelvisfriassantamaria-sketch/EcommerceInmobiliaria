namespace EcommerceInmobiliaria.Models;

public class Usuario
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public int RolId { get; set; }
    public string RolNombre { get; set; } = "";
    public bool Activo { get; set; }
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string NombreCompleto => $"{Nombre} {Apellido}";
}

public class Rol
{
    public int RolId { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public int TotalUsuarios { get; set; }
    public int TotalPermisos { get; set; }
}

public class Permiso
{
    public int PermisoId { get; set; }
    public string Codigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Modulo { get; set; } = "";
}

public class RegistroAcceso
{
    public long RegistroId { get; set; }
    public int? UsuarioId { get; set; }
    public string? Email { get; set; }
    public string Evento { get; set; } = "";
    public string? Detalle { get; set; }
    public string? Ip { get; set; }
    public DateTime Fecha { get; set; }
}
