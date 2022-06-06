using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using p1eXu5.Result;
using p1eXu5.Result.Extensions;
using Vocabulary.Descriptions.DataContracts;
using Vocabulary.Descriptions.Ports;

namespace Vocabulary.Adapters.Persistance.Repositories
{
    public class DescriptionRepository : IDescriptionRepository
    {
        private readonly IDbContextFactory<VocabularyDbContext> _dbContextFactory;
        private readonly ILogger<DescriptionRepository> _logger;

        public DescriptionRepository(IDbContextFactory<VocabularyDbContext> dbContextFactory, ILogger<DescriptionRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<Result<DescriptionTerms>> GetDescriptionTermsAsync(Guid termId)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var term = await dbContext.Terms.SingleOrDefaultAsync(t => t.Id == termId);

            if ( term is null ) {
                _logger.LogWarning("Has no term with id {termId}", termId);
                return Result<DescriptionTerms>.Failure($"Has no term with id {termId}");
            }

            if (string.IsNullOrWhiteSpace(term.Description)) {
                return Result<DescriptionTerms>.Failure($"Term {termId} has no Description.");
            }

            var terms = await dbContext.Terms.Select( t => t.Name ).ToArrayAsync();

            return new DescriptionTerms(term.Description, terms).ToSuccessResult();
        }

        public async Task<Result<string>> ReplaceDescription(Guid termId, string newDescription)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var term = await dbContext.Terms.SingleOrDefaultAsync(t => t.Id == termId);

            if (term is null) {
                _logger.LogWarning("Has no term with id {termId}", termId);
                return Result<string>.Failure($"Has no term with id {termId}");
            }

            term.Description = newDescription;

            await dbContext.SaveChangesAsync();

            return newDescription.ToSuccessResult();
        }
    }
}