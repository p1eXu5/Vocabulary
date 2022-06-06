using p1eXu5.Result;
using Vocabulary.Terms.DataContracts;

namespace Vocabulary.Terms.Ports;

public interface IMarkdownParser
{
    ValueTask<Result<IReadOnlyList<ImportingTerm>>> ParseAsync(string fileContent);
}