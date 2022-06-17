using Microsoft.Extensions.Logging;
using p1eXu5.Result;
using p1eXu5.Result.Extensions;
using Techno.Mir.Upay.Abstractions;
using Vocabulary.Terms.DataContracts;
using Vocabulary.Terms.Ports;

namespace Vocabulary.Terms;


public record ExportTermsCommand : IResultCommand<MemoryStream>;


public class ExportTermsCommandHandler : IResultCommandHandler<ExportTermsCommand, MemoryStream>
{
    private readonly ITermRepository _termRepository;
    private readonly ILogger<ExportTermsCommandHandler> _logger;

    public ExportTermsCommandHandler(ITermRepository termRepository, ILogger<ExportTermsCommandHandler> logger)
    {
        _termRepository = termRepository;
        _logger = logger;
    }

    public async Task<Result<MemoryStream>> Handle(ExportTermsCommand request, CancellationToken cancellationToken)
    {
        var termsResult = await _termRepository.GetTermsAsync(cancellationToken);

        if (termsResult.TryGetSucceededContext(out var terms))
        {
            try
            {
                var memoryStream = new MemoryStream();
                using var sw = new StreamWriter(memoryStream, leaveOpen: true);

                await sw.WriteLineAsync(ExportingTerm.Header());
                foreach (var term in terms)
                {
                    await sw.WriteLineAsync(term.ToString());
                }

                await sw.FlushAsync();
                memoryStream.Position = 0;

                return memoryStream.ToSuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on GenerateTermMarkdownCommand.");
                return Result<MemoryStream>.Failure(ex);
            }
        }

        return Result<MemoryStream>.Failure(termsResult.FailedContext);
    }
}