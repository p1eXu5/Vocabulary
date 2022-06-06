using p1eXu5.Result;
using Techno.Mir.Upay.Abstractions;

namespace Vocabulary.Descriptions;

public record CheckTermsCommand(Guid TermId) : ICommand<Result<string>>;
