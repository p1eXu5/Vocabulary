using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using p1eXu5.Result;
using Vocabulary.Adapters.Persistance;
using Vocabulary.Adapters.Persistance.Repositories;
using Vocabulary.BlazorServer.Tests.Factories;
using Vocabulary.Categories.DataContracts;
using Vocabulary.Terms.Abstractions;
using Vocabulary.Terms.DataContracts;

namespace Vocabulary.Adapters.Tests.Persistence.Repositories;

public class TermRepositoryTests : MapperTestsBase
{
    protected override ICollection<Type> MappingTypes => new Type[] { typeof(Db.Term), typeof(Db.Category), typeof(Db.Synonym) };

    [Test]
    public async Task ImportAsync_ByDefault_StoresNewTerms()
    {
        // Arrange:
        var option = new DbContextOptionsBuilder<VocabularyDbContext>().UseInMemoryDatabase(databaseName: "Test_Database").Options;

        var dbContext = new VocabularyDbContext(option);
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        Mock<IDbContextFactory<VocabularyDbContext>> mock = new();
        mock.Setup(m => m.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContext);

        var repo = new TermRepository(mock.Object, Mapper, MockLoggerFactories.GetMockILogger<TermRepository>(TestContext.WriteLine).Object);

        var importingTerms = AutoFaker.Generate<TestConfirmedTerm>(5);

        // Action:
        Result result = await repo.ImportAsync(importingTerms);

        // Assert:
        result.Succeeded.Should().BeTrue(" is failed:\n\t"+ result.ToString());

        dbContext = new VocabularyDbContext(option);

        var dbTerms = 
            dbContext.Terms
                .Include(t => t.Synonyms)
                .Include(t => t.TermCategories)
                .ToList()
                .Where(t => importingTerms.Any(it => it.ImportingTerm.Id == t.Id))
                .ToArray();

        dbTerms.SelectMany(t => t.TermCategories.Select(c => c.CategoryId)).Should().BeEquivalentTo(importingTerms.SelectMany(t => t.Categories.Select(c => c.Id)));
        dbTerms.SelectMany(t => t.Synonyms.Select(s => s.Name)).Should().BeEquivalentTo(importingTerms.SelectMany(t => t.ImportingTerm.Synonyms.Select(s => s.Name)));
        dbTerms.Select(t => t.Name).Should().BeEquivalentTo(importingTerms.Select(t => t.ImportingTerm.Name));
        dbTerms.Select(t => t.AdditionalName).Should().BeEquivalentTo(importingTerms.Select(t => t.ImportingTerm.AdditionalName));
        dbTerms.Select(t => t.Description).Should().BeEquivalentTo(importingTerms.Select(t => t.ImportingTerm.Description));

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.DisposeAsync();
    }

    public record TestConfirmedTerm(ImportingTerm ImportingTerm, IReadOnlyCollection<Category> Categories) : IConfirmedTerm;
}