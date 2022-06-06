namespace Vocabulary.Terms.DataContracts;

public record struct Synonym(string Name)
{
    public string Name { get; init; } = Name;
}
