using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vocabulary.Adapters.Persistance.Models;
using Vocabulary.Adapters.Persistance.Repositories;
using Vocabulary.Categories.Ports;
using Vocabulary.Descriptions.Ports;
using Vocabulary.Terms.Ports;

namespace Vocabulary.Adapters.Persistance.Configuration;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextFactory<VocabularyDbContext>(dbBuilder =>
        {
            dbBuilder
                .UseSqlite(connectionString)
                .UseAsyncSeeding(async (context, _, ct) =>
                {
                    var category = await context.Set<Category>().FirstOrDefaultAsync(t => t.Name == "General", ct);
                    if (category is null)
                    {
                        category = new Category("General");
                        context.Set<Category>().Add(category);

                        var term = new Term("Детокенизация", description: "Процесс преобразования Токена в PAN.")
                        {
                            Sequence = 1,
                            IsDeleted = false,
                            Timestamp = DateTimeOffset.UtcNow.ToFileTime(),
                        };

                        context.Set<Term>().Add(term);
                        context.Set<TermCategory>().Add(new TermCategory
                        {
                            TermId = term.Id,
                            CategoryId = category.Id
                        });

                        await context.SaveChangesAsync(ct);
                    }
                });
        });

        services.AddTransient<IDescriptionRepository, DescriptionRepository>();
        services.AddTransient<ITermRepository, TermRepository>();
        services.AddTransient<ICategoryRepository, CategoryRepository>();

        return services;
    }
}