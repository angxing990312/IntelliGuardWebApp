using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace maddweb.Models
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Entry> Entry { get; set; }
        public virtual DbSet<MaddUser> MaddUser { get; set; }

  
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entry>(entity =>
            {
                entity.Property(e => e.EntryID).HasColumnName("EntryID");

                entity.Property(e => e.EntryTime).HasColumnType("datetime");

                entity.Property(e => e.Photo)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UserID).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Entry)
                    .HasForeignKey(d => d.UserID)
                    .HasConstraintName("FK1");
            });

            modelBuilder.Entity<MaddUser>(entity =>
            {
                entity.HasKey(e => e.UserID)
                    .HasName("PK__MaddUser__1788CCACC81FFADB");

                entity.Property(e => e.UserID).HasColumnName("UserID");

                entity.Property(e => e.FullName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Role)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.UPhotoPath)
                    .HasColumnName("UPhotoPath")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.UserPw)
                    .IsRequired()
                    .HasMaxLength(45)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
