using Microsoft.EntityFrameworkCore;
using NoSqlU.Data.Entities;

namespace NoSqlU.Data;

public class NoSqlUContext : DbContext
{
    public NoSqlUContext(DbContextOptions<NoSqlUContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Nivel> Niveles => Set<Nivel>();
    public DbSet<Instructor> Instructores => Set<Instructor>();
    public DbSet<Curso> Cursos => Set<Curso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
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

        modelBuilder.Entity<Nivel>(entity =>
        {
            entity.ToTable("niveles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id_nivel");
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Estatus).HasColumnName("estatus").HasMaxLength(20);
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
        });

        modelBuilder.Entity<Instructor>(entity =>
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

        modelBuilder.Entity<Curso>(entity =>
        {
            entity.ToTable("cursos");
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

        // Seed data translated from original MySQL schema.sql
        modelBuilder.Entity<Nivel>().HasData(
            new Nivel { Id = 1, Nombre = "Básico", Estatus = "activo", FechaCreacion = new DateTime(2026, 5, 1) },
            new Nivel { Id = 2, Nombre = "Intermedio", Estatus = "activo", FechaCreacion = new DateTime(2026, 5, 1) },
            new Nivel { Id = 3, Nombre = "Avanzado", Estatus = "activo", FechaCreacion = new DateTime(2026, 5, 1) }
        );

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Desarrollo Web", Descripcion = "Cursos relacionados con frontend, backend y aplicaciones web.", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Categoria { Id = 2, Nombre = "Bases de Datos", Descripcion = "Cursos sobre SQL, NoSQL, modelado y administración de datos.", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Categoria { Id = 3, Nombre = "Diseño Digital", Descripcion = "Cursos de diseño gráfico, UX/UI y herramientas creativas.", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Categoria { Id = 4, Nombre = "Negocios", Descripcion = "Cursos sobre emprendimiento, administración y marketing.", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Categoria { Id = 5, Nombre = "Ciberseguridad", Descripcion = "Cursos de seguridad informática, redes y protección de sistemas.", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) }
        );

        modelBuilder.Entity<Instructor>().HasData(
            new Instructor { Id = 1, Nombre = "Laura Mendoza", Email = "laura.mendoza@email.com", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Instructor { Id = 2, Nombre = "Carlos Ramírez", Email = "carlos.ramirez@email.com", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Instructor { Id = 3, Nombre = "Andrea Torres", Email = "andrea.torres@email.com", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) },
            new Instructor { Id = 4, Nombre = "Miguel Hernández", Email = "miguel.hernandez@email.com", Estatus = "activo", FechaCreacion = new DateTime(2026,5,1) }
        );

        modelBuilder.Entity<Curso>().HasData(
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
