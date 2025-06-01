namespace prova2.Models;

using System.Globalization;
using System.Text;

public static class StringExtensions
{
    public static string NormalizeString(this string input)
    {
        if (string.IsNullOrEmpty(input)) {
            return string.Empty;
        }

        string normalized = input.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString().ToLowerInvariant();
    }
}