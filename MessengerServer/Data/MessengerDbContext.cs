using MessengerServer.Models;
using Microsoft.EntityFrameworkCore;

namespace MessengerServer.Data;

public class MessengerDbContext : DbContext
{
    public MessengerDbContext(DbContextOptions<MessengerDbContext> options) : base(options) { }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}