using System.Collections.Immutable;
using Vocabulary.Categories.DataContracts;
using Vocabulary.Terms.DataContracts;

namespace Vocabulary.Terms.Abstractions;

public interface IConfirmedTerm
{
    ImportingTerm ImportingTerm { get; }

    IReadOnlyCollection<Category> Categories { get; }
}