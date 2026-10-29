using System.ComponentModel.DataAnnotations;

namespace EcommerceInmobiliaria.Models;

public class RegistroViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(80)]
    public string Apellido { get; set; } = "";

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo con formato inválido")]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Mínimo 8 caracteres, con mayúscula, minúscula y número")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirme la contraseña")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    [DataType(DataType.Password)]
    public string ConfirmarPassword { get; set; } = "";
}

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo con formato inválido")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class RolFormViewModel
{
    public int RolId { get; set; }

    [Required(ErrorMessage = "El nombre del rol es obligatorio")]
    [StringLength(50)]
    public string Nombre { get; set; } = "";

    [StringLength(200)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}

public class EditarUsuarioViewModel
{
    public Usuario Usuario { get; set; } = new();
    public List<Rol> Roles { get; set; } = new();
}

public class PermisosRolViewModel
{
    public Rol Rol { get; set; } = new();
    public List<Permiso> Permisos { get; set; } = new();
    public HashSet<int> Asignados { get; set; } = new();
}