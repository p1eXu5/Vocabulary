using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vocabulary.Adapters.Persistance;
using Vocabulary.Adapters.Persistance.Models;

namespace Vocabulary.BlazorServer.Tests.Persistence;

public class VocabularyDbContextTests
{
    private DbContextOptions<VocabularyDbContext> _option = default!;

    [OneTimeSetUp]
    public void InitDb()
    {
        _option = new DbContextOptionsBuilder<VocabularyDbContext>().UseSqlite("Data Source=test.db").Options;

        using var context = new VocabularyDbContext(_option);
        context.Database.Migrate();
    }

    [OneTimeTearDown]
    public void DeleteDb()
    {
        using var context = new VocabularyDbContext(_option);
        try
        {
            context.Database.EnsureDeleted();
        }
        catch (Exception ex)
        {
            TestContext.WriteLine(ex.Message);
        }
    }

    private VocabularyDbContext GetDbContext()
        => new VocabularyDbContext(_option);

    [Test]
    public async Task InMemoryDatabase_CanCreateToAndReadTermFromDatabase()
    {
        var option = new DbContextOptionsBuilder<VocabularyDbContext>().UseInMemoryDatabase(databaseName: "Test_Database").Options;

        var context = new VocabularyDbContext(option);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var term = new Term("Тестовое Определение", "TestTerm", "Description", "Rules");
        var synonym = new Synonym("synonym");
        term.Synonyms.Add(synonym);

        await context.Terms.AddAsync(term);
        await context.SaveChangesAsync();

        var terms = await context.Terms.Include(t => t.Synonyms).Where(t => t.Id == term.Id).ToListAsync();
        terms.Should().HaveCount(1);
        terms[0].Id.Should().Be(term.Id);
        terms[0].Synonyms.Should().HaveCount(1);
        terms[0].Sequence.Should().NotBe(0); //  behavior is different than when real db file is used
    }

    [Test]
    public async Task DatabaseFile_AddTermWithZeroSequence_Throws()
    {
        using var context = GetDbContext();

        var term = new Term("Тестовое Определение", "TestTerm", "Description", "Rules");
        var synonym = new Synonym("synonym");
        term.Synonyms.Add(synonym);

        await context.Terms.AddAsync(term);

        Func<Task> act = () => context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Test]
    public async Task DatabaseFile_AddTermWithTheSameSequence_Throws()
    {
        using (var context = GetDbContext())
        {
            var term1 = new Term("Тестовое Определение1", "TestTerm1", "Description1", "Rules1");
            term1.Sequence = 2;

            await context.Terms.AddAsync(term1);
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext())
        {
            var term2 = new Term("Тестовое Определение2", "TestTerm2", "Description2", "Rules2");
            term2.Sequence = 2;

            await context.Terms.AddAsync(term2);

            Func<Task> act = () => context.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateException>();
        }
    }
}
