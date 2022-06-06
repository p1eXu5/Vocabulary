using System.Diagnostics.CodeAnalysis;
using Vocabulary.Terms.Abstractions;
using Vocabulary.Terms.Enums;

namespace Vocabulary.Terms;

public class TermNamesComparer
{
    public IEqualityComparer<ITermNames> GetComparer(ComparingNames comparingNames) => comparingNames switch
    {
        ComparingNames.MainAndAdditional => new MainAndAdditionalNamesComparer(),
        ComparingNames.MainOrAdditional => new MainOrAdditionalNamesComparer(),
        ComparingNames.CombinedMainOrAdditional => new CombinedMainOrAdditionalNamesComparer(),
        ComparingNames.Main => new MainNamesComparer(),
        ComparingNames.Additional => new AdditionalNamesComparer(),
        _ => throw new ArgumentException("Unknown ComparingName value.", nameof(comparingNames))
    };

    public IEnumerable<(ITermNames, IEnumerable<ITermNames>)> Compare(IEnumerable<ITermNames> compared, IEnumerable<ITermNames> comparing, ComparingNames comparingNames)
    {
        var comparer = GetComparer(comparingNames);
        return compared.Select(tnd => (tnd, comparing.Where(tng => comparer.Equals(tnd, tng)))).Where(t => t.Item2.Any());
    }

    public IEnumerable<ITermNames> Compare(ITermNames compared, IEnumerable<ITermNames> comparing, ComparingNames comparingNames)
    {
        var comparer = GetComparer(comparingNames);
        return comparing.Where(tng => comparer.Equals(compared, tng));
    }
}

public abstract class NamesComparerBase : IEqualityComparer<ITermNames>
{
    bool IEqualityComparer<ITermNames>.Equals(ITermNames? x, ITermNames? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return Equals(x, y);
    }

    protected abstract bool Equals(ITermNames x, ITermNames y);

    public int GetHashCode([DisallowNull] ITermNames obj)
        => obj.GetHashCode();
}


public sealed class MainAndAdditionalNamesComparer : NamesComparerBase
{
    protected override bool Equals(ITermNames x, ITermNames y)
    {
        if (x.AdditionalName is null || y.AdditionalName is null)
        {
            return false;
        }

        return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.AdditionalName, y.AdditionalName, StringComparison.OrdinalIgnoreCase);
    }
}


public sealed class MainOrAdditionalNamesComparer : NamesComparerBase
{
    protected override bool Equals(ITermNames x, ITermNames y)
    {
        return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase)
            ||
            (
                x.AdditionalName is not null && y.AdditionalName is not null
                && 
                string.Equals(x.AdditionalName, y.AdditionalName, StringComparison.OrdinalIgnoreCase)
            );
    }
}


public sealed class CombinedMainOrAdditionalNamesComparer : NamesComparerBase
{
    protected override bool Equals(ITermNames x, ITermNames y)
    {
        return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase)
            ||
            (
                x.AdditionalName is not null && y.AdditionalName is not null
                &&
                string.Equals(x.AdditionalName, y.AdditionalName, StringComparison.OrdinalIgnoreCase)
            )
            ||
            (
                x.AdditionalName is not null
                &&
                string.Equals(x.AdditionalName, y.Name, StringComparison.OrdinalIgnoreCase)
            )
            ||
            (
                y.AdditionalName is not null
                &&
                string.Equals(y.AdditionalName, x.Name, StringComparison.OrdinalIgnoreCase)
            )
            ;
    }
}


public sealed class MainNamesComparer : NamesComparerBase
{
    protected override bool Equals(ITermNames x, ITermNames y)
    {
        return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
    }
}


public sealed class AdditionalNamesComparer : NamesComparerBase
{
    protected override bool Equals(ITermNames x, ITermNames y)
    {
        if (x.AdditionalName is null || y.AdditionalName is null)
        {
            return false;
        } 

        return string.Equals(x.AdditionalName, y.AdditionalName, StringComparison.OrdinalIgnoreCase);
    }
}