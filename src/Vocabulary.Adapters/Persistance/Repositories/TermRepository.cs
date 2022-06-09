using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using p1eXu5.Result;
using Vocabulary.Terms.DataContracts;
using Vocabulary.Terms.Ports;
using AutoMapper;
using System.Collections.Immutable;
using p1eXu5.Result.Extensions;
using Vocabulary.Terms.Abstractions;
using Vocabulary.Adapters.Persistance.Models;

namespace Vocabulary.Adapters.Persistance.Repositories;

using DbTerm = Vocabulary.Adapters.Persistance.Models.Term;


public class TermRepository : ITermRepository
{
    private readonly IDbContextFactory<VocabularyDbContext> _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<TermRepository> _logger;


    public TermRepository(IDbContextFactory<VocabularyDbContext> dbContextFactory, IMapper mapper, ILogger<TermRepository> logger)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<Result<IReadOnlyCollection<TermNames>>> GetTermNamesAsync(CancellationToken cancellationToken)
    {
        using VocabularyDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var termList = await dbContext.Terms.ToListAsync(cancellationToken);

        if (!termList.Any())
        {
            return Result<IReadOnlyCollection<TermNames>>.Failure("Have no terms.");
        }

        IReadOnlyCollection<TermNames> result =
            termList.Select(_mapper.Map<TermNames>).ToImmutableArray();

        return result.ToSuccessResult();
    }


    public async Task<Result<IReadOnlyCollection<ExportingTerm>>> GetTermsAsync(CancellationToken cancellationToken)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var termList = await dbContext.Terms.ToListAsync(cancellationToken);

        if (!termList.Any()) {
            return Result<IReadOnlyCollection<ExportingTerm>>.Failure("Have no terms.");
        }

        IReadOnlyCollection<ExportingTerm> result =
            termList.Select(_mapper.Map<ExportingTerm>).ToImmutableArray();

        return result.ToSuccessResult();
    }


    public async Task<Result> ImportAsync(IEnumerable<IConfirmedTerm> importingTerms)
    {
        try
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var seq = await dbContext.Terms.OrderBy(t => t.Sequence).LastAsync();
            var startSequence = seq.Sequence + 1;

            var terms = importingTerms.Select(t =>
            {
                var term = _mapper.Map<Term>(t.ImportingTerm);
                term.Sequence = startSequence++;
                term.TermCategories = t.Categories.Select(c => new TermCategory() { TermId = term.Id, CategoryId = c.Id }).ToList();
                return term;
            });

            await dbContext.Terms.AddRangeAsync(terms);
            await dbContext.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("Error on importing terms into database.", ex);
        }
    }


    public async Task<Result> DeleteAsync(Guid termId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        DbTerm? dbTerm = await dbContext.Terms.SingleOrDefaultAsync(t => t.Id == termId);

        if (dbTerm is not null)
        {
            if (dbTerm.IsDeleted)
            {
                return Result.Failure("Term had already been deleted.");
            }

            dbTerm.IsDeleted = true;

            try
            {
                await dbContext.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("Error on updating term.", ex);
            }
        }

        return Result.Failure("Term has not been found.");
    }
}