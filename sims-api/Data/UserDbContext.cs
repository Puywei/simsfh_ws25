using Microsoft.EntityFrameworkCore;
using sims.Models;

namespace sims.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        // This creates the tables using EF
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<BlacklistedToken> BlacklistedTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Uid);
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Firstname).IsRequired().HasMaxLength(32);
                entity.Property(u => u.Lastname).IsRequired().HasMaxLength(32);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(64);
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
                entity.Property(u => u.CreationDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();

                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.RoleId);
                entity.HasIndex(r => r.RoleName).IsUnique();

                entity.Property(r => r.RoleName).IsRequired().HasMaxLength(50);
            });


            //Configure BlacklistedToken entity
            modelBuilder.Entity<BlacklistedToken>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.Token)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(b => b.Expiry)
                    .IsRequired();

                entity.HasIndex(b => b.Token).IsUnique();
            });

            // Seed roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin", Description = "Administrator - full access" },
                new Role { RoleId = 2, RoleName = "User", Description = "Regular user - regular access" }
            );
        }

        public void Seed()
        {
            Database.Migrate();

            // Seed Default (Example) users
            if (!Users.Any())
            {
                Users.AddRange(
                    new User
                    {
                        Uid = 1,
                        Firstname = "Default",
                        Lastname = "Administrator",
                        Email = "admin@admin.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("passme123!"),
                        RoleId = 1
                    },
                    new User
                    {
                        Uid = 2,
                        Firstname = "Max",
                        Lastname = "Mustermann",
                        Email = "max@mustermann.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("passme123!"),
                        RoleId = 2
                    },
                    new User
                    {
                        Uid = 3,
                        Firstname = "Maria",
                        Lastname = "Musterfrau",
                        Email = "maria@musterfrau.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("passme123!"),
                        RoleId = 2
                    }
                );
                SaveChanges();
            }
        }
    }
}