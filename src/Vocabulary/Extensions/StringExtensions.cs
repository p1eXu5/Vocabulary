namespace Vocabulary.Extensions;

public static class StringExtensions
{
    public static bool NotNullOrWhiteSpace(this string? value)
        => !string.IsNullOrWhiteSpace(value);
}