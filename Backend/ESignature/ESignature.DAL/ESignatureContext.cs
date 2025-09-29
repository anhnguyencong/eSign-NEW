using ESignature.DAL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Linq;

namespace ESignature.DAL
{
    public class ESignatureContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public ESignatureContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<JobHistory> JobHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            OnIdentityCreating(modelBuilder);
            OnIndexesCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
        private static void OnIndexesCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Job>().HasIndex(q => new
            {
                q.AppName,
                q.BatchId,
                q.RefId,
                q.CreatedDate,
                q.Status,
                q.Priority
            });
            modelBuilder.Entity<Media>().HasIndex(q => new { q.CreatedDate, q.JobFileType, q.Name });
        }

        private static void OnIdentityCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                // Each Role can have many associated RoleClaims
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}