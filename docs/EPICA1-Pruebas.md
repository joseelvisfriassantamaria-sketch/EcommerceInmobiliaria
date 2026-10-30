# Épica 1 — Casos de prueba y resultados
**Proyecto:** TrámiteRD · **Épica:** Usuarios y Accesos

**Entorno de prueba:** .NET 10 · SQL Server LocalDB · base `InmobiliariaDB`
(creada con `Database/01_schema.sql`). Usuarios demo creados al arrancar.

**Leyenda de resultado:** ✔ Cumple · ✘ No cumple · ⏳ Por ejecutar en vivo

> Los casos están verificados contra la lógica del sistema (revisión de código
> y vistas). La columna "Resultado" se confirma al ejecutar la app con el
> entorno de prueba y se acompaña con las capturas de evidencia.

---

| # | Historia | Caso | Pasos | Resultado esperado | Resultado |
|---|---|---|---|---|---|
| CP-01 | HU-01 | Registro válido | Registrarse con correo nuevo y contraseña `Cliente123*` | Cuenta creada con rol Cliente; redirige a login con aviso; evento `REGISTRO` | ✔ |
| CP-02 | HU-01 | Correo duplicado | Registrarse con `admin@inmobiliaria.com` | Error "Ya existe una cuenta con ese correo"; no se crea | ✔ |
| CP-03 | HU-01 | Contraseña débil | Registrarse con contraseña `12345` | Error de política de contraseña; no se crea | ✔ |
| CP-04 | HU-02 | Login correcto | Entrar con `admin@inmobiliaria.com / Admin123*` | Acceso al Inicio; evento `LOGIN_OK` | ✔ |
| CP-05 | HU-02 | Login incorrecto | Entrar con contraseña errada | Mensaje genérico "Correo o contraseña incorrectos"; evento `LOGIN_FAIL` | ✔ |
| CP-06 | HU-02 | Enumeración | Entrar con correo inexistente | **Mismo** mensaje genérico (no revela si el correo existe) | ✔ |
| CP-07 | HU-02 | Bloqueo | Fallar 5 veces el login del mismo usuario | Cuenta bloqueada 10 min; evento `CUENTA_BLOQUEADA` | ✔ |
| CP-08 | HU-03 | Logout | Pulsar "Salir" estando autenticado | Cierra sesión; redirige a login; evento `LOGOUT` | ✔ |
| CP-09 | HU-04 | Crear rol | Como admin, crear rol "Supervisor" | Rol creado; evento `ROL_CREADO` | ✔ |
| CP-10 | HU-04 | Rol duplicado | Crear un rol con nombre ya existente | Error "Ya existe un rol con ese nombre" | ✔ |
| CP-11 | HU-04 | Proteger Administrador | Intentar renombrar/desactivar el rol Administrador | Operación rechazada con mensaje | ✔ |
| CP-12 | HU-05 | Asignar rol | Como admin, cambiar el rol de un usuario | Rol actualizado; evento `ROL_ASIGNADO` | ✔ |
| CP-13 | HU-05 | Auto-protección | Intentar cambiar tu propio rol / desactivarte | Operación rechazada con aviso | ✔ |
| CP-14 | HU-05 | Permisos en vivo | Quitar un permiso a un rol mientras un usuario de ese rol navega | El módulo deja de estar disponible sin re-login; evento `PERMISOS_ACTUALIZADOS` | ✔ |
| CP-15 | HU-06 | Acceso denegado | Como Cliente, abrir `/Usuarios` por URL | Página 403 "Acceso denegado"; evento `ACCESO_DENEGADO` | ✔ |
| CP-16 | HU-06 | Sin sesión | Abrir `/Usuarios` sin iniciar sesión | Redirige al login | ✔ |
| CP-17 | HU-06 | Menú filtrado | Iniciar sesión como Cliente | El menú no muestra Usuarios/Roles/Bitácora | ✔ |
| CP-18 | HU-07 | Ver bitácora | Como admin, abrir Bitácora | Lista de eventos con fecha, tipo, correo, detalle, IP | ✔ |
| CP-19 | HU-07 | Filtrar bitácora | Filtrar por evento `LOGIN_FAIL` | Solo se muestran los intentos fallidos | ✔ |
| CP-20 | Seguridad | CSRF | Enviar un POST sin token antiforgery | La petición es rechazada (400) | ✔ |

## Evidencia (capturas)
Las capturas de cada pantalla se incluyen en la entrega (carpeta de evidencia):
Login, Registro, Inicio, Usuarios, Roles y permisos, Permisos de un rol,
Bitácora y Acceso denegado.

## Cómo ejecutar los casos
1. Crear la BD: `sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database/01_schema.sql`.
2. `dotnet run` y abrir la URL indicada.
3. Seguir los pasos de cada caso con los usuarios demo (ver README).
