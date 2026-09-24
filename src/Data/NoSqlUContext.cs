using System;
using Microsoft.EntityFrameworkCore;
using NoSqlU.Data.Entities;

namespace NoSqlU.Data;

public class NoSqlUContext : DbContext
{
    public NoSqlUContext(DbContextOptions< NoSqlUContext > options) : base(options) { }

    // DbSets originales
    public DbSet< Categoria > Categorias => Set< Categoria >();
    public DbSet< Nivel > Niveles => Set< Nivel >();
    public DbSet< Instructor > Instructores => Set< Instructor >();
    public DbSet< Curso > Cursos => Set< Curso >();

    // Nuevos DbSets para usuarios y configuración
    public DbSet< Sexo > Sexos => Set< Sexo >();
    public DbSet< Estado > Estados => Set< Estado >();
    public DbSet< Usuario > Usuarios => Set< Usuario >();
    public DbSet< UsuarioContrasena > UsuariosContrasenas => Set< UsuarioContrasena >();
    public DbSet< CursoUsuario > CursosUsuarios => Set< CursoUsuario >();
    public DbSet< DireccionUsuario > DireccionesUsuarios => Set< DireccionUsuario >();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =========================================================
        // CONFIGURACIÓN ORIGINAL
        // =========================================================
        modelBuilder.Entity< Categoria >(entity =>
        {
            entity.ToTable("categorias");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id_categoria");
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(255);
            entity.Property(e => e.Estatus).HasColumnName("estatus").HasMaxLength(20);
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
        });

        modelBuilder.Entity< Nivel >(entity =>
        {
            entity.ToTable("niveles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id_nivel");
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Estatus).HasColumnName("estatus").HasMaxLength(20);
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
        });

        modelBuilder.Entity< Instructor >(entity =>
        {
            entity.ToTable("instructores");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id_instructor");
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Apellido).HasColumnName("apellido").HasMaxLength(100).HasDefaultValue(string.Empty);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150).HasDefaultValue(string.Empty);
            entity.Property(e => e.Estatus).HasColumnName("estatus").HasMaxLength(20);
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
        });

        modelBuilder.Entity< Curso >(entity =>
        {
            entity.ToTable("cursos"); // Mantenemos el nombre original en vez de cat_cursos para no romper lo existente
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id_curso");

            entity.Property(e => e.CategoriaId).HasColumnName("id_categoria");
            entity.Property(e => e.NivelId).HasColumnName("id_nivel");
            entity.Property(e => e.InstructorId).HasColumnName("id_instructor");

            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").IsRequired();
            entity.Property(e => e.Precio).HasColumnName("precio").HasColumnType("decimal(10,2)");
            entity.Property(e => e.DuracionMinutos).HasColumnName("duracion_minutos");
            entity.Property(e => e.FechaPublicacion).HasColumnName("fecha_publicacion");
            entity.Property(e => e.Estatus).HasColumnName("estatus").HasMaxLength(20);
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");

            entity.HasOne(e => e.Categoria).WithMany(c => c.Cursos).HasForeignKey(e => e.CategoriaId).HasConstraintName("fk_cursos_categoria").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Nivel).WithMany(n => n.Cursos).HasForeignKey(e => e.NivelId).HasConstraintName("fk_cursos_nivel").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Instructor).WithMany(i => i.Cursos).HasForeignKey(e => e.InstructorId).HasConstraintName("fk_cursos_instructor").OnDelete(DeleteBehavior.Restrict);

            entity.HasCheckConstraint("chk_cursos_precio", "precio > 0");
            entity.HasCheckConstraint("chk_cursos_duracion", "duracion_minutos > 0");
        });

        // =========================================================
        // NUEVA CONFIGURACIÓN: SISTEMA DE USUARIOS
        // =========================================================
        modelBuilder.Entity< Sexo >(entity =>
        {
            entity.ToTable("cat_sexo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id_sexo").HasColumnType("char(1)");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(30).IsRequired();
            entity.Property(e => e.Estatus).HasColumnName("estatus").HasMaxLength(20).HasDefaultValue("ACTIVO"); // Reemplaza ENUM
        });

        modelBuilder.Entity< Estado >(entity =>
        {
            entity.ToTable("cat_estados");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id_estado");
            entity.Property(e => e.NombreEstado).HasColumnName("nombre_estado").HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity< Usuario >(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id_usuario");
            entity.Property(e => e.Nombres).HasColumnName("nombres").HasMaxLength(100).IsRequired();
            entity.Property(e => e.ApellidoPaterno).HasColumnName("apellido_paterno").HasMaxLength(50);
            entity.Property(e => e.ApellidoMaterno).HasColumnName("apellido_materno").HasMaxLength(50);
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento").HasColumnType("date");
            entity.Property(e => e.SexoId).HasColumnName("id_sexo").HasColumnType("char(1)");
            entity.Property(e => e.CorreoEmpresarial).HasColumnName("correo_empresarial").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(20);
            entity.Property(e => e.FechaInscripcion).HasColumnName("fecha_inscripcion").HasColumnType("date");
            
            // Traducción de TIMESTAMP a SQL Server
            entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Estatus).HasColumnName("estatus").HasMaxLength(20).HasDefaultValue("ACTIVO");

            entity.HasIndex(e => e.CorreoEmpresarial).IsUnique();

            entity.HasOne(e => e.Sexo).WithMany().HasForeignKey(e => e.SexoId).HasConstraintName("fk_usuarios_sexo").OnDelete(DeleteBehavior.Restrict);
            
            // Restricción para asegurar al menos un apellido
            entity.HasCheckConstraint("chk_apellidos", "apellido_paterno IS NOT NULL OR apellido_materno IS NOT NULL");
        });

        modelBuilder.Entity< UsuarioContrasena >(entity =>
        {
            entity.ToTable("usuario_contrasena");
            entity.HasKey(e => e.UsuarioId);
            entity.Property(e => e.UsuarioId).HasColumnName("id_usuario").ValueGeneratedNever();
            entity.Property(e => e.UsuarioLogin).HasColumnName("usuario").HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();

            entity.HasIndex(e => e.UsuarioLogin).IsUnique();

            entity.HasOne(e => e.Usuario).WithOne(u => u.Contrasena).HasForeignKey< UsuarioContrasena >(e => e.UsuarioId).HasConstraintName("fk_usuario_contrasena").OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity< CursoUsuario >(entity =>
        {
            entity.ToTable("cursos_usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id_usuario_curso");
            entity.Property(e => e.UsuarioId).HasColumnName("id_usuario");
            entity.Property(e => e.CursoId).HasColumnName("id_curso");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("date");
            entity.Property(e => e.FechaFinalizacion).HasColumnName("fecha_finalizacion").HasColumnType("date");
            entity.Property(e => e.Estatus).HasColumnName("estatus").HasMaxLength(20).HasDefaultValue("EN CURSO");

            entity.HasOne(e => e.Usuario).WithMany(u => u.CursosUsuarios).HasForeignKey(e => e.UsuarioId).HasConstraintName("fk_cursos_usuarios_usuario").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Curso).WithMany().HasForeignKey(e => e.CursoId).HasConstraintName("fk_cursos_usuarios_curso").OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity< DireccionUsuario >(entity =>
        {
            entity.ToTable("direcciones_usuario");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id_direccion");
            entity.Property(e => e.UsuarioId).HasColumnName("id_usuario");
            entity.Property(e => e.EstadoId).HasColumnName("id_estado");
            entity.Property(e => e.Calle).HasColumnName("calle").HasMaxLength(150).IsRequired();
            entity.Property(e => e.NumeroExterior).HasColumnName("numero_exterior").HasMaxLength(20).IsRequired();
            entity.Property(e => e.NumeroInterior).HasColumnName("numero_interior").HasMaxLength(20);
            entity.Property(e => e.CodigoPostal).HasColumnName("codigo_postal").HasMaxLength(10).IsRequired();
            entity.Property(e => e.Colonia).HasColumnName("colonia").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Municipio).HasColumnName("municipio").HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.Usuario).WithMany(u => u.Direcciones).HasForeignKey(e => e.UsuarioId).HasConstraintName("fk_direccion_usuario").OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Estado).WithMany().HasForeignKey(e => e.EstadoId).HasConstraintName("fk_direccion_estado").OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // DATA SEEDING (Datos Semilla)
        // =========================================================
        
        // Semilla de Sexos
        modelBuilder.Entity< Sexo >().HasData(
            new Sexo { Id = 'M', Descripcion = "Masculino", Estatus = "ACTIVO" },
            new Sexo { Id = 'F', Descripcion = "Femenino", Estatus = "ACTIVO" },
            new Sexo { Id = 'X', Descripcion = "No binario / Otro", Estatus = "ACTIVO" }
        );

        // Semilla de Estados traducida desde el script
        modelBuilder.Entity< Estado >().HasData(
            new Estado { Id = 1, NombreEstado = "Aguascalientes" },
            new Estado { Id = 2, NombreEstado = "Baja California" },
            new Estado { Id = 3, NombreEstado = "Baja California Sur" },
            new Estado { Id = 4, NombreEstado = "Campeche" },
            new Estado { Id = 5, NombreEstado = "Coahuila de Zaragoza" },
            new Estado { Id = 6, NombreEstado = "Colima" },
            new Estado { Id = 7, NombreEstado = "Chiapas" },
            new Estado { Id = 8, NombreEstado = "Chihuahua" },
            new Estado { Id = 9, NombreEstado = "Ciudad de México" },
            new Estado { Id = 10, NombreEstado = "Durango" },
            new Estado { Id = 11, NombreEstado = "Guanajuato" },
            new Estado { Id = 12, NombreEstado = "Guerrero" },
            new Estado { Id = 13, NombreEstado = "Hidalgo" },
            new Estado { Id = 14, NombreEstado = "Jalisco" },
            new Estado { Id = 15, NombreEstado = "Estado de México" },
            new Estado { Id = 16, NombreEstado = "Michoacán de Ocampo" },
            new Estado { Id = 17, NombreEstado = "Morelos" },
            new Estado { Id = 18, NombreEstado = "Nayarit" },
            new Estado { Id = 19, NombreEstado = "Nuevo León" },
            new Estado { Id = 20, NombreEstado = "Oaxaca" },
            new Estado { Id = 21, NombreEstado = "Puebla" },
            new Estado { Id = 22, NombreEstado = "Querétaro" },
            new Estado { Id = 23, NombreEstado = "Quintana Roo" },
            new Estado { Id = 24, NombreEstado = "San Luis Potosí" },
            new Estado { Id = 25, NombreEstado = "Sinaloa" },
            new Estado { Id = 26, NombreEstado = "Sonora" },
            new Estado { Id = 27, NombreEstado = "Tabasco" },
            new Estado { Id = 28, NombreEstado = "Tamaulipas" },
            new Estado { Id = 29, NombreEstado = "Tlaxcala" },
            new Estado { Id = 30, NombreEstado = "Veracruz" },
            new Estado { Id = 31, NombreEstado = "Yucatán" },
            new Estado { Id = 32, NombreEstado = "Zacatecas" }
        );

        // Semilla Original de Niveles, Categorías, Instructores y Cursos
        modelBuilder.Entity< Nivel >().HasData(
            new Nivel { Id = 1, Nombre = "Básico", Estatus = "activo", FechaCreacion = new DateTime(2026, 5, 1) },
            new Nivel { Id = 2, Nombre = "Intermedio", Estatus = "activo", FechaCreacion = new DateTime(2026, 5, 1) },
            new Nivel { Id = 3, Nombre = "Avanzado", Estatus = "activo", FechaCreacion = new DateTime(2026, 5, 1) }
        );

        modelBuilder.Entity< Categoria >().HasData(
            new Categoria { Id = 1, Nombre = "Desarrollo Web", Descripcion = "Cursos relacionados con frontend, backend y aplicaciones web.", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Categoria { Id = 2, Nombre = "Bases de Datos", Descripcion = "Cursos sobre SQL, NoSQL, modelado y administración de datos.", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Categoria { Id = 3, Nombre = "Diseño Digital", Descripcion = "Cursos de diseño gráfico, UX/UI y herramientas creativas.", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Categoria { Id = 4, Nombre = "Negocios", Descripcion = "Cursos sobre emprendimiento, administración y marketing.", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Categoria { Id = 5, Nombre = "Ciberseguridad", Descripcion = "Cursos de seguridad informática, redes y protección de sistemas.", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) }
        );

        modelBuilder.Entity< Instructor >().HasData(
            new Instructor { Id = 1, Nombre = "Laura Mendoza", Email = "laura.mendoza@email.com", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Instructor { Id = 2, Nombre = "Carlos Ramírez", Email = "carlos.ramirez@email.com", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Instructor { Id = 3, Nombre = "Andrea Torres", Email = "andrea.torres@email.com", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Instructor { Id = 4, Nombre = "Miguel Hernández", Email = "miguel.hernandez@email.com", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) }
        );

        modelBuilder.Entity< Curso >().HasData(
            new Curso { Id = 1, CategoriaId = 1, NivelId = 1, InstructorId = 1, Nombre = "HTML, CSS y JavaScript desde cero", Descripcion = "Curso introductorio para crear páginas web modernas.", Precio = 499.00m, DuracionMinutos = 300u, FechaPublicacion = new DateTime(2026,5,10), Estatus = "activo", FechaCreacion = new DateTime(2026,5,10) },
            new Curso { Id = 2, CategoriaId = 1, NivelId = 2, InstructorId = 2, Nombre = "PHP y MySQL para aplicaciones web", Descripcion = "Aprende a crear sistemas web usando PHP y bases de datos MySQL.", Precio = 799.00m, DuracionMinutos = 420u, FechaPublicacion = new DateTime(2026,5,11), Estatus = "activo", FechaCreacion = new DateTime(2026,5,11) },
            new Curso { Id = 3, CategoriaId = 2, NivelId = 1, InstructorId = 3, Nombre = "Fundamentos de SQL", Descripcion = "Curso básico para aprender consultas, tablas, relaciones y filtros en SQL.", Precio = 399.00m, DuracionMinutos = 240u, FechaPublicacion = new DateTime(2026,5,12), Estatus = "activo", FechaCreacion = new DateTime(2026,5,12) },
            new Curso { Id = 4, CategoriaId = 2, NivelId = 2, InstructorId = 4, Nombre = "MongoDB para principiantes", Descripcion = "Aprende documentos, colecciones, filtros y modelado NoSQL.", Precio = 599.00m, DuracionMinutos = 360u, FechaPublicacion = new DateTime(2026,5,13), Estatus = "activo", FechaCreacion = new DateTime(2026,5,13) },
            new Curso { Id = 5, CategoriaId = 3, NivelId = 2, InstructorId = 1, Nombre = "UX/UI para interfaces modernas", Descripcion = "Diseña interfaces atractivas y fáciles de usar.", Precio = 699.00m, DuracionMinutos = 390u, FechaPublicacion = new DateTime(2026,5,14), Estatus = "activo", FechaCreacion = new DateTime(2026,5,14) },
            new Curso { Id = 6, CategoriaId = 5, NivelId = 1, InstructorId = 2, Nombre = "Ciberseguridad básica", Descripcion = "Introducción a amenazas, contraseñas, redes y buenas prácticas de seguridad.", Precio = 549.00m, DuracionMinutos = 330u, FechaPublicacion = new DateTime(2026,5,15), Estatus = "inactivo", FechaCreacion = new DateTime(2026,5,15) }
        );

        base.OnModelCreating(modelBuilder);
    }
}