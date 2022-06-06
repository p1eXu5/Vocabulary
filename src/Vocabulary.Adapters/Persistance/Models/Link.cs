using System.ComponentModel.DataAnnotations;

namespace Vocabulary.Adapters.Persistance.Models;

public class Link
{
    public Link()
    { }

    public Link(string href)
    {
        Href = href;
    }

    public int Id { get; set; }

    [Required, MaxLength(2047)]
    public string Href { get; set; } = "";

    public string? ResourceDescription { get; set; }

    public Link CloneToNewLink() => new Link() { Href = Href, ResourceDescription = ResourceDescription };
}
