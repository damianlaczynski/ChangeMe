using ChangeMe.Backend.Domain.Aggregates.Invitations;
using ChangeMe.Backend.Domain.Aggregates.Invitations.Entities;
using ChangeMe.Backend.Domain.Aggregates.Invitations.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Invitations;

public class InvitationConfiguration : BaseEntityTypeConfiguration<Invitation>
{
  protected override string TableName => "invitations";

  public override void Configure(EntityTypeBuilder<Invitation> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.Email)
      .IsRequired()
      .HasMaxLength(InvitationConstraints.EMAIL_MAX_LENGTH);

    builder.Property(x => x.NormalizedEmail)
      .IsRequired()
      .HasMaxLength(InvitationConstraints.EMAIL_MAX_LENGTH);

    builder.Property(x => x.FirstName)
      .HasMaxLength(InvitationConstraints.NAME_MAX_LENGTH);

    builder.Property(x => x.LastName)
      .HasMaxLength(InvitationConstraints.NAME_MAX_LENGTH);

    builder.Property(x => x.InvitedByUserId)
      .IsRequired();

    builder.Property(x => x.Status)
      .IsRequired()
      .HasConversion<string>()
      .HasMaxLength(32);

    builder.Property(x => x.ExpiresAt)
      .IsRequired();

    builder.Property(x => x.AcceptedAt);

    builder.Property(x => x.TokenHash)
      .IsRequired()
      .HasMaxLength(128);

    builder.HasIndex(x => x.NormalizedEmail);
    builder.HasIndex(x => x.Status);
    builder.HasIndex(x => x.ExpiresAt);
    builder.HasIndex(x => x.TokenHash)
      .IsUnique();

    builder.HasMany(x => x.Roles)
      .WithOne()
      .HasForeignKey(x => x.InvitationId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(x => x.Roles)
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

public class InvitationRoleConfiguration : IEntityTypeConfiguration<InvitationRole>
{
  public void Configure(EntityTypeBuilder<InvitationRole> builder)
  {
    builder.ToTable("invitation_roles");

    builder.HasKey(x => new { x.InvitationId, x.RoleId });
  }
}
