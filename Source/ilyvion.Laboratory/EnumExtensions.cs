#if !v1_7_OR_GREATER
namespace ilyvion.Laboratory;

/// <summary>
/// Provides the generic <c>Enum</c> overloads (<c>GetValues&lt;TEnum&gt;</c>,
/// <c>GetNames&lt;TEnum&gt;</c>, <c>GetName&lt;TEnum&gt;</c>, <c>IsDefined&lt;TEnum&gt;</c> and,
/// on RimWorld 1.5 and earlier, <c>Parse&lt;TEnum&gt;</c>), which .NET Framework lacks.
/// </summary>
[SinceVersion(0, 26, 0)]
public static class EnumExtensions
{
#pragma warning disable CA1034 // Extension blocks compile to a nested type.
    extension(Enum)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Retrieves an array of the values of the constants in <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
        /// <returns>An array that contains the values of the constants in <typeparamref name="TEnum"/>.</returns>
        public static TEnum[] GetValues<TEnum>()
            where TEnum : struct, Enum => (TEnum[])Enum.GetValues(typeof(TEnum));

        /// <summary>
        /// Retrieves an array of the names of the constants in <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
        /// <returns>A string array of the names of the constants in <typeparamref name="TEnum"/>.</returns>
        public static string[] GetNames<TEnum>()
            where TEnum : struct, Enum => Enum.GetNames(typeof(TEnum));

        /// <summary>
        /// Retrieves the name of the constant in <typeparamref name="TEnum"/> that has
        /// <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
        /// <param name="value">The value of a particular enumerated constant.</param>
        /// <returns>The name of the enumerated constant whose value is <paramref name="value"/>, or <see langword="null"/> if there is no such constant.</returns>
        public static string? GetName<TEnum>(TEnum value)
            where TEnum : struct, Enum => Enum.GetName(typeof(TEnum), value);

        /// <summary>
        /// Returns whether <paramref name="value"/> is the value of a constant in
        /// <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
        /// <param name="value">The value of a constant in <typeparamref name="TEnum"/>.</param>
        /// <returns><see langword="true"/> if a constant in <typeparamref name="TEnum"/> has a value equal to <paramref name="value"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsDefined<TEnum>(TEnum value)
            where TEnum : struct, Enum => Enum.IsDefined(typeof(TEnum), value);

#if !v1_6_OR_GREATER
        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more
        /// enumerated constants to an equivalent <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <returns>A <typeparamref name="TEnum"/> whose value is represented by <paramref name="value"/>.</returns>
        public static TEnum Parse<TEnum>(string value)
            where TEnum : struct, Enum => (TEnum)Enum.Parse(typeof(TEnum), value);

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more
        /// enumerated constants to an equivalent <typeparamref name="TEnum"/>. A parameter
        /// specifies whether the operation is case-insensitive.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <param name="ignoreCase"><see langword="true"/> to ignore case; <see langword="false"/> to regard case.</param>
        /// <returns>A <typeparamref name="TEnum"/> whose value is represented by <paramref name="value"/>.</returns>
        public static TEnum Parse<TEnum>(string value, bool ignoreCase)
            where TEnum : struct, Enum => (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
#endif
    }
}
#endif
