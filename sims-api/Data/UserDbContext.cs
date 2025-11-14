using Microsoft.EntityFrameworkCore;
using sims.Models;

namespace sims.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        // Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

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
                entity.Property(u => u.PasswordHash).IsRequired();

                // Relationship: User → Role
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
            
            // Seed Default roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin", Description = "Administrator - full access" },
                new Role { RoleId = 2, RoleName = "User", Description = "Regular user - regular access" }
            );
        }
    }
}