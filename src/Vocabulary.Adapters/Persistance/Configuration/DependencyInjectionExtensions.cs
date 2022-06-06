using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vocabulary.Adapters.Persistance.Repositories;
using Vocabulary.Categories.Ports;
using Vocabulary.Descriptions.Ports;
using Vocabulary.Terms.Ports;

namespace Vocabulary.Adapters.Persistance.Configuration;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistance(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextFactory<VocabularyDbContext>(dbBuilder =>
        {
            dbBuilder.UseSqlite(connectionString);
        });

        services.AddTransient<IDescriptionRepository, DescriptionRepository>();
        services.AddTransient<ITermRepository, TermRepository>();
        services.AddTransient<ICategoryRepository, CategoryRepository>();

        return services;
    }
}