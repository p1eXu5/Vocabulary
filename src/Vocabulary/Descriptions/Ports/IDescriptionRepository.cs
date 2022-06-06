using p1eXu5.Result;
using Vocabulary.Descriptions.DataContracts;

namespace Vocabulary.Descriptions.Ports;

public interface IDescriptionRepository
{
    Task<Result<DescriptionTerms>> GetDescriptionTermsAsync(Guid termId);

    Task<Result<string>> ReplaceDescription(Guid termId, string newDescription);
}
