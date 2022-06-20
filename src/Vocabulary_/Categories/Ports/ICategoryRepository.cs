using p1eXu5.Result;
using System.Collections.Immutable;
using Vocabulary.DataContracts.Types;
using Vocabulary.Categories.DataContracts;

namespace Vocabulary.Categories.Ports;

public interface ICategoryRepository
{
    Task<Result<Category>> FindAsync(Guid categoryId);

    Task<Result> RemoveAsync(Guid categoryId);
}