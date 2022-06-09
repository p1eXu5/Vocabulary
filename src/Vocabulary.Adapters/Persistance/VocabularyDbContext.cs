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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplyConfigurations(modelBuilder);


        var category = new Category("General");
        var term = new Term("Детокенизация", description: "Процесс преобразования Токена в PAN.")
        {
            Timestamp = DateTimeOffset.UtcNow.ToFileTime(),
        };

        modelBuilder.Entity("TermCategory").HasData(new { TermId = term.Id, CategoryId = category.Id });
        modelBuilder.Entity<Category>().HasData(category);
        modelBuilder.Entity<Term>().HasData(new { term.Id, term.Name, term.Description, Sequence = 1, IsDeleted = false, term.Timestamp });
    }

    protected static void ApplyConfigurations(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TermConfiguration).Assembly);
    }
}
