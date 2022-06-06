using p1eXu5.AutoProfile.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Vocabulary.Adapters.Persistance.Models;

[MapTo(typeof(Vocabulary.Categories.DataContracts.Category), MemberList = AutoMapper.MemberList.Destination, ReverseMap = true)]
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

    public ICollection<Term> Terms { get; set; } = new HashSet<Term>();

    public override string ToString()
    {
        return Name;
    }
}
