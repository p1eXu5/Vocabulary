using Microsoft.FSharp.Collections;
using p1eXu5.Result;
using Vocabulary.DataContracts.Types;
using Vocabulary.Terms.Abstractions;
using Vocabulary.Terms.DataContracts;

namespace Vocabulary.Terms.Ports;

public interface ITermRepository
{
    Task<Result<IReadOnlyCollection<ExportingTerm>>> GetTermsAsync(CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<TermNames>>> GetTermNamesAsync(CancellationToken cancellationToken);

    Task<Result> ImportAsync(IEnumerable<IConfirmedTerm> importingTerms);
    Task<Result> DeleteAsync(Guid termId);
    Task<IEnumerable<TermName>> GetUncategorizedTermsAsync();
    Task<FSharpList<FullTerm>> GetFullTermsAsync();
}