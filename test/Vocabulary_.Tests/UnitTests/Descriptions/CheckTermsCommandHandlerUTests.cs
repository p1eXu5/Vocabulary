using FluentAssertions;
using Moq;
using p1eXu5.Result;
using p1eXu5.Result.Extensions;
using Vocabulary.Descriptions;
using Vocabulary.Descriptions.DataContracts;
using Vocabulary.Descriptions.Ports;

namespace Vocabulary.Tests.UnitTests.Descriptions;

public class CheckTermsCommandHandlerUTests
{
    [Test]
    public async Task DescriptionContainsSingleTermWithDot_InsertsLinkAndPreserveDotToDescription()
    {
        // Arrange:
        var terms = new[] {
            "Foo",
        };
        var description = $"{terms[0]}.";

        var termId = Guid.NewGuid();
        var dto = new DescriptionTerms(description, terms);
        var (handler, map) = GetHandler(termId, dto);
        var command = new CheckTermsCommand(termId);

        // Action:
        Result<string> res = await handler.Handle(command, CancellationToken.None);

        // Assert:
        res.Succeeded.Should().BeTrue();
        map.Should().NotBeEmpty();
        map[termId].Should().NotBeEquivalentTo(description);
        TestContext.WriteLine(map[termId]);
    }

    [Test]
    public async Task DescriptionContainsTerms_InsertsLinksToDescription()
    {
        // Arrange:
        var terms = new[] {
            "Foo",
            "Simple Bar",
            "Baz 8536"
        };
        var description = $"A {terms[0]} contains {terms[1]}. See {terms[2]}.";

        var termId = Guid.NewGuid();
        var dto = new DescriptionTerms(description, terms);
        var (handler, map) = GetHandler(termId, dto);
        var command = new CheckTermsCommand(termId);

        // Action:
        Result<string> res = await handler.Handle(command, CancellationToken.None);

        // Assert:
        res.Succeeded.Should().BeTrue();
        map.Should().NotBeEmpty();
        map[termId].Should().NotBeEquivalentTo(description);
        TestContext.WriteLine(map[termId]);
    }

    [Test]
    public async Task DescriptionDoesNotContainTerms_DoesNotCallReplaceDescription()
    {
        // Arrange:
        var terms = new[] {
            "Foo",
            "Simple Bar",
            "Baz 8536"
        };
        var description = $"description";

        var termId = Guid.NewGuid();
        var dto = new DescriptionTerms(description, terms);
        var (handler, map) = GetHandler(termId, dto);
        var command = new CheckTermsCommand(termId);

        // Action:
        Result<string> res = await handler.Handle(command, CancellationToken.None);

        // Assert:
        _mockRepo.Verify(r => r.ReplaceDescription(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    #region factories

    private Mock<IDescriptionRepository> _mockRepo = default!;

    private (CheckTermsCommandHandler, Dictionary<Guid, string>) GetHandler(Guid termId, DescriptionTerms dto)
    {
        _mockRepo = new Mock<IDescriptionRepository>();

        _mockRepo.Setup(r => r.GetDescriptionTermsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(dto.ToSuccessResult());

        var map = new Dictionary<Guid, string>();

        _mockRepo.Setup(r => r.ReplaceDescription(It.IsAny<Guid>(), It.IsAny<string>()))
            .Callback<Guid, string>((guid, descr) => map[guid] = descr)
            .ReturnsAsync((Guid guid, string descr) => Result<string>.Success(descr));

        return (new CheckTermsCommandHandler(_mockRepo.Object), map);
    }

    #endregion ───────────────────────────────────────────────────── factories ─┘
}