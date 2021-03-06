using p1eXu5.Result;
using Vocabulary.Terms.Abstractions;
using Vocabulary.Terms.DataContracts;

namespace Vocabulary.Terms.Ports;

public interface ITermRepository
{
    Task<Result<IReadOnlyCollection<ExportingTerm>>> GetTermsAsync(CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<TermNames>>> GetTermNamesAsync(CancellationToken cancellationToken);

    Task<Result> ImportAsync(IEnumerable<IConfirmedTerm> importingTerms);
    Task<Result> DeleteAsync(Guid termId);
}