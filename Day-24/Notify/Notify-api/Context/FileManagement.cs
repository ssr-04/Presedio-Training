
using Microsoft.EntityFrameworkCore;

namespace NotifyService.Data
{
    public class FileManagement : DbContext
    {
        public FileManagement(DbContextOptions<FileManagement> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<FileData> Files { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- User Entity Configuration ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id); // Primary key

                entity.Property(e => e.Username)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(e => e.Auth0UserId)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.HasIndex(e => e.Auth0UserId)
                      .IsUnique();

                entity.Property(e => e.Role)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true); // Default value

                entity.Property(e => e.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("NOW()"); 
            });

            // --- FileData Entity Configuration ---
            modelBuilder.Entity<FileData>(entity =>
            {
                entity.HasKey(e => e.Id); // Primary key is the GUID string

                entity.Property(e => e.Id)
                      .IsRequired()
                      .HasMaxLength(36);

                entity.Property(e => e.OriginalFileName)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.ContentType)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.FileSize)
                      .IsRequired();

                entity.Property(e => e.UploadedDate)
                      .IsRequired()
                      .HasDefaultValueSql("NOW()"); // current UTC time in Postgres

                entity.Property(e => e.UploadedByAuth0UserId)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.Description)
                      .HasMaxLength(1000); 

                entity.Property(e => e.Content)
                      .IsRequired(); 
            });
        }
    }
}