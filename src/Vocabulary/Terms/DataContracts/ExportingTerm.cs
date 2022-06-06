using System.Text;

namespace Vocabulary.Terms.DataContracts;

public record ExportingTerm(
    int Sequence, 
    string Name, 
    string? AdditionalName, 
    string? Description,
    string? ValidationRules,
    IReadOnlyCollection<Synonym> Synonyms
)
{
    internal static string Header()
    {
        var content =
            new StringBuilder()
                .AppendLine("# Глоссарий")
                .AppendLine("| №  | Термин | Определение | Правила валидации | Синонимы |")
                .Append("| -- | ------ | ----------- | ----------------- | -------- |")
                .ToString();

        return content.ToString();
    }

    public override string ToString()
    {
        var content =
            new StringBuilder()
                .Append("| ")
                .Append(Sequence).Append(" | ")
                .Append(Name);
        
        if (!string.IsNullOrEmpty(AdditionalName)) 
        {
            content
                .Append("<br/>(").Append(AdditionalName).Append(")");
        }

            content
                .Append(" | ")
                .Append(CheckBreaks(Description)).Append(" | ")
                .Append(CheckBreaks(ValidationRules)).Append(" | ")
                .Append(JoinWithComma(Synonyms)).Append(" | ")
                .ToString();

        return content.ToString();
    }

    internal static string CheckBreaks(in string? conent)
        => conent?.ReplaceLineEndings().Replace(Environment.NewLine, "<br/>") ?? "";

    internal static string JoinWithComma(in IReadOnlyCollection<Synonym> coll)
        => string.Join(",", coll.Select(s => s.Name));
}