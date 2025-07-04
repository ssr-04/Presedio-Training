﻿// <auto-generated />
using System;
using FreelanceProjectBoardApi.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FreelanceProjectBoardApi.Migrations
{
    [DbContext(typeof(FreelanceContext))]
    partial class FreelanceContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.ClientProfile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasMaxLength(75)
                        .HasColumnType("character varying(75)");

                    b.Property<string>("ContactPersonName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Location")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("ClientProfiles");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.File", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Category")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<Guid?>("ProjectId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ProposalId")
                        .HasColumnType("uuid");

                    b.Property<string>("StoredFileName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.Property<Guid>("UploaderId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.HasIndex("ProposalId");

                    b.HasIndex("UploaderId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.FreelancerProfile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Bio")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ExperienceLevel")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Headline")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<decimal?>("HourlyRate")
                        .HasColumnType("numeric");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("PortfolioUrl")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<Guid?>("ProfilePictureFileId")
                        .HasColumnType("uuid");

                    b.Property<int>("ProjectsCompleted")
                        .HasColumnType("integer");

                    b.Property<Guid?>("ResumeFileId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ProfilePictureFileId");

                    b.HasIndex("ResumeFileId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("FreelancerProfiles");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.FreelancerSkill", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("FreelancerProfileId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("SkillId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FreelancerProfileId");

                    b.HasIndex("SkillId");

                    b.ToTable("FreelancerSkills");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.Notification", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Category")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsRead")
                        .HasColumnType("boolean");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ReceiverId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ReceiverId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.Project", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AssignedFreelancerId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Budget")
                        .HasColumnType("numeric");

                    b.Property<Guid>("ClientId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ClientProfileId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("CompletionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("Deadline")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AssignedFreelancerId");

                    b.HasIndex("ClientId");

                    b.HasIndex("ClientProfileId");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.ProjectSkill", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SkillId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.HasIndex("SkillId");

                    b.ToTable("ProjectSkills");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.Proposal", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CoverLetter")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("FreelancerId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("FreelancerProfileId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("ProposedBudget")
                        .HasColumnType("numeric");

                    b.Property<DateTime?>("ProposedDeadLine")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FreelancerId");

                    b.HasIndex("FreelancerProfileId");

                    b.HasIndex("ProjectId");

                    b.ToTable("Proposals");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.Rating", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Comment")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("FreelancerProfileId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RateeId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RaterId")
                        .HasColumnType("uuid");

                    b.Property<int>("RatingValue")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FreelancerProfileId");

                    b.HasIndex("ProjectId");

                    b.HasIndex("RateeId");

                    b.HasIndex("RaterId");

                    b.ToTable("Ratings");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.Skill", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Skills");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(75)
                        .HasColumnType("character varying(75)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(265)
                        .HasColumnType("character varying(265)");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("text");

                    b.Property<DateTime>("RefreshTokenExpiryTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.ClientProfile", b =>
                {
                    b.HasOne("FreelanceProjectBoardApi.Models.User", "User")
                        .WithOne("ClientProfile")
                        .HasForeignKey("FreelanceProjectBoardApi.Models.ClientProfile", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.File", b =>
                {
                    b.HasOne("FreelanceProjectBoardApi.Models.Project", "Project")
                        .WithMany("Attachments")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("FreelanceProjectBoardApi.Models.Proposal", "Proposal")
                        .WithMany("Attachments")
                        .HasForeignKey("ProposalId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("FreelanceProjectBoardApi.Models.User", "Uploader")
                        .WithMany("UploadedFiles")
                        .HasForeignKey("UploaderId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("Proposal");

                    b.Navigation("Uploader");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.FreelancerProfile", b =>
                {
                    b.HasOne("FreelanceProjectBoardApi.Models.File", "ProfilePictureFile")
                        .WithMany()
                        .HasForeignKey("ProfilePictureFileId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("FreelanceProjectBoardApi.Models.File", "ResumeFile")
                        .WithMany()
                        .HasForeignKey("ResumeFileId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("FreelanceProjectBoardApi.Models.User", "User")
                        .WithOne("FreelancerProfile")
                        .HasForeignKey("FreelanceProjectBoardApi.Models.FreelancerProfile", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProfilePictureFile");

                    b.Navigation("ResumeFile");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.FreelancerSkill", b =>
                {
                    b.HasOne("FreelanceProjectBoardApi.Models.FreelancerProfile", "FreelancerProfile")
                        .WithMany("FreelancerSkills")
                        .HasForeignKey("FreelancerProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FreelanceProjectBoardApi.Models.Skill", "Skill")
                        .WithMany()
                        .HasForeignKey("SkillId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FreelancerProfile");

                    b.Navigation("Skill");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.Notification", b =>
                {
                    b.HasOne("FreelanceProjectBoardApi.Models.User", "Receiver")
                        .WithMany()
                        .HasForeignKey("ReceiverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Receiver");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.Project", b =>
                {
                    b.HasOne("FreelanceProjectBoardApi.Models.User", "AssignedFreelancer")
                        .WithMany("AssignedProjects")
                        .HasForeignKey("AssignedFreelancerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("FreelanceProjectBoardApi.Models.User", "Client")
                        .WithMany("PostedProjects")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("FreelanceProjectBoardApi.Models.ClientProfile", null)
                        .WithMany("PostedProjects")
                        .HasForeignKey("ClientProfileId");

                    b.Navigation("AssignedFreelancer");

                    b.Navigation("Client");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.ProjectSkill", b =>
                {
                    b.HasOne("FreelanceProjectBoardApi.Models.Project", "Project")
                        .WithMany("ProjectSkills")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FreelanceProjectBoardApi.Models.Skill", "Skill")
                        .WithMany()
                        .HasForeignKey("SkillId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("Skill");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.Proposal", b =>
                {
                    b.HasOne("FreelanceProjectBoardApi.Models.User", "Freelancer")
                        .WithMany("Proposals")
                        .HasForeignKey("FreelancerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("FreelanceProjectBoardApi.Models.FreelancerProfile", null)
                        .WithMany("SubmittedProposals")
                        .HasForeignKey("FreelancerProfileId");

                    b.HasOne("FreelanceProjectBoardApi.Models.Project", "Project")
                        .WithMany("Proposals")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Freelancer");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.Rating", b =>
                {
                    b.HasOne("FreelanceProjectBoardApi.Models.FreelancerProfile", null)
                        .WithMany("RatingsAsRatee")
                        .HasForeignKey("FreelancerProfileId");

                    b.HasOne("FreelanceProjectBoardApi.Models.Project", "Project")
                        .WithMany("Ratings")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FreelanceProjectBoardApi.Models.User", "Ratee")
                        .WithMany("RatingsReceived")
                        .HasForeignKey("RateeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("FreelanceProjectBoardApi.Models.User", "Rater")
                        .WithMany("RatingsGiven")
                        .HasForeignKey("RaterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("Ratee");

                    b.Navigation("Rater");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.ClientProfile", b =>
                {
                    b.Navigation("PostedProjects");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.FreelancerProfile", b =>
                {
                    b.Navigation("FreelancerSkills");

                    b.Navigation("RatingsAsRatee");

                    b.Navigation("SubmittedProposals");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.Project", b =>
                {
                    b.Navigation("Attachments");

                    b.Navigation("ProjectSkills");

                    b.Navigation("Proposals");

                    b.Navigation("Ratings");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.Proposal", b =>
                {
                    b.Navigation("Attachments");
                });

            modelBuilder.Entity("FreelanceProjectBoardApi.Models.User", b =>
                {
                    b.Navigation("AssignedProjects");

                    b.Navigation("ClientProfile");

                    b.Navigation("FreelancerProfile");

                    b.Navigation("PostedProjects");

                    b.Navigation("Proposals");

                    b.Navigation("RatingsGiven");

                    b.Navigation("RatingsReceived");

                    b.Navigation("UploadedFiles");
                });
#pragma warning restore 612, 618
        }
    }
}
