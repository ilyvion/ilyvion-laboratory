#if !v1_7_OR_GREATER
namespace ilyvion.Laboratory;

/// <summary>
/// Provides <c>Convert.ToHexString</c>, which .NET Framework lacks.
/// </summary>
[SinceVersion(0, 25, 0)]
public static class ConvertExtensions
{
#pragma warning disable CA1034 // Extension blocks compile to a nested type.
    extension(Convert)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Converts <paramref name="inArray"/> to its equivalent string representation, encoded
        /// with uppercase hex characters.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <returns>The string representation in hex of the elements in <paramref name="inArray"/>.</returns>
        public static string ToHexString(byte[] inArray)
        {
            ArgumentNullException.ThrowIfNull(inArray);
            return Convert.ToHexString(inArray, 0, inArray.Length);
        }

        /// <summary>
        /// Converts <paramref name="length"/> elements of <paramref name="inArray"/>, starting at
        /// <paramref name="offset"/>, to their equivalent string representation, encoded with
        /// uppercase hex characters.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <param name="offset">An offset in <paramref name="inArray"/>.</param>
        /// <param name="length">The number of elements of <paramref name="inArray"/> to convert.</param>
        /// <returns>The string representation in hex of <paramref name="length"/> elements of <paramref name="inArray"/>, starting at <paramref name="offset"/>.</returns>
        public static string ToHexString(byte[] inArray, int offset, int length)
        {
            ArgumentNullException.ThrowIfNull(inArray);
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            if (offset > inArray.Length - length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(offset),
                    "Offset and length must refer to a position in the array."
                );
            }

            const string Digits = "0123456789ABCDEF";
            var chars = new char[length * 2];
            for (var i = 0; i < length; i++)
            {
                var value = inArray[offset + i];
                chars[i * 2] = Digits[value >> 4];
                chars[(i * 2) + 1] = Digits[value & 0xF];
            }
            return new string(chars);
        }
    }
}
#endif
