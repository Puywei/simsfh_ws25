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
            
            /// Seed Default Admin and some test users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Uid = 1,
                    Firstname = "Default",
                    Lastname = "Administrator",
                    Email = "admin@admin.com",
                    PasswordHash = "$2b$10$4Xt2A0AFTwspkNlpVPZNmuZJsydGp3pfLi2.YBoGxL3T7ShwCgZLS",
                    RoleId = 1
                },
                 new User
                {
                    Uid = 2,
                    Firstname = "Max",
                    Lastname = "Mustermann",
                    Email = "max@mustermann.com",
                    PasswordHash = "$2b$10$DHsquTrVit/1zvSpEr89Sepkj0KTKRtbxU7PJ0LejsRNUkAQPwYQy",
                    RoleId = 2
                },
                 new User
                 {
                     Uid = 3,
                     Firstname = "Maria",
                     Lastname = "Musterfrau",
                     Email = "maria@musterfrau.com",
                     PasswordHash = "$2b$10$qwiR8mn2wbeUxAH3SoMlo.oTtpObxaqPHhQsdp10kyF8TE7QdO9WS",
                     RoleId = 2
                 }
                
            );

        }
    }
}