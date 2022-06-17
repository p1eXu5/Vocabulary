using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using p1eXu5.Result;
using p1eXu5.Result.Extensions;
using System.Collections.Immutable;
using Techno.Mir.Upay.Abstractions;
using Vocabulary.Terms.Abstractions;
using Vocabulary.Terms.DataContracts;
using Vocabulary.Terms.Enums;
using Vocabulary.Terms.Ports;

namespace Vocabulary.Terms;

using ConfirmResult = Result<ImmutableArray<ConfirmImportingTerm>>;


/// <summary>
/// 
/// </summary>
/// <param name="fileName"></param>
/// <param name="ComparingNames"></param>
public record ImportTermsCommand(string fileName, ComparingNames ComparingNames) : IResultCommand<string>;


/// <summary>
/// 
/// </summary>
public class ImportTermsCommandHandler : IResultCommandHandler<ImportTermsCommand, string>
{
    private readonly IMarkdownParser _markdownParser;
    private readonly ITermRepository _termRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<ImportTermsCommandHandler> _logger;
    private readonly TermNamesComparer _termNamesComparer;

    public ImportTermsCommandHandler( IMarkdownParser markdownParser,
                                      ITermRepository termRepository,
                                      IMemoryCache memoryCache,
                                      ILogger<ImportTermsCommandHandler> logger)
    {
        _markdownParser = markdownParser;
        _termRepository = termRepository;
        _memoryCache = memoryCache;
        _logger = logger;

        _termNamesComparer = new();
    }

    public async Task<Result<string>> Handle(ImportTermsCommand request, CancellationToken cancellationToken)
    {
        var contentTask = 
            File.ReadAllTextAsync(request.fileName, cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.Exception?.InnerException is not null)
                    {
                        throw t.Exception.InnerException;
                    }
                    return _markdownParser.ParseAsync(t.Result).AsTask();
                }, cancellationToken)
                .Unwrap();

        var getTermNamesTask = _termRepository.GetTermNamesAsync(cancellationToken);

        try
        {
            await Task.WhenAll(contentTask, getTermNamesTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return Result<string>.Failure(ex);
        }

        if (!contentTask.Result.TryGetSucceededContext(out IReadOnlyList<ImportingTerm> importingTerms))
        {
            return Result<string>.Failure(contentTask.Result);
        }

        ImmutableArray<ConfirmImportingTerm> confirmImportingTerms =
            !getTermNamesTask.Result.TryGetSucceededContext(out IReadOnlyCollection<TermNames> termNames)
                ? importingTerms.OrderBy(it => it.Name).Select(it => new ConfirmImportingTerm(it)).ToImmutableArray()
                : CompareWithExisting(importingTerms, termNames, request.ComparingNames);

        string key = Path.GetFileNameWithoutExtension(request.fileName);
        _memoryCache.Set(key, confirmImportingTerms, TimeSpan.FromMinutes(60));

        return key.ToSuccessResult();
    }


    private ImmutableArray<ConfirmImportingTerm> CompareWithExisting( IReadOnlyList<ImportingTerm> importingTerms,
                                                                      IReadOnlyCollection<TermNames> termNames,
                                                                      ComparingNames comparingNames)
    {
        IEqualityComparer<ITermNames> comparer = _termNamesComparer.GetComparer(comparingNames);

        return
            importingTerms.OrderBy(it => it.Name).Select(it =>
            {
                IReadOnlyCollection<TermNames> similar = termNames.Where(tn => comparer.Equals(it, tn)).ToImmutableArray();
                return new ConfirmImportingTerm(it, similar);
            })
            .OrderByDescending(ct => ct.IsNotInDb)
            .ToImmutableArray();
    }
}
