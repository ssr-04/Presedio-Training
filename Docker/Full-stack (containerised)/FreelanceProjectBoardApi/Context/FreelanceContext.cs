using System.Security.Claims;
using FreelanceProjectBoardApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FreelanceProjectBoardApi.Context
{
    public class FreelanceContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor; 
        public FreelanceContext(DbContextOptions<FreelanceContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor; 
        }
        

        public DbSet<User> Users { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }
        public DbSet<FreelancerProfile> FreelancerProfiles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<Models.File> Files { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<FreelancerSkill> FreelancerSkills { get; set; }
        public DbSet<ProjectSkill> ProjectSkills { get; set; }

         public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique(); // Email is unique

                // One to One relationship with ClientProfile
                entity.HasOne(u => u.ClientProfile)
                        .WithOne(cp => cp.User)
                        .HasForeignKey<ClientProfile>(cp => cp.UserId)
                        .OnDelete(DeleteBehavior.Cascade); // If a user is deleted the client profile is too deleted

                // One to One with the FreelancerProfile
                entity.HasOne(u => u.FreelancerProfile)
                        .WithOne(fp => fp.User)
                        .HasForeignKey<FreelancerProfile>(fp => fp.UserId)
                        .OnDelete(DeleteBehavior.Cascade); // If a user is deleted the freelancer profile is too deleted

                // Relation for the Projects posted by client
                entity.HasMany(u => u.PostedProjects)
                      .WithOne(p => p.Client)
                      .HasForeignKey(p => p.ClientId)
                      .OnDelete(DeleteBehavior.Restrict); // Not to cascade delete projects when client deleted (atleast in our case)

                // Relation for the Assigned Freelancer
                entity.HasMany(u => u.AssignedProjects)
                      .WithOne(p => p.AssignedFreelancer)
                      .HasForeignKey(p => p.AssignedFreelancerId)
                      .OnDelete(DeleteBehavior.Restrict); // Not to delete project if the freelancer is deleted

                // Relation for the Proposals by freelancers
                entity.HasMany(u => u.Proposals)
                      .WithOne(pr => pr.Freelancer)
                      .HasForeignKey(pr => pr.FreelancerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relations for ratings given by user
                entity.HasMany(u => u.RatingsGiven)
                      .WithOne(r => r.Rater)
                      .HasForeignKey(r => r.RaterId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relations for ratings received by user
                entity.HasMany(u => u.RatingsReceived)
                      .WithOne(r => r.Ratee)
                      .HasForeignKey(r => r.RateeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relation for files associated with user
                entity.HasMany(u => u.UploadedFiles)
                      .WithOne(f => f.Uploader)
                      .HasForeignKey(f => f.UploaderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 2) Freelancer Profile
            modelBuilder.Entity<FreelancerProfile>(entity =>
            {
                // Realtions with the File (Resume)
                entity.HasOne(fp => fp.ResumeFile)
                    .WithMany()
                    .HasForeignKey(fp => fp.ResumeFileId)
                    .OnDelete(DeleteBehavior.SetNull); // If file deleted then setting to null

                // Relation with the Profile Picture
                entity.HasOne(fp => fp.ProfilePictureFile)
                    .WithMany()
                    .HasForeignKey(fp => fp.ProfilePictureFileId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // 3) Project
            modelBuilder.Entity<Project>(entity =>
            {
                // FK to Project - already defined in Project
                // FK to Freelancer - already defined in User

                // One to Many with the Proposals
                entity.HasMany(p => p.Proposals)
                    .WithOne(pr => pr.Project)
                    .HasForeignKey(pr => pr.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade); // If project deleted then deleting associated proposals

                // Many to Many with Project attachments
                entity.HasMany(p => p.Attachments)
                    .WithOne(f => f.Project)
                    .HasForeignKey(f => f.ProjectId)
                    .OnDelete(DeleteBehavior.SetNull); // if project deleted then just setting ProjectId in File to null
            });

            // 4) Proposal
            modelBuilder.Entity<Proposal>(entity =>
            {
                // FK to Project already in Project
                // FK to Freelancer already in User

                // many to many with file (proposal attachment)
                entity.HasMany(pr => pr.Attachments)
                    .WithOne(f => f.Proposal)
                    .HasForeignKey(f => f.ProposalId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // 5) File
            modelBuilder.Entity<Models.File>(entity =>
            {
                // FK to Uploader (user) already in user
                // FK to Project (ProjectId) already in Project
                // Fk to Proposal (ProposalId) already in proposal
            });

            // 6) Rating
            modelBuilder.Entity<Rating>(entity =>
            {
                // FK to Project already in Project
                // FK to Rater (user) already in user
                // FK to Ratee (user) already in user
            });

            // 7) Skill
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasIndex(s => s.Name).IsUnique(); // Skills should not be duplicate
            });

            // 8) Freelancer skill (many to many b/w freelancerProfile and skills)
            modelBuilder.Entity<FreelancerSkill>(entity =>
            {
                entity.HasOne(fs => fs.FreelancerProfile)
                    .WithMany(fp => fp.FreelancerSkills)
                    .HasForeignKey(fs => fs.FreelancerProfileId)
                    .OnDelete(DeleteBehavior.Cascade); // If freelancer profile deleted then removing skill mapping (not skill)

                entity.HasOne(fs => fs.Skill)
                    .WithMany()
                    .HasForeignKey(fs => fs.SkillId)
                    .OnDelete(DeleteBehavior.Cascade); // If skill deleted removing associations
            });

            // 9. Project skill (many to many b/w project and skills)
            modelBuilder.Entity<ProjectSkill>(entity =>
            {
                entity.HasOne(ps => ps.Project)
                      .WithMany(p => p.ProjectSkills)
                      .HasForeignKey(ps => ps.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade); // If project is deleted, removing its skill requirements

                entity.HasOne(ps => ps.Skill)
                      .WithMany()
                      .HasForeignKey(ps => ps.SkillId)
                      .OnDelete(DeleteBehavior.Cascade); // If a skill is deleted, removing the associations
            });

            //10. Notifications
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Receiver)
                .WithMany() // A user can have many notifications, but a notification has one receiver.
                .HasForeignKey(n => n.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // Over riding Savechanges to auto update audit columns
        public override int SaveChanges()
        {
            ApplyAuditInformation();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return base.SaveChangesAsync(cancellationToken);
        }

        public void ApplyAuditInformation()
        {
            // Getting current authenticated user ID
            string? currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    // Sets CreatedBy if a user ID is available, otherwise "System"
                    entry.Entity.CreatedBy = currentUserId ?? "System";
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // Sets UpdatedBy if a user ID is available
                    entry.Entity.UpdatedBy = currentUserId ?? "System"; // Is system if no user is authenticated
                }
            }
        }
    }
}