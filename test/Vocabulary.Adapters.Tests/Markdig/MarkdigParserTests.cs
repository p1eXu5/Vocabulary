using FluentAssertions;
using System.Collections;
using Vocabulary.Adapters.Markdig;
using Vocabulary.Terms.DataContracts;

namespace Vocabulary.Adapters.Tests.Markdig;

public class MarkdigParserTests
{
    [TestCaseSource(nameof(ValidMarkdownCases))]
    public async ValueTask _When_ValidMarkdown__Then_CollectsTerms(string fileContent, ImportingTerm[] terms)
    {
        // Arrange:
        var parser = new MarkdigParser();

        // Action:
        var res = await parser.ParseAsync(fileContent);

        // Assert:
        TestContext.WriteLine(res.ToString());
        res.Succeeded.Should().BeTrue();
        res.SuccessContext.Should().HaveCount(2);
        AssertTerms(res.SuccessContext[0], terms[0]);
    }

    private void AssertTerms(ImportingTerm actual, ImportingTerm expected)
    {
        actual.Name.Should().Be(expected.Name);
        actual.AdditionalName.Should().Be(expected.AdditionalName);
        actual.Description.Should().Be(expected.Description);
        actual.Synonyms.Should().BeEquivalentTo(expected.Synonyms);
    }

    internal static IEnumerable ValidMarkdownCases()
    {
        yield return
            new TestCaseData(new object[] {
                """
                # Глоссарий

                | Термин | Определение |
                | --- | ----------- |
                | Foo (Bar) | Foo description. |
                | Baz | Baz description. |
                """,
                new[] {
                    new ImportingTerm("Foo", "Bar", "Foo description."),
                    new ImportingTerm("Baz", "Baz description."),
                }
            }).SetName("| Term | Description |");
    }
}