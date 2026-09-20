-- Script T-SQL para SQL Server generado a partir de api-cursos/sql/schema.sql
IF DB_ID(N'NoSqlU1_migrated') IS NULL
BEGIN
	CREATE DATABASE NoSqlU1_migrated;
END
GO
USE NoSqlU1_migrated;
GO

CREATE TABLE categorias (
	id_categoria INT IDENTITY(1,1) PRIMARY KEY,
	nombre VARCHAR(100) NOT NULL UNIQUE,
	descripcion VARCHAR(255) NULL,
	estatus VARCHAR(20) NOT NULL DEFAULT 'activo',
	fecha_creacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE niveles (
	id_nivel INT IDENTITY(1,1) PRIMARY KEY,
	nombre VARCHAR(30) NOT NULL UNIQUE,
	estatus VARCHAR(20) NOT NULL DEFAULT 'activo',
	fecha_creacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE instructores (
	id_instructor INT IDENTITY(1,1) PRIMARY KEY,
	nombre VARCHAR(150) NOT NULL UNIQUE,
	apellido VARCHAR(100) NULL DEFAULT '',
	email VARCHAR(150) NULL DEFAULT '',
	estatus VARCHAR(20) NOT NULL DEFAULT 'activo',
	fecha_creacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE cursos (
	id_curso INT IDENTITY(1,1) PRIMARY KEY,
	id_categoria INT NOT NULL,
	id_nivel INT NOT NULL,
	id_instructor INT NOT NULL,
	nombre VARCHAR(200) NOT NULL,
	descripcion TEXT NOT NULL,
	precio DECIMAL(10,2) NOT NULL,
	duracion_minutos INT NOT NULL,
	fecha_publicacion DATE NOT NULL,
	estatus VARCHAR(20) NOT NULL DEFAULT 'activo',
	fecha_creacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
	CONSTRAINT fk_cursos_categoria FOREIGN KEY (id_categoria) REFERENCES categorias(id_categoria) ON UPDATE CASCADE ON DELETE NO ACTION,
	CONSTRAINT fk_cursos_nivel FOREIGN KEY (id_nivel) REFERENCES niveles(id_nivel) ON UPDATE CASCADE ON DELETE NO ACTION,
	CONSTRAINT fk_cursos_instructor FOREIGN KEY (id_instructor) REFERENCES instructores(id_instructor) ON UPDATE CASCADE ON DELETE NO ACTION,
	CONSTRAINT chk_cursos_precio CHECK (precio > 0),
	CONSTRAINT chk_cursos_duracion CHECK (duracion_minutos > 0)
);
GO

-- Insertar datos básicos si no existen
IF NOT EXISTS (SELECT 1 FROM niveles WHERE nombre = 'Básico')
BEGIN
	INSERT INTO niveles (nombre, estatus) VALUES ('Básico','activo');
END
IF NOT EXISTS (SELECT 1 FROM niveles WHERE nombre = 'Intermedio')
BEGIN
	INSERT INTO niveles (nombre, estatus) VALUES ('Intermedio','activo');
END
IF NOT EXISTS (SELECT 1 FROM niveles WHERE nombre = 'Avanzado')
BEGIN
	INSERT INTO niveles (nombre, estatus) VALUES ('Avanzado','activo');
END

-- Categorías (ejemplos)
IF NOT EXISTS (SELECT 1 FROM categorias WHERE nombre = 'Desarrollo Web')
BEGIN
	INSERT INTO categorias (nombre, descripcion, estatus) VALUES ('Desarrollo Web','Cursos relacionados con frontend, backend y aplicaciones web.','activo');
END
IF NOT EXISTS (SELECT 1 FROM categorias WHERE nombre = 'Bases de Datos')
BEGIN
	INSERT INTO categorias (nombre, descripcion, estatus) VALUES ('Bases de Datos','Cursos sobre SQL, NoSQL, modelado y administración de datos.','activo');
END
GO
