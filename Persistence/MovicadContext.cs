using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Movicad.Persistence;

public partial class MovicadContext : DbContext
{
    public MovicadContext()
    {
    }

    public MovicadContext(DbContextOptions<MovicadContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Administrative> Administratives { get; set; }

    public virtual DbSet<AdministrativeMessage> AdministrativeMessages { get; set; }

    public virtual DbSet<AdministrativeMessageFile> AdministrativeMessageFiles { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<CallsFor> CallsFors { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<DestinationCountry> DestinationCountries { get; set; }

    public virtual DbSet<ExpulsionReason> ExpulsionReasons { get; set; }

    public virtual DbSet<FileRequest> FileRequests { get; set; }

    public virtual DbSet<ForumFile> ForumFiles { get; set; }

    public virtual DbSet<ForumMessage> ForumMessages { get; set; }

    public virtual DbSet<FrequentQuestion> FrequentQuestions { get; set; }

    public virtual DbSet<Link> Links { get; set; }

    public virtual DbSet<PrivateMessage> PrivateMessages { get; set; }

    public virtual DbSet<Problem> Problems { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentFile> StudentFiles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var password = Environment.GetEnvironmentVariable("FINAL_PASS");
        optionsBuilder.UseNpgsql($"Host=localhost;Username=final;Password={password};Database=movicad");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrative>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("administratives_pkey");

            entity.ToTable("administratives");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Acronym)
                .HasMaxLength(100)
                .HasColumnName("acronym");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.Icon).HasColumnName("icon");
            entity.Property(e => e.IconMediaType).HasColumnName("icon_media_type");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Website).HasColumnName("website");

            entity.HasOne(d => d.Country).WithMany(p => p.Administratives)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("administratives_country_id_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.Administrative)
                .HasForeignKey<Administrative>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("administratives_user_id_fkey");
        });

        modelBuilder.Entity<AdministrativeMessage>(entity =>
        {
            entity.HasKey(e => e.AdministrativeMessageId).HasName("administrative_messages_pkey");

            entity.ToTable("administrative_messages");

            entity.HasIndex(e => e.Key, "administrative_messages_key_key").IsUnique();

            entity.Property(e => e.AdministrativeMessageId).HasColumnName("administrative_message_id");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.Creation).HasColumnName("creation");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Key)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("key");
            entity.Property(e => e.RecipientId).HasColumnName("recipient_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Subject)
                .HasMaxLength(150)
                .HasColumnName("subject");

            entity.HasOne(d => d.Recipient).WithMany(p => p.AdministrativeMessageRecipients)
                .HasForeignKey(d => d.RecipientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("administrative_messages_recipient_id_fkey");

            entity.HasOne(d => d.Sender).WithMany(p => p.AdministrativeMessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("administrative_messages_sender_id_fkey");
        });

        modelBuilder.Entity<AdministrativeMessageFile>(entity =>
        {
            entity.HasKey(e => e.AdministrativeMessageFileId).HasName("administrative_message_files_pkey");

            entity.ToTable("administrative_message_files");

            entity.HasIndex(e => e.Key, "administrative_message_files_key_key").IsUnique();

            entity.Property(e => e.AdministrativeMessageFileId).HasColumnName("administrative_message_file_id");
            entity.Property(e => e.AdministrativeMessageId).HasColumnName("administrative_message_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Key)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("key");
            entity.Property(e => e.MediaType).HasColumnName("media_type");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasOne(d => d.AdministrativeMessage).WithMany(p => p.AdministrativeMessageFiles)
                .HasForeignKey(d => d.AdministrativeMessageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("administrative_message_files_administrative_message_id_fkey");
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("applications_pkey");

            entity.ToTable("applications");

            entity.HasIndex(e => new { e.StudentId, e.CallForId }, "applications_student_id_call_for_id_key").IsUnique();

            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.Banned).HasColumnName("banned");
            entity.Property(e => e.CallForId).HasColumnName("call_for_id");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.CallFor).WithMany(p => p.Applications)
                .HasForeignKey(d => d.CallForId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("applications_call_for_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.Applications)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("applications_student_id_fkey");
        });

        modelBuilder.Entity<CallsFor>(entity =>
        {
            entity.HasKey(e => e.CallForId).HasName("calls_for_pkey");

            entity.ToTable("calls_for");

            entity.HasIndex(e => e.Key, "calls_for_key_key").IsUnique();

            entity.Property(e => e.CallForId).HasColumnName("call_for_id");
            entity.Property(e => e.AdministrativeId).HasColumnName("administrative_id");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FinalDate).HasColumnName("final_date");
            entity.Property(e => e.InitialDate).HasColumnName("initial_date");
            entity.Property(e => e.Key)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("key");
            entity.Property(e => e.Modification).HasColumnName("modification");
            entity.Property(e => e.PublishDate).HasColumnName("publish_date");
            entity.Property(e => e.Requirements).HasColumnName("requirements");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Administrative).WithMany(p => p.CallsFors)
                .HasForeignKey(d => d.AdministrativeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("calls_for_administrative_id_fkey");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.CountryId).HasName("countries_pkey");

            entity.ToTable("countries");

            entity.HasIndex(e => e.Name, "countries_name_key").IsUnique();

            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<DestinationCountry>(entity =>
        {
            entity.HasKey(e => new { e.CallForId, e.CountryId }).HasName("destination_countries_pkey");

            entity.ToTable("destination_countries");

            entity.Property(e => e.CallForId).HasColumnName("call_for_id");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.Deleted).HasColumnName("deleted");

            entity.HasOne(d => d.CallFor).WithMany(p => p.DestinationCountries)
                .HasForeignKey(d => d.CallForId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("destination_countries_call_for_id_fkey");

            entity.HasOne(d => d.Country).WithMany(p => p.DestinationCountries)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("destination_countries_country_id_fkey");
        });

        modelBuilder.Entity<ExpulsionReason>(entity =>
        {
            entity.HasKey(e => e.ExpulsionReasonId).HasName("expulsion_reasons_pkey");

            entity.ToTable("expulsion_reasons");

            entity.HasIndex(e => e.Key, "expulsion_reasons_key_key").IsUnique();

            entity.Property(e => e.ExpulsionReasonId).HasColumnName("expulsion_reason_id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.Content)
                .HasMaxLength(300)
                .HasColumnName("content");
            entity.Property(e => e.Creation).HasColumnName("creation");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Key)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("key");

            entity.HasOne(d => d.Application).WithMany(p => p.ExpulsionReasons)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("expulsion_reasons_application_id_fkey");
        });

        modelBuilder.Entity<FileRequest>(entity =>
        {
            entity.HasKey(e => e.FileRequestId).HasName("file_requests_pkey");

            entity.ToTable("file_requests");

            entity.HasIndex(e => e.Key, "file_requests_key_key").IsUnique();

            entity.Property(e => e.FileRequestId).HasColumnName("file_request_id");
            entity.Property(e => e.CallForId).HasColumnName("call_for_id");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Key)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("key");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");

            entity.HasOne(d => d.CallFor).WithMany(p => p.FileRequests)
                .HasForeignKey(d => d.CallForId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("file_requests_call_for_id_fkey");
        });

        modelBuilder.Entity<ForumFile>(entity =>
        {
            entity.HasKey(e => e.ForumFileId).HasName("forum_files_pkey");

            entity.ToTable("forum_files");

            entity.HasIndex(e => e.Key, "forum_files_key_key").IsUnique();

            entity.Property(e => e.ForumFileId).HasColumnName("forum_file_id");
            entity.Property(e => e.CallForId).HasColumnName("call_for_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Key)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("key");
            entity.Property(e => e.MediaType).HasColumnName("media_type");
            entity.Property(e => e.Modification).HasColumnName("modification");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");

            entity.HasOne(d => d.CallFor).WithMany(p => p.ForumFiles)
                .HasForeignKey(d => d.CallForId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("forum_files_call_for_id_fkey");
        });

        modelBuilder.Entity<ForumMessage>(entity =>
        {
            entity.HasKey(e => e.ForumMessageId).HasName("forum_messages_pkey");

            entity.ToTable("forum_messages");

            entity.HasIndex(e => e.Key, "forum_messages_key_key").IsUnique();

            entity.Property(e => e.ForumMessageId).HasColumnName("forum_message_id");
            entity.Property(e => e.AttachedImage).HasColumnName("attached_image");
            entity.Property(e => e.CallForId).HasColumnName("call_for_id");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content");
            entity.Property(e => e.Creation).HasColumnName("creation");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.ImageMediaType).HasColumnName("image_media_type");
            entity.Property(e => e.Key)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("key");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.CallFor).WithMany(p => p.ForumMessages)
                .HasForeignKey(d => d.CallForId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("forum_messages_call_for_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ForumMessages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("forum_messages_user_id_fkey");
        });

        modelBuilder.Entity<FrequentQuestion>(entity =>
        {
            entity.HasKey(e => e.FrequentQuestionId).HasName("frequent_questions_pkey");

            entity.ToTable("frequent_questions");

            entity.HasIndex(e => e.Key, "frequent_questions_key_key").IsUnique();

            entity.Property(e => e.FrequentQuestionId).HasColumnName("frequent_question_id");
            entity.Property(e => e.Answer).HasColumnName("answer");
            entity.Property(e => e.CallForId).HasColumnName("call_for_id");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Key)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("key");
            entity.Property(e => e.Question)
                .HasMaxLength(150)
                .HasColumnName("question");

            entity.HasOne(d => d.CallFor).WithMany(p => p.FrequentQuestions)
                .HasForeignKey(d => d.CallForId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("frequent_questions_call_for_id_fkey");
        });

        modelBuilder.Entity<Link>(entity =>
        {
            entity.HasKey(e => e.LinkId).HasName("links_pkey");

            entity.ToTable("links");

            entity.HasIndex(e => e.Key, "links_key_key").IsUnique();

            entity.Property(e => e.LinkId).HasColumnName("link_id");
            entity.Property(e => e.CallForId).HasColumnName("call_for_id");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Key)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("key");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");
            entity.Property(e => e.Url).HasColumnName("url");

            entity.HasOne(d => d.CallFor).WithMany(p => p.Links)
                .HasForeignKey(d => d.CallForId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("links_call_for_id_fkey");
        });

        modelBuilder.Entity<PrivateMessage>(entity =>
        {
            entity.HasKey(e => e.PrivateMessageId).HasName("private_messages_pkey");

            entity.ToTable("private_messages");

            entity.HasIndex(e => e.Key, "private_messages_key_key").IsUnique();

            entity.Property(e => e.PrivateMessageId).HasColumnName("private_message_id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.AttachedImage).HasColumnName("attached_image");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content");
            entity.Property(e => e.Creation).HasColumnName("creation");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.ImageMediaType).HasColumnName("image_media_type");
            entity.Property(e => e.Key)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("key");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Application).WithMany(p => p.PrivateMessages)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("private_messages_application_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.PrivateMessages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("private_messages_user_id_fkey");
        });

        modelBuilder.Entity<Problem>(entity =>
        {
            entity.HasKey(e => e.ProblemId).HasName("problems_pkey");

            entity.ToTable("problems");

            entity.Property(e => e.ProblemId).HasColumnName("problem_id");
            entity.Property(e => e.AuthorEmail)
                .HasMaxLength(200)
                .HasColumnName("author_email");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.Creation).HasColumnName("creation");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Subject)
                .HasMaxLength(150)
                .HasColumnName("subject");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("students_pkey");

            entity.ToTable("students");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Icon).HasColumnName("icon");
            entity.Property(e => e.IconMediaType).HasColumnName("icon_media_type");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("students_user_id_fkey");
        });

        modelBuilder.Entity<StudentFile>(entity =>
        {
            entity.HasKey(e => new { e.ApplicationId, e.FileRequestId }).HasName("student_files_pkey");

            entity.ToTable("student_files");

            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.FileRequestId).HasColumnName("file_request_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.MediaType).HasColumnName("media_type");
            entity.Property(e => e.Modification).HasColumnName("modification");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasOne(d => d.Application).WithMany(p => p.StudentFiles)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_files_application_id_fkey");

            entity.HasOne(d => d.FileRequest).WithMany(p => p.StudentFiles)
                .HasForeignKey(d => d.FileRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_files_file_request_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Key, "users_key_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Creation).HasColumnName("creation");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Key)
                .HasMaxLength(50)
                .HasColumnName("key");
            entity.Property(e => e.Password)
                .HasMaxLength(60)
                .IsFixedLength()
                .HasColumnName("password");
            entity.Property(e => e.Role).HasColumnName("role");

            entity.HasMany(d => d.Administratives).WithMany(p => p.Senders)
                .UsingEntity<Dictionary<string, object>>(
                    "Rating",
                    r => r.HasOne<Administrative>().WithMany()
                        .HasForeignKey("AdministrativeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("ratings_administrative_id_fkey"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("ratings_sender_id_fkey"),
                    j =>
                    {
                        j.HasKey("SenderId", "AdministrativeId").HasName("ratings_pkey");
                        j.ToTable("ratings");
                        j.IndexerProperty<long>("SenderId").HasColumnName("sender_id");
                        j.IndexerProperty<long>("AdministrativeId").HasColumnName("administrative_id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
