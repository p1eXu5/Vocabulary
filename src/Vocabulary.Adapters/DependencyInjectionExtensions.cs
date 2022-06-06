using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vocabulary.Adapters.Markdig;
using Vocabulary.Adapters.Persistance.Configuration;
using Vocabulary.Terms.Ports;

namespace Vocabulary.Adapters;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAdapters(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistance(configuration.GetConnectionString("VocabularyDb"));
        services.AddTransient<IMarkdownParser, MarkdigParser>();
        return services;
    }
}
