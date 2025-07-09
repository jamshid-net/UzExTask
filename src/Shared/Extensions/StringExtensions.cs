namespace ProjectTemplate.Shared.Extensions;
public static class StringExtensions
{
    public static string FromCamelCaseToSnakeCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var stringBuilder = new System.Text.StringBuilder();
        var previousCategory = default(System.Globalization.UnicodeCategory?);

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            var currentCategory = char.GetUnicodeCategory(c);

            if (currentCategory == System.Globalization.UnicodeCategory.UppercaseLetter)
            {
                if (i > 0 && previousCategory != System.Globalization.UnicodeCategory.SpaceSeparator &&
                    previousCategory != System.Globalization.UnicodeCategory.UppercaseLetter)
                {
                    stringBuilder.Append('_');
                }

                stringBuilder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                stringBuilder.Append(c);
            }

            previousCategory = currentCategory;
        }

        return stringBuilder.ToString();
    }
}
