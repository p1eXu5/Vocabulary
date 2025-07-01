using System.Collections.Immutable;
using Techno.Mir.Upay.Abstractions;
using Vocabulary.Descriptions.Ports;
using System.Web;
using p1eXu5.Result;
using p1eXu5.Result.Extensions;
using Vocabulary.Descriptions.DataContracts;

namespace Vocabulary.Descriptions;

public class CheckTermsCommandHandler : ICommandHandler<CheckTermsCommand, Result<string, string>>
{
    private readonly IDescriptionRepository _descriptionRepository;

    public CheckTermsCommandHandler(IDescriptionRepository descriptionRepository)
    {
        _descriptionRepository = descriptionRepository;
    }

    public async Task<Result<string, string>> Handle(CheckTermsCommand request, CancellationToken cancellationToken)
    {
        var descriptionTermsRes = await _descriptionRepository.GetDescriptionTermsAsync(request.TermId);
        
        Result<string, Unit> checkedResult = descriptionTermsRes
            .MapError(_ => new Unit())
            .Bind(dt => CheckTerms(dt));

        if (checkedResult.IsError())
        {
            return "Failed to check terms".ToError<string, string>();
        }

        string description = checkedResult.SuccessContext();

        return await _descriptionRepository.ReplaceDescription(request.TermId, description);
    }

    private Result<string, Unit> CheckTerms(DescriptionTerms descriptionTerms)
    {
        ImmutableArray<Replacement> positions = 
            descriptionTerms.TermNames
                .Select(t => {
                    // TODO: use regex to find and remove existing links
                    // TODO: check all occurrences of term
                    var ind = descriptionTerms.Desription.IndexOf(t);
                    if (
                        ind >= 0 
                        && 
                        (
                            ind + t.Length == descriptionTerms.Desription.Length 
                            || Char.IsWhiteSpace(descriptionTerms.Desription[ind + t.Length])
                            || Char.IsPunctuation(descriptionTerms.Desription[ind + t.Length])
                        ) 
                    ) {
                        var link = $"[{t}](/terms?search={Uri.EscapeDataString(t)})";
                        return new Replacement(ind, t, link, link.Length - t.Length);
                    }

                    return (Replacement?)null;
                })
                .Where(r => r.HasValue)
                .Cast<Replacement>()
                .OrderBy(r => r.OriginInd)
                .ThenByDescending(r => r.Difference)
                .DistinctBy(r => r.OriginInd)
                .Aggregate(new List<Replacement>(), (list, el) =>
                {
                    if (!list.Any())
                    {
                        list.Add(el);
                        return list;
                    }

                    var lastReplacement = list.Last();
                    if ((lastReplacement.OriginInd + lastReplacement.Term.Length) < el.OriginInd)
                    {
                        list.Add(el);
                    }

                    return list;
                })
                .ToImmutableArray();

        if (!positions.Any()) {
            return Result.UnitErrorWith<string>();
        }

        var description = descriptionTerms.Desription.AsMemory();
        var buffLength = positions.Sum(r => r.Difference) + descriptionTerms.Desription.Length;
        var buff = new char[buffLength];
        var result = new Memory<char>(buff);
        int startOrigin = 0, startRes = 0, originLength = 0;
        Memory<char> resSpan;

        foreach (var pos in positions) 
        {
            if (startOrigin < pos.OriginInd)
            {
                originLength = pos.OriginInd - startOrigin;
                resSpan = result.Slice(startRes, originLength);
                description.Slice(startOrigin, originLength).CopyTo(resSpan);

                startRes += originLength;
                resSpan = result.Slice(startRes, pos.Link.Length);
                pos.Link.AsMemory().CopyTo(resSpan);
                startRes += pos.Link.Length;

                startOrigin = pos.OriginInd + pos.Term.Length;
            }
        }

        originLength = descriptionTerms.Desription.Length - startOrigin;
        resSpan = result.Slice(startRes, originLength);
        description.Slice(startOrigin, originLength).CopyTo(resSpan);

        return new String(buff).ToOk();
    }

    private record struct Replacement(
        int OriginInd,
        string Term,
        string Link,
        int Difference
    )
    {
        /// <summary>
        /// Difference between generating link length and found term length.
        /// </summary>
        public int Difference { get; init; } = Difference;
    };
}
