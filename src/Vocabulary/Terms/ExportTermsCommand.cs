using Microsoft.Extensions.Logging;
using p1eXu5.Result;
using p1eXu5.Result.Extensions;
using Techno.Mir.Upay.Abstractions;
using Vocabulary.Terms.DataContracts;
using Vocabulary.Terms.Ports;

namespace Vocabulary.Terms;


public record ExportTermsCommand : IResultCommand<MemoryStream>;


public class ExportTermsCommandHandler(ITermRepository termRepository, ILogger<ExportTermsCommandHandler> logger) : IResultCommandHandler<ExportTermsCommand, MemoryStream>
{
    public async Task<Result<MemoryStream, string>> Handle(ExportTermsCommand request, CancellationToken cancellationToken)
    {
        var termsResult = await termRepository.GetTermsAsync(cancellationToken);

        if (termsResult.TryGetSuccessContext(out var terms))
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

                await sw.FlushAsync(cancellationToken);
                memoryStream.Position = 0;

                return memoryStream.ToOkWithStringError();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on GenerateTermMarkdownCommand.");
                return new Result<MemoryStream, string>.Error("Failed to export terms.");
            }
        }

        return new Result<MemoryStream, string>.Error(termsResult.FailedContext());
    }
}