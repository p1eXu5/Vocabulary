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
        
        services.AddTransient<Vocabulary.Terms.Ports.ITermRepository, TermRepository>();
        services.AddTransient<Vocabulary.Terms.Types.ITermRepository, TermRepository>();

        services.AddTransient<Vocabulary.Categories.Ports.ICategoryRepository, CategoryRepository>();
        services.AddTransient<Vocabulary.Categories.Types.ICategoryRepository, CategoryRepository>();

        return services;
    }
}