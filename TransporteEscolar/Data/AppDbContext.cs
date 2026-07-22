using Microsoft.EntityFrameworkCore;
using TransporteEscolar.Models;

namespace TransporteEscolar.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Autobus> Autobuses { get; set; }
        public DbSet<BitacoraAcceso> BitacoraAccesos { get; set; }
        public DbSet<Chofer> Choferes { get; set; }
        public DbSet<JornadaChofer> JornadaChofer { get; set; }
        public DbSet<Alumno> Alumnos { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<Incidencia> Incidencias { get; set; }
        public DbSet<PadreAlumno> PadreAlumno { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Rol ─────────────────────────────────────────────
            modelBuilder.Entity<Rol>(e =>
            {
                e.ToTable("Roles");
                e.HasKey(r => r.RolId);
                e.Property(r => r.Nombre).IsRequired().HasMaxLength(50);
                e.Property(r => r.Descripcion).HasMaxLength(200);
                e.HasIndex(r => r.Nombre).IsUnique();
            });

            // ── Autobús ──────────────────────────────────────────
            modelBuilder.Entity<Autobus>(e =>
            {
                e.ToTable("Autobuses");
                e.HasKey(a => a.AutobusId);
                e.Property(a => a.Ficha).IsRequired().HasMaxLength(20);
                e.Property(a => a.Placa).IsRequired().HasMaxLength(20);
                e.HasIndex(a => a.Ficha).IsUnique();
                e.HasIndex(a => a.Placa).IsUnique();
            });

            // ── Usuario ──────────────────────────────────────────
            modelBuilder.Entity<Usuario>(e =>
            {
                e.ToTable("Usuarios");
                e.HasKey(u => u.UsuarioId);
                e.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(100);
                e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
                e.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(200);
                e.Property(u => u.Email).HasMaxLength(200);
                e.HasIndex(u => u.NombreUsuario).IsUnique();
                e.Property(u => u.FotoUrl).HasMaxLength(500);

                // Relación con Rol
                e.HasOne(u => u.Rol)
                 .WithMany(r => r.Usuarios)
                 .HasForeignKey(u => u.RolId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Relación con Autobús (nullable — solo Azafata)
                e.HasOne(u => u.Autobus)
                 .WithMany(a => a.Usuarios)
                 .HasForeignKey(u => u.AutobusId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ── Bitácora ─────────────────────────────────────────
            modelBuilder.Entity<BitacoraAcceso>(e =>
            {
                e.ToTable("BitacoraAccesos");
                e.HasKey(b => b.BitacoraId);
                e.Property(b => b.NombreUsuario).IsRequired().HasMaxLength(100);
                e.Property(b => b.DireccionIP).HasMaxLength(50);
                e.Property(b => b.UserAgent).HasMaxLength(500);
                e.Property(b => b.Detalle).HasMaxLength(500);

                e.HasOne(b => b.Usuario)
                 .WithMany()
                 .HasForeignKey(b => b.UsuarioId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
            });
            // ── Chofer ───────────────────────────────────────────
            modelBuilder.Entity<Chofer>(e =>
            {
                e.ToTable("Choferes");
                e.HasKey(c => c.ChoferId);
                e.Property(c => c.NombreCompleto).IsRequired().HasMaxLength(200);
                e.Property(c => c.Telefono).HasMaxLength(20);
                e.Property(c => c.DUI).IsRequired().HasMaxLength(20);
                e.Property(c => c.Licencia).IsRequired().HasMaxLength(30);
                e.Property(c => c.FotoUrl).HasMaxLength(500);
                e.HasIndex(c => c.DUI).IsUnique();
            });

            // ── JornadaChofer ─────────────────────────────────────
            modelBuilder.Entity<JornadaChofer>(e =>
            {
                e.ToTable("JornadaChofer");
                e.HasKey(j => j.JornadaId);

                e.HasOne(j => j.Usuario)
                 .WithMany()
                 .HasForeignKey(j => j.UsuarioId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(j => j.Chofer)
                 .WithMany(c => c.Jornadas)
                 .HasForeignKey(j => j.ChoferId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(j => j.Autobus)
                 .WithMany()
                 .HasForeignKey(j => j.AutobusId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
            // ── Alumno ───────────────────────────────────────────
            modelBuilder.Entity<Alumno>(e =>
            {
                e.ToTable("Alumnos");
                e.HasKey(a => a.AlumnoId);
                e.Property(a => a.NombreCompleto).IsRequired().HasMaxLength(200);
                e.Property(a => a.GradoEscolar).HasMaxLength(50);
                e.Property(a => a.NombreTutor).HasMaxLength(200);
                e.Property(a => a.TelefonoEmergencia).HasMaxLength(20);
                e.Property(a => a.FotoUrl).HasMaxLength(500);
                e.Property(a => a.DireccionRecogida).HasMaxLength(300);
                e.Property(a => a.DireccionEntrega).HasMaxLength(300);
                e.Property(a => a.CodigoQR).IsRequired().HasMaxLength(100);
                e.HasIndex(a => a.CodigoQR).IsUnique();

                e.HasOne(a => a.Autobus)
                 .WithMany()
                 .HasForeignKey(a => a.AutobusId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Asistencia ───────────────────────────────────────
            modelBuilder.Entity<Asistencia>(e =>
            {
                e.ToTable("Asistencia");
                e.HasKey(a => a.AsistenciaId);
                e.Property(a => a.TipoRuta).IsRequired().HasMaxLength(10);
                e.Property(a => a.Observacion).HasMaxLength(300);

                e.HasOne(a => a.Alumno)
                 .WithMany(al => al.Asistencias)
                 .HasForeignKey(a => a.AlumnoId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Autobus)
                 .WithMany()
                 .HasForeignKey(a => a.AutobusId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Usuario)
                 .WithMany()
                 .HasForeignKey(a => a.UsuarioId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Incidencia ───────────────────────────────────────
            modelBuilder.Entity<Incidencia>(e =>
            {
                e.ToTable("Incidencias");
                e.HasKey(i => i.IncidenciaId);
                e.Property(i => i.Titulo).IsRequired().HasMaxLength(100);
                e.Property(i => i.Descripcion).IsRequired().HasMaxLength(1000);
                e.Property(i => i.Tipo).IsRequired().HasMaxLength(30);

                e.HasOne(i => i.Autobus)
                 .WithMany()
                 .HasForeignKey(i => i.AutobusId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(i => i.Usuario)
                 .WithMany()
                 .HasForeignKey(i => i.UsuarioId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(i => i.Alumno)
                 .WithMany(a => a.Incidencias)
                 .HasForeignKey(i => i.AlumnoId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ── PadreAlumno ───────────────────────────────────────
            modelBuilder.Entity<PadreAlumno>(e =>
            {
                e.ToTable("PadreAlumno");
                e.HasKey(p => p.PadreAlumnoId);

                e.HasOne(p => p.Usuario)
                 .WithMany()
                 .HasForeignKey(p => p.UsuarioId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(p => p.Alumno)
                 .WithMany()
                 .HasForeignKey(p => p.AlumnoId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(p => new { p.UsuarioId, p.AlumnoId }).IsUnique();
            });
            // ── Notificaciones ───────────────────────────────────────
            modelBuilder.Entity<Notificacion>(e =>
            {
                e.ToTable("Notificaciones");
                e.HasKey(n => n.NotificacionId);
                e.Property(n => n.Titulo).IsRequired().HasMaxLength(100);
                e.Property(n => n.Mensaje).IsRequired().HasMaxLength(500);
                e.Property(n => n.Tipo).IsRequired().HasMaxLength(30);

                e.HasOne(n => n.Usuario)
                 .WithMany()
                 .HasForeignKey(n => n.UsuarioId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(n => n.Alumno)
                 .WithMany()
                 .HasForeignKey(n => n.AlumnoId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}