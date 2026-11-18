using Gestion_randonnees.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gestion_randonnees.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Bus> Buses => Set<Bus>();
    public DbSet<Hike> Hikes => Set<Hike>();
    public DbSet<Participation> Participations => Set<Participation>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Nom).HasMaxLength(100).IsRequired();
            builder.Property(u => u.Email).HasMaxLength(150).IsRequired();
            builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            builder.Property(u => u.Telephone).HasMaxLength(20);
            builder.Property(u => u.ProfileImagePath).HasMaxLength(255);
            builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Bus>(builder =>
        {
            builder.ToTable("Buses");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Immatriculation).HasMaxLength(50).IsRequired();
            builder.Property(b => b.DriverName).HasMaxLength(100).IsRequired();
            builder.Property(b => b.DriverPhone).HasMaxLength(20).IsRequired();
            builder.Property(b => b.SeatCount).IsRequired();
        });

        modelBuilder.Entity<Hike>(builder =>
        {
            builder.ToTable("Hikes");
            builder.HasKey(h => h.Id);

            builder.Property(h => h.Categorie).HasMaxLength(50).IsRequired();
            builder.Property(h => h.Titre).HasMaxLength(150).IsRequired();
            builder.Property(h => h.Description).IsRequired();
            builder.Property(h => h.DateDebut).HasColumnType("datetime2").IsRequired();
            builder.Property(h => h.DateFin).HasColumnType("datetime2").IsRequired();
            builder.Property(h => h.Region).HasMaxLength(100).IsRequired();
            builder.Property(h => h.DistanceKm).IsRequired();
            builder.Property(h => h.Difficulte).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(h => h.Capacite).IsRequired();
            builder.Property(h => h.ImagePath).HasMaxLength(255);
            builder.Property(h => h.DateCreation).HasColumnType("datetime2").IsRequired();
            builder.Property(h => h.Statut).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(h => h.Prix).HasColumnType("decimal(10,2)").IsRequired();

            builder.HasIndex(h => h.DateDebut);
            builder.HasIndex(h => h.Region);
            builder.HasIndex(h => h.Statut);

            builder.HasOne(h => h.Bus)
                .WithMany(b => b.Hikes)
                .HasForeignKey(h => h.BusId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Participation>(builder =>
        {
            builder.ToTable("Participations");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Statut).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(p => p.IsSeenByUser).IsRequired();
            builder.Property(p => p.DateCreation).HasColumnType("datetime2").IsRequired();

            builder.HasIndex(p => new { p.UserId, p.HikeId }).IsUnique();
            builder.HasIndex(p => p.HikeId);
            builder.HasIndex(p => p.Statut);

            builder.HasOne(p => p.User)
                .WithMany(u => u.Participations)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Hike)
                .WithMany(h => h.Participations)
                .HasForeignKey(p => p.HikeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Favorite>(builder =>
        {
            builder.ToTable("Favorites");
            builder.HasKey(f => new { f.UserId, f.HikeId });

            builder.Property(f => f.DateCreation).HasColumnType("datetime2").IsRequired();

            builder.HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Hike)
                .WithMany(h => h.Favorites)
                .HasForeignKey(f => f.HikeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
