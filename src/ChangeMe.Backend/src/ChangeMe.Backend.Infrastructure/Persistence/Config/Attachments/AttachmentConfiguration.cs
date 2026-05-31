using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.Domain.Common.Attachments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Attachments;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
  public void Configure(EntityTypeBuilder<Attachment> builder)
  {
    builder.ToTable("attachments");
    builder.HasDiscriminator<string>("AttachmentType")
      .HasValue<IssueAttachment>("Issue");

    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .ValueGeneratedNever();

    builder.Property(e => e.CreatedAt)
      .IsRequired();

    builder.Property(e => e.UpdatedAt);

    builder.Property(e => e.IsDeleted)
      .HasDefaultValue(false);

    builder.Property(e => e.CreatedBy)
      .IsRequired();

    builder.Property(e => e.UpdatedBy);

    builder.HasIndex(e => e.Id);

    builder.HasQueryFilter(e => !e.IsDeleted);

    builder.Property(a => a.StorageContainer)
      .IsRequired()
      .HasMaxLength(AttachmentConstraints.STORAGE_CONTAINER_MAX_LENGTH);

    builder.Property(a => a.OwnerId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(a => a.OriginalFileName)
      .IsRequired()
      .HasMaxLength(AttachmentConstraints.ORIGINAL_FILE_NAME_MAX_LENGTH);

    builder.Property(a => a.ContentType)
      .IsRequired()
      .HasMaxLength(AttachmentConstraints.CONTENT_TYPE_MAX_LENGTH);

    builder.Property(a => a.SizeBytes)
      .IsRequired();

    builder.Property(a => a.StorageKey)
      .IsRequired()
      .HasMaxLength(AttachmentConstraints.STORAGE_KEY_MAX_LENGTH);

    builder.Property(a => a.Status)
      .IsRequired()
      .HasConversion<string>();

    builder.HasIndex(a => a.OwnerId);
    builder.HasIndex(a => new { a.StorageContainer, a.OwnerId, a.StorageKey }).IsUnique();
    builder.HasIndex(a => new { a.Status, a.CreatedAt });
  }
}

public class IssueAttachmentConfiguration : IEntityTypeConfiguration<IssueAttachment>
{
  public void Configure(EntityTypeBuilder<IssueAttachment> builder)
  {
    builder.HasOne<Issue>()
      .WithMany(i => i.Attachments)
      .HasForeignKey(a => a.OwnerId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
