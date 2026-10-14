#if !v1_7_OR_GREATER
using System.Text.RegularExpressions;

namespace ilyvion.Laboratory;

/// <summary>
/// Provides <c>Regex.Count</c>, which .NET Framework lacks.
/// </summary>
[SinceVersion(0, 25, 0)]
public static class RegexExtensions
{
#pragma warning disable CA1034 // Extension blocks compile to a nested type.
    extension(Regex regex)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Searches <paramref name="input"/> for all occurrences of the regular expression and
        /// returns the number of matches.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <returns>The number of matches.</returns>
        public int Count(string input)
        {
            ArgumentNullException.ThrowIfNull(regex);
            return regex.Matches(input).Count;
        }

        /// <summary>
        /// Searches <paramref name="input"/> for all occurrences of <paramref name="pattern"/> and
        /// returns the number of matches.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <returns>The number of matches.</returns>
        public static int Count(string input, string pattern) =>
            Regex.Matches(input, pattern).Count;

        /// <summary>
        /// Searches <paramref name="input"/> for all occurrences of <paramref name="pattern"/>,
        /// using <paramref name="options"/>, and returns the number of matches.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="options">A bitwise combination of the enumeration values that specify options for matching.</param>
        /// <returns>The number of matches.</returns>
        public static int Count(string input, string pattern, RegexOptions options) =>
            Regex.Matches(input, pattern, options).Count;

        /// <summary>
        /// Searches <paramref name="input"/> for all occurrences of <paramref name="pattern"/>,
        /// using <paramref name="options"/> and <paramref name="matchTimeout"/>, and returns the
        /// number of matches.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="options">A bitwise combination of the enumeration values that specify options for matching.</param>
        /// <param name="matchTimeout">A time-out interval, or <see cref="Regex.InfiniteMatchTimeout"/> to indicate that the method should not time out.</param>
        /// <returns>The number of matches.</returns>
        public static int Count(
            string input,
            string pattern,
            RegexOptions options,
            TimeSpan matchTimeout
        ) => Regex.Matches(input, pattern, options, matchTimeout).Count;
    }
}
#endif
