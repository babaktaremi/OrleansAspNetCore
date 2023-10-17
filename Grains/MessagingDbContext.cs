using Microsoft.EntityFrameworkCore;

namespace Grains;

public class MessagingDbContext:DbContext
{
    public MessagingDbContext(DbContextOptions<MessagingDbContext> options):base(options)
    {
        
    }

    public DbSet<MessagingModel> Messages { get; set; } = default!;
}