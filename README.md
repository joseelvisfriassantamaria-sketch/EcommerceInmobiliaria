# TrámiteRD — Épica 1: Configuración / Gestión de Usuarios y Accesos

Aplicación web (ASP.NET Core MVC, C#) que implementa el control de acceso de
**TrámiteRD**: autenticación segura, roles y permisos, administración de
usuarios y **bitácora** de trazabilidad. La interfaz sigue el prototipo de app
móvil de TrámiteRD (HTML/CSS/JS, sin framework de front).

## Tecnología
- **Backend:** ASP.NET Core MVC (.NET 10), C#.
- **Base de datos:** SQL Server (LocalDB) con acceso ADO.NET parametrizado (`Microsoft.Data.SqlClient`).
- **Autenticación:** cookie de sesión (HttpOnly) con expiración deslizante.
- **Front:** Razor + CSS/JS propio (`wwwroot/css/tramiterd.css`, `wwwroot/js/tramiterd.js`).

## Requisitos para ejecutar
1. **.NET 10 SDK** — https://dotnet.microsoft.com/download
2. **SQL Server LocalDB** (viene con "Visual Studio" o con "SQL Server Express").

## Cómo ejecutar (instrucciones reproducibles)
1. Crear la base de datos (una sola vez):
   ```bash
   sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database/01_schema.sql
   ```
   > También puede abrir `Database/01_schema.sql` en SQL Server Management
   > Studio o Azure Data Studio y ejecutarlo.
2. Levantar la aplicación:
   ```bash
   dotnet run
   ```
3. Abrir la URL que indique la consola (p. ej. `https://localhost:5001`).
   Al arrancar, la app crea sola el usuario administrador y los usuarios demo.

### Usuarios de prueba (se crean al arrancar)
| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin@inmobiliaria.com | Admin123* |
| Agente Inmobiliario | agente@inmobiliaria.com | Agente123* |
| Arquitecto | arquitecto@inmobiliaria.com | Arquitecto123* |
| Cliente | cliente@inmobiliaria.com | Cliente123* |

> La contraseña del admin se puede cambiar en `appsettings.json` (sección `Seed`).

## Seguridad incluida (Épica 1)
- Contraseñas con **PBKDF2-SHA256** (210k iteraciones + sal aleatoria).
- **Bloqueo** de la cuenta por 10 min tras 5 intentos fallidos.
- **Mensaje de error genérico** en login (no revela si el correo existe).
- **CSRF** (antiforgery) en todos los POST.
- **Autorización por permisos**: cada acción exige un permiso concreto; el rol
  del usuario se consulta en vivo (un cambio de permisos se aplica al instante).
- **Bitácora**: se registran login, logout, registros, accesos denegados,
  bloqueos y cambios de roles/permisos.

## Estructura
```
EcommerceInmobiliaria/
├── Controllers/        # Account, Usuarios, Roles, Bitacora, Home, Modulos
├── Services/           # Repositorios + Hash + Autorización + Bitácora + Seeder
├── Security/           # PermisoAttribute (filtro de autorización)
├── Models/             # Entidades y ViewModels
├── Views/              # Razor con estilo TrámiteRD
├── wwwroot/css/tramiterd.css   # Sistema de diseño (colores/botones/tablas)
├── wwwroot/js/tramiterd.js     # Interacciones (mostrar contraseña, etc.)
├── Database/01_schema.sql      # Esquema + roles + permisos (datos semilla)
└── docs/               # Historias de usuario y plan de pruebas
```

## Documentación de la Épica 1
- [`docs/EPICA1-Historias.md`](docs/EPICA1-Historias.md) — Historias de usuario y criterios de aceptación.
- [`docs/EPICA1-Pruebas.md`](docs/EPICA1-Pruebas.md) — Casos de prueba y resultados.
