using p1eXu5.AutoProfile.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Vocabulary.Adapters.Persistance.Models;

[MapTo(typeof(Terms.DataContracts.ExportingTerm), MemberList = AutoMapper.MemberList.Destination)]
[MapTo(typeof(Terms.DataContracts.TermNames), MemberList = AutoMapper.MemberList.Destination)]
[MapFrom(typeof(Terms.DataContracts.ImportingTerm), MemberList = AutoMapper.MemberList.Source)]
public class Term
{
    public Term()
    { }

    public Term(string name, params Synonym[] synonyms)
    {
        Name = name;
        Synonyms = synonyms.ToHashSet();
    }

    public Term(string name, string description, params Synonym[] synonyms) : this(name, synonyms)
    {
        Description = description;
    }

    public Term(string name, string additionalName, string description, string validationRules, params Synonym[] synonyms) : this(name, synonyms)
    {
        AdditionalName = additionalName;
        Description = description;
        ValidationRules = validationRules;
    }

    public Guid Id { get; init; } = Guid.NewGuid();

    [Required]
    public int Sequence { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; } = "New Term";

    [MaxLength(255)]
    public string? AdditionalName { get; set; }

    public string? Description { get; set; }

    public string? ValidationRules { get; set; }

    public bool IsDeleted { get; set; }

    public long Timestamp { get; internal set; }

    public ICollection<Synonym> Synonyms { get; } = new HashSet<Synonym>();
    public ICollection<Link> Links { get; } = new HashSet<Link>();
    public ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    public ICollection<TermCategory> TermCategories { get; set; } = new HashSet<TermCategory>();
}
