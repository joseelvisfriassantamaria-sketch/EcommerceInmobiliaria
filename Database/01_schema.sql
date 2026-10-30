/* =====================================================================
   TrámiteRD — Épica 1: Configuración / Gestión de Usuarios y Accesos
   Script de base de datos (SQL Server / LocalDB)

   Crea la base, las tablas y los datos semilla (roles y permisos).
   Los USUARIOS iniciales (admin y demo) los crea la app al arrancar
   (DbSeeder), con la contraseña ya hasheada.

   Ejecutar UNA vez antes de levantar la aplicación:
     sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database\01_schema.sql
   (o abrir el archivo en SQL Server Management Studio / Azure Data Studio
    y ejecutarlo).
   ===================================================================== */

IF DB_ID('InmobiliariaDB') IS NULL
    CREATE DATABASE InmobiliariaDB;
GO
USE InmobiliariaDB;
GO

/* --- Reinicio seguro (respeta dependencias) para poder re-ejecutar --- */
IF OBJECT_ID('dbo.RegistroAcceso','U') IS NOT NULL DROP TABLE dbo.RegistroAcceso;
IF OBJECT_ID('dbo.RolPermiso','U')     IS NOT NULL DROP TABLE dbo.RolPermiso;
IF OBJECT_ID('dbo.Permisos','U')       IS NOT NULL DROP TABLE dbo.Permisos;
IF OBJECT_ID('dbo.Usuarios','U')       IS NOT NULL DROP TABLE dbo.Usuarios;
IF OBJECT_ID('dbo.Roles','U')          IS NOT NULL DROP TABLE dbo.Roles;
GO

/* ============================ Tablas ============================ */

CREATE TABLE dbo.Roles (
    RolId       INT IDENTITY(1,1) CONSTRAINT PK_Roles PRIMARY KEY,
    Nombre      NVARCHAR(50)  NOT NULL CONSTRAINT UQ_Roles_Nombre UNIQUE,
    Descripcion NVARCHAR(200) NULL,
    Activo      BIT           NOT NULL CONSTRAINT DF_Roles_Activo DEFAULT (1)
);

CREATE TABLE dbo.Usuarios (
    UsuarioId        INT IDENTITY(1,1) CONSTRAINT PK_Usuarios PRIMARY KEY,
    Nombre           NVARCHAR(80)  NOT NULL,
    Apellido         NVARCHAR(80)  NOT NULL,
    Email            NVARCHAR(150) NOT NULL CONSTRAINT UQ_Usuarios_Email UNIQUE,
    PasswordHash     NVARCHAR(400) NOT NULL,
    RolId            INT           NOT NULL CONSTRAINT FK_Usuarios_Roles REFERENCES dbo.Roles(RolId),
    Activo           BIT           NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
    IntentosFallidos INT           NOT NULL CONSTRAINT DF_Usuarios_Intentos DEFAULT (0),
    BloqueadoHasta   DATETIME2     NULL,
    FechaRegistro    DATETIME2     NOT NULL CONSTRAINT DF_Usuarios_Fecha DEFAULT (SYSUTCDATETIME())
);

CREATE TABLE dbo.Permisos (
    PermisoId   INT IDENTITY(1,1) CONSTRAINT PK_Permisos PRIMARY KEY,
    Codigo      NVARCHAR(50)  NOT NULL CONSTRAINT UQ_Permisos_Codigo UNIQUE,
    Descripcion NVARCHAR(150) NOT NULL,
    Modulo      NVARCHAR(50)  NOT NULL
);

CREATE TABLE dbo.RolPermiso (
    RolId     INT NOT NULL CONSTRAINT FK_RolPermiso_Rol     REFERENCES dbo.Roles(RolId)    ON DELETE CASCADE,
    PermisoId INT NOT NULL CONSTRAINT FK_RolPermiso_Permiso REFERENCES dbo.Permisos(PermisoId) ON DELETE CASCADE,
    CONSTRAINT PK_RolPermiso PRIMARY KEY (RolId, PermisoId)
);

CREATE TABLE dbo.RegistroAcceso (
    RegistroId BIGINT IDENTITY(1,1) CONSTRAINT PK_RegistroAcceso PRIMARY KEY,
    UsuarioId  INT           NULL,
    Email      NVARCHAR(150) NULL,
    Evento     NVARCHAR(40)  NOT NULL,
    Detalle    NVARCHAR(300) NULL,
    Ip         NVARCHAR(45)  NULL,
    Fecha      DATETIME2     NOT NULL CONSTRAINT DF_RegistroAcceso_Fecha DEFAULT (SYSUTCDATETIME())
);
CREATE INDEX IX_RegistroAcceso_Fecha  ON dbo.RegistroAcceso(Fecha DESC, RegistroId DESC);
CREATE INDEX IX_RegistroAcceso_Evento ON dbo.RegistroAcceso(Evento);
GO

/* ======================= Datos semilla ========================= */

/* Roles */
INSERT INTO dbo.Roles (Nombre, Descripcion) VALUES
 ('Administrador',       'Acceso total: gestiona usuarios, roles, permisos y bitácora.'),
 ('Agente Inmobiliario', 'Gestiona el catálogo de trámites.'),
 ('Arquitecto',          'Gestiona los diseños/solicitudes.'),
 ('Cliente',             'Usuario final: consulta trámites y su información.');

/* Permisos (uno por acción controlada en el sistema) */
INSERT INTO dbo.Permisos (Codigo, Descripcion, Modulo) VALUES
 ('USUARIOS_VER',          'Ver la lista de usuarios',              'Usuarios'),
 ('USUARIOS_GESTIONAR',    'Editar rol y estado de los usuarios',   'Usuarios'),
 ('ROLES_GESTIONAR',       'Crear/editar roles y asignar permisos', 'Roles'),
 ('BITACORA_VER',          'Ver la bitácora de accesos',            'Bitácora'),
 ('PROPIEDADES_VER',       'Ver el catálogo',                       'Propiedades'),
 ('PROPIEDADES_GESTIONAR', 'Publicar/editar en el catálogo',        'Propiedades'),
 ('DISENOS_VER',           'Ver los diseños/solicitudes',           'Diseños'),
 ('DISENOS_GESTIONAR',     'Crear/editar diseños/solicitudes',      'Diseños');

/* Administrador = todos los permisos */
INSERT INTO dbo.RolPermiso (RolId, PermisoId)
SELECT r.RolId, p.PermisoId
FROM dbo.Roles r CROSS JOIN dbo.Permisos p
WHERE r.Nombre = 'Administrador';

/* Agente Inmobiliario = catálogo */
INSERT INTO dbo.RolPermiso (RolId, PermisoId)
SELECT r.RolId, p.PermisoId
FROM dbo.Roles r JOIN dbo.Permisos p ON p.Codigo IN ('PROPIEDADES_VER','PROPIEDADES_GESTIONAR')
WHERE r.Nombre = 'Agente Inmobiliario';

/* Arquitecto = diseños */
INSERT INTO dbo.RolPermiso (RolId, PermisoId)
SELECT r.RolId, p.PermisoId
FROM dbo.Roles r JOIN dbo.Permisos p ON p.Codigo IN ('DISENOS_VER','DISENOS_GESTIONAR')
WHERE r.Nombre = 'Arquitecto';

/* Cliente = solo consulta */
INSERT INTO dbo.RolPermiso (RolId, PermisoId)
SELECT r.RolId, p.PermisoId
FROM dbo.Roles r JOIN dbo.Permisos p ON p.Codigo IN ('PROPIEDADES_VER','DISENOS_VER')
WHERE r.Nombre = 'Cliente';
GO

PRINT 'Esquema TrámiteRD creado. Ahora levante la app: los usuarios admin/demo se crean solos.';
GO
