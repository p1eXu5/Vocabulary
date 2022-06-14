using p1eXu5.AutoProfile.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Vocabulary.Adapters.Persistance.Models;

[MapTo(typeof(Categories.DataContracts.Category), MemberList = AutoMapper.MemberList.Destination, ReverseMap = true)]
[MapTo(typeof(DataContracts.Types.NavCategory), MemberList = AutoMapper.MemberList.Destination)]
public class Category
{
    public Category()
    { }

    public Category(string name)
    {
        Name = name;
    }

    public Guid Id { get; init; } = Guid.NewGuid();

    [Required, MaxLength(255)]
    public string Name { get; set; } = "";

    [Opposite(nameof(DataContracts.Types.TermName))]
    public HashSet<Term> Terms { get; set; } = new HashSet<Term>();

    public override string ToString()
    {
        return Name;
    }
}
