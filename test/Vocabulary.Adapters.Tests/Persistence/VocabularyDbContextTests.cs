using FluentAssertions;
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
    [Test]
    public async Task CanCreateToAndReadTermFromDatabase()
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
        terms[0].Sequence.Should().NotBe(0);
    }
}
