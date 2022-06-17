using Vocabulary.Terms.DataContracts;

namespace Vocabulary.Terms.Abstractions;

public interface ITermNames
{
    string Name { get; init; }
    string? AdditionalName { get; }
    IReadOnlyCollection<Synonym> Synonyms { get; }
}