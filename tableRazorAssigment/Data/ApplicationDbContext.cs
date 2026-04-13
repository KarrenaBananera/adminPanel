using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace tableRazorAssigment.Data;
public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<User>()
            .Property(x => x.UserEmail)
            .HasMaxLength(260);
        builder.Entity<User>()
          .Property(x => x.Title)
          .HasMaxLength(260);
        builder.Entity<User>()
        .HasIndex(x => x.UserEmail)
        .IsUnique();
    }
}