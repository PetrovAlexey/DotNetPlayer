using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Player.Models
{
    public sealed class UserContext : IdentityDbContext<User>
    {
        public UserContext(DbContextOptions<UserContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserAudio>()
                .HasKey(t => new { t.Id, t.AudioId });

            modelBuilder.Entity<UserAudio>()
                .HasOne(sc => sc.User)
                .WithMany(s => s.Audios)
                .HasForeignKey(sc => sc.Id);

            modelBuilder.Entity<UserAudio>()
                .HasOne(sc => sc.Audio)
                .WithMany(c => c.Users)
                .HasForeignKey(sc => sc.AudioId);
        }
        public DbSet<Audio> Audios { get; set; }
        public DbSet<UserAudio> UserAudios { get; set; }
    }
}
