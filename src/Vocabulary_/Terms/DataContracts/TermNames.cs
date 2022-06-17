using Vocabulary.Terms.Abstractions;

namespace Vocabulary.Terms.DataContracts;

public record TermNames(Guid Id, int Sequence, string Name, string? AdditionalName, IReadOnlyCollection<Synonym> Synonyms) : ITermNames;
