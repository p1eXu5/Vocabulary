using System.Collections.Immutable;

namespace Vocabulary.DataContracts;


public record struct Synonym(string Name);

public record struct Link(string ResourceDescription, string Href);

public record FullTerm(
    Guid Id,
    string Name,
    string AdditionalName,
    string Description,
    string ValidationRules,
    ImmutableArray<Synonym> Synonyms,
    ImmutableArray<Guid> CategoryIds,
    ImmutableArray<Link> Links
);


public record TermName(Guid Id, string Name, string AdditionalName);


public record NavCategory(
    Guid Id,
    string Name,
    ImmutableArray<TermName> TermNames
);