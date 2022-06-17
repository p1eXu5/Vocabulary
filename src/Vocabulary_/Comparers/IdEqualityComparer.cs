using System.Diagnostics.CodeAnalysis;
using Vocabulary.Abstractions;

namespace Vocabulary.Comparers;

public class IdEqualityComparer<TEntity> : IEqualityComparer<TEntity>
    where TEntity : IId
{
    public bool Equals(TEntity? x, TEntity? y)
    {
        if (x is null && y is null)
        {
            return false;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.Id.Equals(y.Id);
    }

    public int GetHashCode([DisallowNull] TEntity obj)
    {
        throw new NotImplementedException();
    }
}