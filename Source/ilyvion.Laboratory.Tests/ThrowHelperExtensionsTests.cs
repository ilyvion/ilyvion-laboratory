// ThrowHelperExtensionsTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class ThrowHelperExtensionsTests
{
    private static TException? Catch<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException e)
        {
            return e;
        }
        return null;
    }

    [Test]
    public static void ThrowIfNullThrowsWithArgumentNameOnNull()
    {
        object? argument = null;

        var caught = Catch<ArgumentNullException>(() =>
            ArgumentNullException.ThrowIfNull(argument)
        );

        Assert.That(caught?.GetType() == typeof(ArgumentNullException)).Is.True();
        Assert.That(caught?.ParamName).Is.EqualTo(nameof(argument));
    }

    [Test]
    public static void ThrowIfNullDoesNotThrowOnNonNull()
    {
        var argument = new object();

        var caught = Catch<ArgumentNullException>(() =>
            ArgumentNullException.ThrowIfNull(argument)
        );

        Assert.That(caught == null).Is.True();
    }

    [Test]
    public static void ThrowIfNegativeThrowsWithArgumentNameAndValueOnNegative()
    {
        var count = -1;

        var caught = Catch<ArgumentOutOfRangeException>(() =>
            ArgumentOutOfRangeException.ThrowIfNegative(count)
        );

        Assert.That(caught?.GetType() == typeof(ArgumentOutOfRangeException)).Is.True();
        Assert.That(caught?.ParamName).Is.EqualTo(nameof(count));
        Assert.That(Equals(caught?.ActualValue, -1)).Is.True();
    }

    [Test]
    public static void SignHelpersAcceptAndRejectTheExpectedValues()
    {
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfNegative(0)
                ) == null
            )
            .Is.True();
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfZero(0))
                    != null
            )
            .Is.True();
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfZero(-1f)
                ) == null
            )
            .Is.True();
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(0L)
                ) != null
            )
            .Is.True();
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(0.5)
                ) == null
            )
            .Is.True();
    }

    [Test]
    public static void ComparisonHelpersAcceptAndRejectTheExpectedValues()
    {
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfEqual(1, 1)
                ) != null
            )
            .Is.True();
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfNotEqual(1, 1)
                ) == null
            )
            .Is.True();
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfGreaterThan(2, 1)
                ) != null
            )
            .Is.True();
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfGreaterThan(1, 1)
                ) == null
            )
            .Is.True();
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(1, 1)
                ) != null
            )
            .Is.True();
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfLessThan(0, 1)
                ) != null
            )
            .Is.True();
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfLessThan(1, 1)
                ) == null
            )
            .Is.True();
        Assert
            .That(
                Catch<ArgumentOutOfRangeException>(() =>
                    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(1, 1)
                ) != null
            )
            .Is.True();
    }
}
