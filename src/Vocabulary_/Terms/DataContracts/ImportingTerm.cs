using AutoMapper.Configuration.Annotations;
using System.Collections.Immutable;
using Vocabulary.Terms.Abstractions;

namespace Vocabulary.Terms.DataContracts
{
    public record struct ImportingTerm : ITermNames
    {
        public ImportingTerm(string name)
        {
            Name = name;
            Description = default;
            AdditionalName = default;
            Synonyms = Array.Empty<Synonym>().ToImmutableArray();
        }

        public ImportingTerm(string name, string description)
        {
            Name = name;
            Description = description;
            AdditionalName = default;
            Synonyms = Array.Empty<Synonym>().ToImmutableArray();
        }

        public ImportingTerm(string name, string? additionalName, string description)
            : this(name, description)
        {
            AdditionalName = additionalName;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; init; }
        public string? AdditionalName { get; init; }
        public string? Description { get; init; }


        public IReadOnlyCollection<Synonym> Synonyms { get; init; }
    }
}