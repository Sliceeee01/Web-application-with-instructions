using Microsoft.EntityFrameworkCore;

namespace InstructiionsApp.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Эти свойства станут таблицами в базе данных
        public DbSet<Instruction> Instructions { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
    }
}