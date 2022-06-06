using p1eXu5.AutoProfile.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Vocabulary.Adapters.Persistance.Models;

[MapTo(typeof(Vocabulary.Terms.DataContracts.Synonym), ReverseMap = true)]
public record Synonym
{
    public Synonym()
    { }

    public Synonym(string name)
    {
        Name = name;
    }

    [Required, MaxLength(255)]
    public string Name { get; set; } = "";
}
