// SHA256ExtensionsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using System.Security.Cryptography;
using System.Text;
using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class SHA256ExtensionsTests
{
    [Test]
    public static void HashDataReturnsTheSha256HashOfTheData()
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes("abc"));

        Expect.AreEqual(
            "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD",
            Convert.ToHexString(hash)
        );
    }

    [Test]
    public static void HashDataOfNoBytesIsTheEmptyHash()
    {
        var hash = SHA256.HashData([]);

        Expect.AreEqual(
            "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855",
            Convert.ToHexString(hash)
        );
    }
}
