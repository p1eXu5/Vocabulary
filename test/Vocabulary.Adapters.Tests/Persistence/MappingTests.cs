using AutoMapper;
using FluentAssertions;
using p1eXu5.AutoProfile;
using System.Collections.Generic;
using Vocabulary.BlazorServer.Tests.Factories;

namespace Vocabulary.Adapters.Tests.Persistence;

using DbTerm = Persistance.Models.Term;
using DbSynonym = Persistance.Models.Synonym;
using ExportingTerm = Terms.DataContracts.ExportingTerm;
using ImportingTerm = Terms.DataContracts.ImportingTerm;
using TermNames = Terms.DataContracts.TermNames;
using Synonym = Terms.DataContracts.Synonym;

public class MappingTests
{
    protected IMapper Mapper { get; private set; } = default!;

    protected virtual ICollection<Type> MappingTypes { get; }
        = new[] {
            typeof(DbTerm),
            typeof(DbSynonym),
        };

    [OneTimeSetUp]
    public void Initialize()
    {
        AutoProfile autoProfile = new AutoProfile(
            MockLoggerFactories.GetMockILogger<AutoProfile>(TestContext.WriteLine).Object, 
            new AutoProfileOptions { NotProcessMapAttributesFromAssembly = true }
        );

        foreach (var type in MappingTypes) {
            autoProfile.CreateMaps(type);
        }

        var conf = new MapperConfiguration(cfg => cfg.AddProfile(autoProfile.Configure()));
        conf.AssertConfigurationIsValid();

        Mapper = conf.CreateMapper();
    }

    [Test]
    public void From_PersistantTerm_To_ExportingTerm()
    {
        // Arrange:
        var persistantTerm = AutoFaker.Generate<DbTerm>();

        // Action:
        var exportingTerm = Mapper.Map<ExportingTerm>(persistantTerm);

        // Assert:
        exportingTerm.Sequence.Should().Be(persistantTerm.Sequence);
        exportingTerm.Name.Should().Be(persistantTerm.Name);
        exportingTerm.AdditionalName.Should().Be(persistantTerm.AdditionalName);
        exportingTerm.Description.Should().Be(persistantTerm.Description);
        exportingTerm.ValidationRules.Should().Be(persistantTerm.ValidationRules);
        exportingTerm.Synonyms.Should().BeEquivalentTo(persistantTerm.Synonyms);
    }

    [Test]
    public void From_PersistantTerm_To_TermNames()
    {
        // Arrange:
        var persistantTerm = AutoFaker.Generate<DbTerm>();

        // Action:
        var termNames = Mapper.Map<TermNames>(persistantTerm);

        // Assert:
        termNames.Id.Should().Be(persistantTerm.Id);
        termNames.Sequence.Should().Be(persistantTerm.Sequence);
        termNames.Name.Should().Be(persistantTerm.Name);
        termNames.AdditionalName.Should().Be(persistantTerm.AdditionalName);
        termNames.Synonyms.Should().BeEquivalentTo(persistantTerm.Synonyms);
    }

    [Test]
    public void From_ImportingTerm_To_Term()
    {
        // Arrange:
        var importingTerm = AutoFaker.Generate<ImportingTerm>();

        // Action:
        var dbTerm = Mapper.Map<DbTerm>(importingTerm);

        // Assert:
        importingTerm.Id.Should().Be(dbTerm.Id);
        importingTerm.Name.Should().Be(dbTerm.Name);
        importingTerm.AdditionalName.Should().Be(dbTerm.AdditionalName);
        importingTerm.Description.Should().Be(dbTerm.Description);
        importingTerm.Synonyms.Count.Should().Be(dbTerm.Synonyms.Count);
        importingTerm.Synonyms.Select(s => s.Name).Should().BeEquivalentTo(dbTerm.Synonyms.Select(s => s.Name));
    }
}