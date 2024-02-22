using Microsoft.EntityFrameworkCore;

namespace DataStreams.DataModel.Models;

public class StreamContext : DbContext
{
    public DbSet<Stream> Streams { get; set; }
 
    public StreamContext(DbContextOptions<StreamContext> options)
        : base(options)
    {
    }
}
