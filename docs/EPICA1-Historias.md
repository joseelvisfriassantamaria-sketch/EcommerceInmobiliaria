# Épica 1 — Historias de usuario y criterios de aceptación
**Proyecto:** TrámiteRD · **Épica:** Configuración / Gestión de Usuarios y Accesos

> Objetivo de la épica: establecer un mecanismo seguro de autenticación,
> autorización y administración de usuarios que controle el acceso a los
> módulos del sistema y asegure la trazabilidad de las operaciones.

Formato de criterios: **Dado / Cuando / Entonces**.

---

## HU-01 · Registro de usuario
**Como** visitante, **quiero** crear una cuenta **para** acceder a la plataforma.

**Criterios de aceptación**
- Dado un correo no registrado, cuando envío nombre, apellido, correo y contraseña válida, entonces se crea la cuenta con el rol **Cliente** (menor privilegio) y se me redirige al login con un aviso de éxito.
- Dado un correo ya registrado, cuando intento registrarme, entonces se muestra "Ya existe una cuenta con ese correo" y no se crea nada.
- Dada una contraseña que no cumple la política (mínimo 8, con mayúscula, minúscula y número), cuando envío el formulario, entonces se muestra el error y no se crea la cuenta.
- La contraseña se guarda **hasheada** (nunca en texto plano).
- El registro queda en la **bitácora** como evento `REGISTRO`.

## HU-02 · Inicio de sesión seguro
**Como** usuario registrado, **quiero** iniciar sesión **para** usar el sistema según mi rol.

**Criterios de aceptación**
- Dadas credenciales correctas y cuenta activa, cuando inicio sesión, entonces entro al Inicio y se registra `LOGIN_OK`.
- Dadas credenciales incorrectas, cuando intento entrar, entonces se muestra un **mensaje genérico** ("Correo o contraseña incorrectos") que no revela si el correo existe, y se registra `LOGIN_FAIL`.
- Dado un 5.º intento fallido, cuando vuelvo a fallar, entonces la cuenta se **bloquea 10 minutos** y se registra `CUENTA_BLOQUEADA`.
- Dada una cuenta desactivada, cuando intento entrar con credenciales correctas, entonces no se permite el acceso.
- La sesión usa una **cookie HttpOnly** con expiración de 30 min.

## HU-03 · Cierre de sesión
**Como** usuario autenticado, **quiero** cerrar sesión **para** proteger mi cuenta.

**Criterios de aceptación**
- Dado que estoy autenticado, cuando pulso "Salir", entonces se cierra la sesión, se me redirige al login y se registra `LOGOUT`.

## HU-04 · Gestión de roles
**Como** administrador, **quiero** crear y editar roles **para** organizar los perfiles del sistema.

**Criterios de aceptación**
- Dado el permiso `ROLES_GESTIONAR`, cuando creo un rol con nombre único, entonces se guarda y se registra `ROL_CREADO`.
- Dado un nombre de rol repetido, cuando intento guardarlo, entonces se muestra "Ya existe un rol con ese nombre".
- El rol **Administrador** no se puede renombrar ni desactivar.
- Editar un rol registra `ROL_EDITADO`.

## HU-05 · Asignación de roles y permisos
**Como** administrador, **quiero** asignar un rol a cada usuario y permisos a cada rol **para** que cada quien acceda solo a lo que le corresponde.

**Criterios de aceptación**
- Dado el permiso `USUARIOS_GESTIONAR`, cuando cambio el rol o el estado de un usuario, entonces se guarda y se registra `ROL_ASIGNADO` / `USUARIO_ACTIVADO` / `USUARIO_DESACTIVADO`.
- No puedo **cambiar mi propio rol** ni **desactivarme** a mí mismo.
- Dado el permiso `ROLES_GESTIONAR`, cuando marco/desmarco permisos de un rol, entonces se guardan y se **aplican de inmediato** a todos sus usuarios (sin volver a iniciar sesión); se registra `PERMISOS_ACTUALIZADOS`.
- El rol Administrador **siempre conserva todos los permisos**.

## HU-06 · Control de acceso por permisos
**Como** administrador, **quiero** que cada módulo exija el permiso correcto **para** evitar accesos no autorizados.

**Criterios de aceptación**
- Dado un usuario sin el permiso requerido, cuando intenta entrar a un módulo, entonces se le muestra la página **"Acceso denegado" (403)** y se registra `ACCESO_DENEGADO`.
- Dado un usuario sin sesión, cuando intenta entrar a un módulo protegido, entonces se le redirige al **login**.
- El menú **solo muestra** los módulos para los que el usuario tiene permiso.

## HU-07 · Bitácora / trazabilidad
**Como** administrador, **quiero** ver un registro de los accesos y operaciones **para** auditar el sistema.

**Criterios de aceptación**
- Dado el permiso `BITACORA_VER`, cuando abro la bitácora, entonces veo fecha, evento, correo, detalle e IP de los eventos recientes.
- Puedo **filtrar** por tipo de evento y por correo.
- Se muestran los **200 eventos más recientes**, del más nuevo al más viejo.

---

### Matriz rol -> permisos (datos semilla)
| Permiso | Administrador | Agente Inmobiliario | Arquitecto | Cliente |
|---|:---:|:---:|:---:|:---:|
| USUARIOS_VER | si | | | |
| USUARIOS_GESTIONAR | si | | | |
| ROLES_GESTIONAR | si | | | |
| BITACORA_VER | si | | | |
| PROPIEDADES_VER (Tramites) | si | si | | si |
| PROPIEDADES_GESTIONAR | si | si | | |
| DISENOS_VER (Solicitudes) | si | | si | si |
| DISENOS_GESTIONAR | si | | si | |
