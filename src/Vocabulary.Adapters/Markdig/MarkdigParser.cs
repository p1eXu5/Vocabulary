using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Parsers;
using Markdig.Syntax;
using p1eXu5.Result.Extensions;
using p1eXu5.Result;
using System.Collections.Immutable;
using Vocabulary.Terms.DataContracts;
using Vocabulary.Terms.Ports;

namespace Vocabulary.Adapters.Markdig;

using ParsingResult = Result<IReadOnlyList<ImportingTerm>>;
using ParsingProcessResult = Result<MarkdownDocument>;

public class MarkdigParser : IMarkdownParser
{
    /// <summary>
    /// Returns succeeded result if <paramref name="fileContent"/> contains any term.
    /// </summary>
    /// <param name="fileContent"></param>
    /// <returns></returns>
    public ValueTask<ParsingResult> ParseAsync(string fileContent)
    {
        return new ValueTask<ParsingResult>(Parse(fileContent));
    }

    private static ParsingResult Parse(string fileContent)
    {
        MarkdownDocument document = MarkdownParser.Parse(fileContent, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());

        ICollection<ImportingTerm> terms = Array.Empty<ImportingTerm>();

        if (document.Count < 2) {
            return ParsingResult.Failure("Wrong content");
        }

        return
            CheckHeaderLine(document)
                .Bind(md => CheckTableHeader(fileContent, md, 0, "Термин").Map(_ => md))
                .Bind(md => CheckTableHeader(fileContent, md, 1, "Определение"))
                .Bind(table => CollectTerms(fileContent, table).ToList().TraverseA(id => id).result.Map(r => (IReadOnlyList<ImportingTerm>)r.ToList()));

    }

    private static ParsingProcessResult CheckHeaderLine(MarkdownDocument document)
    {
        if (document[0] is HeadingBlock hb && hb.Inline?.FirstChild?.ToString() == "Глоссарий") {
            return document.ToSuccessResult();
        }
        return ParsingProcessResult.Failure("Header is not valid.");
    }

    private static Result<Table> CheckTableHeader(string fileContent, MarkdownDocument document, int column, string header)
    {
        if (document[1] is Table table 
            && table.Count >= 2                         // it's two or more rows
            && table[0] is TableRow row
            && row.Count == 2                           // it's two or more columns
            && row[column] is TableCell cell
            && !cell.Span.IsEmpty
        ) {
            return 
                fileContent[cell.Span.Start..cell.Span.End].Trim().Equals(header, StringComparison.Ordinal)
                    ? table.ToSuccessResult()
                    : table.ToFailedResult();
        }
        return Result<Table>.Failure($"Table does not contain column \"{header}\" ({column}).");
    }

    private static IEnumerable<Result<ImportingTerm>> CollectTerms(string fileContent, Table table)
    {
        Result<string> ReadCellData(TableRow row, int column)
        {
            if (row[column] is TableCell cell && !cell.Span.IsEmpty)
            {
                return fileContent[cell.Span.Start .. cell.Span.End].Trim().ToSuccessResult();
            }

            return Result<string>.Failure();
        }


        for (int i = 1; i < table.Count; ++i)
        {
            if (table[i] is TableRow row && row.Count == 2)
            {
                yield return
                    ReadCellData(row, 0)
                        .Map(GetTermNames)
                        .Bind(term => ReadCellData(row, 1).Map(descriptions => term with { Description = descriptions}));
            }
        }

        yield break;
    }

    public static ImportingTerm GetTermNames(string termNames)
    {
        int ind = 0;
        char[] buff = new char[termNames.Length];
        List<string> terms = new(3);

        void AddTerm()
        {
            if (ind > 0)
            {
                var value = new string(buff[..ind]).Trim();
                if (value.Length > 0)
                {
                    terms.Add(value);
                }
                ind = 0;
            }
        }

        foreach (var ch in termNames.Replace("<br>", " ").Replace("<br/>", " ").Replace("  ", ""))
        {
            switch (ch)
            {
                case '(':
                    AddTerm();
                    continue;
                case ')':
                    AddTerm();
                    continue;
                case '/':
                    AddTerm();
                    continue;
                default:
                    buff[ind++] = ch;
                    continue;
            }
        }

        AddTerm();

        var term = new ImportingTerm(terms[0]);

        if (terms.Count > 1)
        {
            term = term with { AdditionalName = terms[1] };
        }

        if (terms.Count > 2)
        {
            term = term with { Synonyms = terms.Skip(2).Select(s => new Synonym(s)).ToImmutableArray() };
        }

        return term;
    }
}