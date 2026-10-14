// EitherTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class EitherTests
{
    // Regression guard for commit 0bf0f96: LeftAnd/RightAnd's XML docs previously described the
    // opposite of what the implementation did (docs said "returns other if Left", the code
    // actually re-wraps this instance's own value if Left, and only falls back to `other` when
    // this instance is the opposite side). The implementation itself didn't change, but these
    // tests pin the actually-correct behavior described by the corrected docs.

    [Test]
    public static void LeftAndReturnsLeftValueRewrappedWhenLeft()
    {
        Either<int, string> either = 1;
        var result = either.LeftAnd(Either<int, bool>.Right(true));

        Expect.IsTrue(result.IsLeft);
        Expect.AreEqual(1, result.UnwrapLeft());
    }

    [Test]
    public static void LeftAndReturnsOtherWhenRight()
    {
        Either<int, string> either = "right";
        var other = Either<int, bool>.Right(true);
        var result = either.LeftAnd(other);

        Expect.IsTrue(ReferenceEquals(result, other));
    }

    [Test]
    public static void LeftAndThrowsOnNullOther()
    {
        Either<int, string> either = 1;

        _ = Expect.Throws<Exception>(() => either.LeftAnd<bool>(null!));
    }

    [Test]
    public static void RightAndReturnsRightValueRewrappedWhenRight()
    {
        Either<int, string> either = "right";
        var result = either.RightAnd(Either<bool, string>.Left(true));

        Expect.IsTrue(result.IsRight);
        Expect.AreEqual("right", result.UnwrapRight());
    }

    [Test]
    public static void RightAndReturnsOtherWhenLeft()
    {
        Either<int, string> either = 1;
        var other = Either<bool, string>.Left(true);
        var result = either.RightAnd(other);

        Expect.IsTrue(ReferenceEquals(result, other));
    }

    [Test]
    public static void RightAndThrowsOnNullOther()
    {
        Either<int, string> either = "right";

        _ = Expect.Throws<Exception>(() => either.RightAnd<bool>(null!));
    }
}
