using System.Collections.Immutable;
using Vocabulary.Categories.DataContracts;
using Vocabulary.Terms.Abstractions;

namespace Vocabulary.Terms.DataContracts;

public record ConfirmImportingTerm : IConfirmedTerm
{
    public ConfirmImportingTerm(ImportingTerm importingTerm)
    {
        ImportingTerm = importingTerm;
        TermNames = Array.Empty<TermNames>();
        IsNotInDb = true;
        Categories = ImmutableArray<Category>.Empty;
    }

    public ConfirmImportingTerm(ImportingTerm importingTerm, IReadOnlyCollection<TermNames> termNames)
    {
        ImportingTerm = importingTerm;
        TermNames = termNames;
        IsNotInDb = !termNames.Any();
        Categories = ImmutableArray<Category>.Empty;
    }


    public ImportingTerm ImportingTerm { get; init; }
    public IReadOnlyCollection<TermNames> TermNames { get; init; }

    public bool IsNotInDb { get; set; }

    public IReadOnlyCollection<Category> Categories { get; set; }
}