/*
    SATURNO - Esquema relacional para el registro de empleados y certificados

    Este archivo crea una base NUEVA desde cero. No es una migracion de la base
    legacy NoSqlU1_migrated y no debe ejecutarse sobre una base con tablas previas.
    Ejecutar una sola vez en SQL Server Management Studio.
*/

IF DB_ID(N'SaturnoEmpleados') IS NULL
    CREATE DATABASE SaturnoEmpleados;
GO

USE SaturnoEmpleados;
GO

CREATE TABLE dbo.cat_sexo
(
    id_sexo CHAR(1) NOT NULL,
    descripcion NVARCHAR(30) NOT NULL,
    estatus NVARCHAR(10) NOT NULL
        CONSTRAINT df_cat_sexo_estatus DEFAULT N'ACTIVO',

    CONSTRAINT pk_cat_sexo PRIMARY KEY (id_sexo),
    CONSTRAINT uq_cat_sexo_descripcion UNIQUE (descripcion),
    CONSTRAINT chk_cat_sexo_estatus
        CHECK (estatus IN (N'ACTIVO', N'INACTIVO'))
);
GO

CREATE TABLE dbo.cat_estados
(
    id_estado INT IDENTITY(1,1) NOT NULL,
    nombre_estado NVARCHAR(100) NOT NULL,
    estatus NVARCHAR(10) NOT NULL
        CONSTRAINT df_cat_estados_estatus DEFAULT N'ACTIVO',

    CONSTRAINT pk_cat_estados PRIMARY KEY (id_estado),
    CONSTRAINT uq_cat_estados_nombre UNIQUE (nombre_estado),
    CONSTRAINT chk_cat_estados_estatus
        CHECK (estatus IN (N'ACTIVO', N'INACTIVO'))
);
GO

CREATE TABLE dbo.empleados
(
    id_empleado INT IDENTITY(1,1) NOT NULL,
    nombres NVARCHAR(100) NOT NULL,
    apellido_paterno NVARCHAR(50) NULL,
    apellido_materno NVARCHAR(50) NULL,
    fecha_nacimiento DATE NOT NULL,
    fecha_ingreso DATE NOT NULL,
    id_sexo CHAR(1) NOT NULL,
    correo_empresarial NVARCHAR(150) NOT NULL,
    telefono NVARCHAR(20) NULL,
    estatus NVARCHAR(10) NOT NULL
        CONSTRAINT df_empleados_estatus DEFAULT N'ACTIVO',

    CONSTRAINT pk_empleados PRIMARY KEY (id_empleado),
    CONSTRAINT uq_empleados_correo UNIQUE (correo_empresarial),
    CONSTRAINT fk_empleados_sexo
        FOREIGN KEY (id_sexo)
        REFERENCES dbo.cat_sexo (id_sexo)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT chk_empleados_apellidos
        CHECK (apellido_paterno IS NOT NULL OR apellido_materno IS NOT NULL),
    CONSTRAINT chk_empleados_estatus
        CHECK (estatus IN (N'ACTIVO', N'INACTIVO'))
);
GO

CREATE TABLE dbo.domicilios_empleado
(
    id_domicilio INT IDENTITY(1,1) NOT NULL,
    id_empleado INT NOT NULL,
    id_estado INT NOT NULL,
    calle NVARCHAR(150) NOT NULL,
    numero_exterior NVARCHAR(20) NOT NULL,
    numero_interior NVARCHAR(20) NULL,
    codigo_postal NVARCHAR(10) NOT NULL,
    colonia NVARCHAR(100) NOT NULL,
    municipio NVARCHAR(100) NOT NULL,

    CONSTRAINT pk_domicilios_empleado PRIMARY KEY (id_domicilio),
    CONSTRAINT uq_domicilios_empleado_empleado UNIQUE (id_empleado),
    CONSTRAINT fk_domicilios_empleado_empleado
        FOREIGN KEY (id_empleado)
        REFERENCES dbo.empleados (id_empleado)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT fk_domicilios_empleado_estado
        FOREIGN KEY (id_estado)
        REFERENCES dbo.cat_estados (id_estado)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
);
GO

CREATE TABLE dbo.cursos_empleado
(
    id_curso_empleado INT IDENTITY(1,1) NOT NULL,
    id_empleado INT NOT NULL,
    nombre_curso NVARCHAR(200) NOT NULL,
    institucion NVARCHAR(150) NOT NULL,
    fecha_finalizacion DATE NULL,
    estatus NVARCHAR(20) NOT NULL
        CONSTRAINT df_cursos_empleado_estatus DEFAULT N'EN CURSO',

    CONSTRAINT pk_cursos_empleado PRIMARY KEY (id_curso_empleado),
    CONSTRAINT uq_cursos_empleado_certificado
        UNIQUE (id_empleado, nombre_curso, institucion),
    CONSTRAINT fk_cursos_empleado_empleado
        FOREIGN KEY (id_empleado)
        REFERENCES dbo.empleados (id_empleado)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT chk_cursos_empleado_estatus
        CHECK (estatus IN (N'COMPLETADO', N'EN CURSO')),
    CONSTRAINT chk_cursos_empleado_fecha_finalizacion
        CHECK (
            (estatus = N'COMPLETADO' AND fecha_finalizacion IS NOT NULL)
            OR estatus = N'EN CURSO'
        )
);
GO

INSERT INTO dbo.cat_sexo (id_sexo, descripcion)
VALUES
    ('M', N'Masculino'),
    ('F', N'Femenino'),
    ('X', N'No binario / Otro');
GO

INSERT INTO dbo.cat_estados (nombre_estado)
VALUES
    (N'Aguascalientes'),
    (N'Baja California'),
    (N'Baja California Sur'),
    (N'Campeche'),
    (N'Coahuila de Zaragoza'),
    (N'Colima'),
    (N'Chiapas'),
    (N'Chihuahua'),
    (N'Ciudad de México'),
    (N'Durango'),
    (N'Guanajuato'),
    (N'Guerrero'),
    (N'Hidalgo'),
    (N'Jalisco'),
    (N'Estado de México'),
    (N'Michoacán de Ocampo'),
    (N'Morelos'),
    (N'Nayarit'),
    (N'Nuevo León'),
    (N'Oaxaca'),
    (N'Puebla'),
    (N'Querétaro'),
    (N'Quintana Roo'),
    (N'San Luis Potosí'),
    (N'Sinaloa'),
    (N'Sonora'),
    (N'Tabasco'),
    (N'Tamaulipas'),
    (N'Tlaxcala'),
    (N'Veracruz'),
    (N'Yucatán'),
    (N'Zacatecas');
GO

SELECT TABLE_NAME AS tabla
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = N'dbo'
  AND TABLE_TYPE = N'BASE TABLE'
ORDER BY TABLE_NAME;
GO
