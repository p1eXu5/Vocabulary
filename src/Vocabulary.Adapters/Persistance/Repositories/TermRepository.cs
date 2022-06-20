using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using p1eXu5.Result;
using Vocabulary.Terms.DataContracts;
using Vocabulary.Terms.Types;
using AutoMapper;
using System.Collections.Immutable;
using p1eXu5.Result.Extensions;
using Vocabulary.Terms.Abstractions;
using Vocabulary.Adapters.Persistance.Models;
using Vocabulary.DataContracts.Types;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Vocabulary.Descriptions.DataContracts;

namespace Vocabulary.Adapters.Persistance.Repositories;

using DbTerm = Term;
using Link = DataContracts.Types.Link;
using Synonym = DataContracts.Types.Synonym;

public class TermRepository : Vocabulary.Terms.Ports.ITermRepository, Vocabulary.Terms.Types.ITermRepository
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

    public async Task<IEnumerable<TermName>> GetUncategorizedTermsAsync()
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var dbTerms = await dbContext.Terms.OrderBy(t => t.Name).ToArrayAsync();

        return dbTerms.Select(t => new TermName(t.Id, t.Name, t.AdditionalName));
    }

    public async Task<FSharpList<FullTerm>> GetFullTermsAsync()
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var dbTerms = 
            await dbContext.Terms
                .Include(t => t.Categories)
                .Include(t => t.Synonyms)
                .Include(t => t.Links)
                .OrderBy(t => t.Name)
                .ToArrayAsync();

        IEnumerable<FullTerm> fullTerms =
            dbTerms
                .Select(t => 
                    FullTermModule.create(
                        t.Id,
                        t.Sequence,
                        t.Name,
                        t.AdditionalName,
                        t.Description,
                        t.ValidationRules,
                        t.Synonyms.Select(s => new Synonym(s.Name)),
                        t.Categories.Select(c => new CategoryName(c.Id, c.Name)),
                        t.Links.Select(l => new Link(l.ResourceDescription, l.Href))
                    )
                );

        return
            FullTermModule.toFSharpList(fullTerms);
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

    public async Task<FSharpOption<TermsInDescription>> FindTermsInDescriptionAsync(Guid termId, CancellationToken cancellationToken)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var term = await dbContext.Terms.SingleOrDefaultAsync(t => t.Id == termId, cancellationToken);

        if (term is null)
        {
            _logger.LogWarning("Has no term with id {termId}", termId);
            return FSharpOption<TermsInDescription>.None;
        }

        if (string.IsNullOrWhiteSpace(term.Description))
        {
            _logger.LogDebug("Term {termId} has no description", termId);
            return FSharpOption<TermsInDescription>.None;
        }

        var termNames =
            await dbContext.Terms
                .OrderBy(t => t.Name)
                .Select(t => new TermName(t.Id, t.Name,t.AdditionalName))
                .ToArrayAsync(cancellationToken);

        if (termNames.Any())
        {
            TermsInDescription termsInDescription = TermsInDescriptionModule.create(term.Description, termNames);
            return FSharpOption<TermsInDescription>.Some(termsInDescription);
        }

        _logger.LogWarning("Have no terms.");
        return FSharpOption<TermsInDescription>.None;
    }
}