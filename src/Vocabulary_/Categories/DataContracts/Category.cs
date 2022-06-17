using Vocabulary.Abstractions;

namespace Vocabulary.Categories.DataContracts;

public record Category(Guid Id, string Name) : IId
{
    public Guid Id { get; } = Id;

    public string Name { get; init; } = Name;

    public override string ToString()
    {
        return Name;
    }
}
