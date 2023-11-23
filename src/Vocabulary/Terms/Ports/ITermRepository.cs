using p1eXu5.Result;
using Vocabulary.Terms.Abstractions;
using Vocabulary.Terms.DataContracts;

namespace Vocabulary.Terms.Ports;

public interface ITermRepository
{
    Task<Result<IReadOnlyCollection<ExportingTerm>, string>> GetTermsAsync(CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<TermNames>, string>> GetTermNamesAsync(CancellationToken cancellationToken);

    Task<Result<Unit, string>> ImportAsync(IEnumerable<IConfirmedTerm> importingTerms);
    Task<Result<Unit, string>> DeleteAsync(Guid termId);
}