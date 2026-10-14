#if !v1_7_OR_GREATER
using System.Security.Cryptography;

namespace ilyvion.Laboratory;

/// <summary>
/// Provides <c>SHA256.HashData</c>, which .NET Framework lacks.
/// </summary>
[SinceVersion(0, 25, 0)]
public static class SHA256Extensions
{
#pragma warning disable CA1034 // Extension blocks compile to a nested type.
    extension(SHA256)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Computes the SHA256 hash of <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The data to hash.</param>
        /// <returns>The hash of the data.</returns>
        public static byte[] HashData(byte[] source)
        {
            ArgumentNullException.ThrowIfNull(source);
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(source);
        }
    }
}
#endif
