using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TicketNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(t => t.TicketNumber)
            .IsUnique();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.CreatedAt);

        builder.HasOne(t => t.Customer)
            .WithMany()
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedAgent)
            .WithMany()
            .HasForeignKey(t => t.AssignedAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Activities)
            .WithOne(a => a.Ticket)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.TimeEntries)
            .WithOne(e => e.Ticket)
            .HasForeignKey(e => e.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
