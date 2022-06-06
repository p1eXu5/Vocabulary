namespace Vocabulary.Descriptions.DataContracts;

public record DescriptionTerms (
    string Desription,
    ICollection<string> TermNames
);
