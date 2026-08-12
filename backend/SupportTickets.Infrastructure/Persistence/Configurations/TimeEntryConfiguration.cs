using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Infrastructure.Persistence.Configurations;

public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("TimeEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DurationMinutes).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasOne(e => e.Agent)
            .WithMany()
            .HasForeignKey(e => e.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TicketId);
    }
}
