using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using p1eXu5.Result;
using p1eXu5.Result.Extensions;
using Techno.Mir.Upay.Abstractions;
using Vocabulary.Terms.Abstractions;
using Vocabulary.Terms.DataContracts;
using Vocabulary.Terms.Enums;
using Vocabulary.Terms.Ports;

namespace Vocabulary.Terms;
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

    public ImportTermsCommandHandler(IMarkdownParser markdownParser,
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

    public Task<Result<string, string>> Handle(ImportTermsCommand request, CancellationToken cancellationToken)
    {
        Task<Result<IReadOnlyList<ImportingTerm>, string>> contentTask =
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

        Task<Result<IReadOnlyCollection<TermNames>, string>> getTermNamesTask = _termRepository.GetTermNamesAsync(cancellationToken);

        try
        {
            Task.WaitAll([contentTask, getTermNamesTask], cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return Task.FromResult<Result<string, string>>(
                new Result<string, string>.Error("Failed to handle import terms command."));
        }

        if (!contentTask.Result.TryGetSuccessContext(out IReadOnlyList<ImportingTerm> importingTerms))
        {
            return Task.FromResult<Result<string, string>>(
                new Result<string, string>.Error($"Failed to parse markdown file {request.fileName}."));
        }

        ImmutableArray<ConfirmImportingTerm> confirmImportingTerms =
            !getTermNamesTask.Result.TryGetSuccessContext(out IReadOnlyCollection<TermNames> termNames)
                ? importingTerms.OrderBy(it => it.Name).Select(it => new ConfirmImportingTerm(it)).ToImmutableArray()
                : CompareWithExisting(importingTerms, termNames, request.ComparingNames);

        string key = Path.GetFileNameWithoutExtension(request.fileName);
        _memoryCache.Set(key, confirmImportingTerms, TimeSpan.FromMinutes(60));

        return Task.FromResult(key.ToOkWithStringError());
    }


    private ImmutableArray<ConfirmImportingTerm> CompareWithExisting(IReadOnlyList<ImportingTerm> importingTerms,
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
