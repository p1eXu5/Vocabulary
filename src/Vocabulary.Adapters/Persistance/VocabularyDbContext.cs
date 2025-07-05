using Microsoft.EntityFrameworkCore;
using Vocabulary.Adapters.Persistance.EntityTypeConfigurations;
using Vocabulary.Adapters.Persistance.Models;

namespace Vocabulary.Adapters.Persistance;

public class VocabularyDbContext : DbContext
{
    public VocabularyDbContext(DbContextOptions<VocabularyDbContext> options) : base(options)
    {
    }

    public DbSet<Term> Terms => Set<Term>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<TermCategory> TermCategories => Set<TermCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplyConfigurations(modelBuilder);
    }

    protected static void ApplyConfigurations(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TermConfiguration).Assembly);
    }
}
