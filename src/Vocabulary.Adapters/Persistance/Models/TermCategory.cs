namespace Vocabulary.Adapters.Persistance.Models;

public class TermCategory
{
    internal TermCategory() { }

    public Guid CategoryId { get; init; }

    public Guid TermId { get; init; }
}
