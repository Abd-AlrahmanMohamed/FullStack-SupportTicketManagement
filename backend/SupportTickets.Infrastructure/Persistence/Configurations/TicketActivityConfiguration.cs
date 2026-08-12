using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Infrastructure.Persistence.Configurations;

public class TicketActivityConfiguration : IEntityTypeConfiguration<TicketActivity>
{
    public void Configure(EntityTypeBuilder<TicketActivity> builder)
    {
        builder.ToTable("TicketActivities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.OldValue).HasMaxLength(500);
        builder.Property(a => a.NewValue).HasMaxLength(500);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.TicketId);
    }
}
