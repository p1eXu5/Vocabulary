using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using p1eXu5.Result;
using p1eXu5.Result.Extensions;
using Vocabulary.Descriptions.DataContracts;
using Vocabulary.Descriptions.Ports;

namespace Vocabulary.Adapters.Persistance.Repositories;

public class DescriptionRepository(IDbContextFactory<VocabularyDbContext> dbContextFactory, ILogger<DescriptionRepository> logger) : IDescriptionRepository
{
    public async Task<Result<DescriptionTerms, string>> GetDescriptionTermsAsync(Guid termId)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var term = await dbContext.Terms.SingleOrDefaultAsync(t => t.Id == termId);

        if ( term is null ) {
            logger.LogWarning("Has no term with id {termId}", termId);
            return new Result<DescriptionTerms, string>.Error($"Has no term with id {termId}");
        }

        if (string.IsNullOrWhiteSpace(term.Description)) {
            return new Result<DescriptionTerms, string>.Error($"Term {termId} has no Description.");
        }

        var terms = await dbContext.Terms.Select( t => t.Name ).ToArrayAsync();

        return new DescriptionTerms(term.Description, terms).ToOkWithStringError();
    }

    public async Task<Result<string, string>> ReplaceDescription(Guid termId, string newDescription)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var term = await dbContext.Terms.SingleOrDefaultAsync(t => t.Id == termId);

        if (term is null) {
            logger.LogWarning("Has no term with id {termId}", termId);
            return new Result<string, string>.Error($"Has no term with id {termId}");
        }

        term.Description = newDescription;

        await dbContext.SaveChangesAsync();

        return newDescription.ToOkWithStringError();
    }
}