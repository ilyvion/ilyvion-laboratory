using HarmonyLib;
using RimWorld.Planet;

namespace ilyvion.Laboratory;

/// <summary>
/// A type that holds either a <typeparamref name="TLeft"/> value or a <typeparamref name="TRight"/>
/// value, never both.
/// </summary>
/// <typeparam name="TLeft">The type of the left value.</typeparam>
/// <typeparam name="TRight">The type of the right value.</typeparam>
[SinceVersion(0, 22, 0)]
public class Either<TLeft, TRight> : IEquatable<Either<TLeft, TRight>>
{
    internal TLeft _left;
    internal TRight _right;
    internal bool _isLeft;

    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="TLeft"/> value.
    /// </summary>
    public bool IsLeft => _isLeft;

    /// <summary>
    /// Gets a value indicating whether this instance holds a <typeparamref name="TRight"/> value.
    /// </summary>
    public bool IsRight => !IsLeft;

    internal Either()
    {
        _left = default!;
        _right = default!;
        _isLeft = true;
    }

    private Either(TLeft left)
    {
        _left = left;
        _right = default!;
        _isLeft = true;
    }

    private Either(TRight right)
    {
        _right = right;
        _left = default!;
        _isLeft = false;
    }

#pragma warning disable CA1000, CA2225
    /// <summary>
    /// Creates an <see cref="Either{TLeft, TRight}"/> holding a <typeparamref name="TLeft"/> value.
    /// </summary>
    /// <param name="left">The value to wrap.</param>
    /// <returns>A new <see cref="Either{TLeft, TRight}"/> in the <c>Left</c> state.</returns>
    public static Either<TLeft, TRight> Left(TLeft left) => new(left);

    /// <summary>
    /// Creates an <see cref="Either{TLeft, TRight}"/> holding a <typeparamref name="TRight"/> value.
    /// </summary>
    /// <param name="right">The value to wrap.</param>
    /// <returns>A new <see cref="Either{TLeft, TRight}"/> in the <c>Right</c> state.</returns>
    public static Either<TLeft, TRight> Right(TRight right) => new(right);

    /// <summary>
    /// Implicitly wraps a <typeparamref name="TLeft"/> value as a <c>Left</c>
    /// <see cref="Either{TLeft, TRight}"/>.
    /// </summary>
    /// <param name="left">The value to wrap.</param>
    public static implicit operator Either<TLeft, TRight>(TLeft left) => Left(left);

    /// <summary>
    /// Implicitly wraps a <typeparamref name="TRight"/> value as a <c>Right</c>
    /// <see cref="Either{TLeft, TRight}"/>.
    /// </summary>
    /// <param name="right">The value to wrap.</param>
    public static implicit operator Either<TLeft, TRight>(TRight right) => Right(right);
#pragma warning restore CA1000, CA2225

    /// <summary>
    /// Invokes <paramref name="leftFunc"/> if this instance is <c>Left</c>, or
    /// <paramref name="rightFunc"/> if it is <c>Right</c>, and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type returned by both functions.</typeparam>
    /// <param name="leftFunc">The function to invoke for a <c>Left</c> value.</param>
    /// <param name="rightFunc">The function to invoke for a <c>Right</c> value.</param>
    /// <returns>The result of whichever function was invoked.</returns>
    public TResult Match<TResult>(Func<TLeft, TResult> leftFunc, Func<TRight, TResult> rightFunc) =>
        leftFunc == null
            ? throw new ArgumentNullException(nameof(leftFunc))
            : (
                rightFunc == null
                    ? throw new ArgumentNullException(nameof(rightFunc))
                    : (IsLeft ? leftFunc(_left) : rightFunc(_right))
            );

    /// <summary>
    /// Transforms the left value with <paramref name="selector"/> if this instance is
    /// <c>Left</c>; otherwise passes the right value through unchanged.
    /// </summary>
    /// <typeparam name="TLeftResult">The type of the transformed left value.</typeparam>
    /// <param name="selector">The function used to transform the left value.</param>
    /// <returns>
    /// An <see cref="Either{TLeftResult, TRight}"/> with the transformed left value, or the
    /// original right value.
    /// </returns>
    public Either<TLeftResult, TRight> MapLeft<TLeftResult>(Func<TLeft, TLeftResult> selector) =>
        selector == null
            ? throw new ArgumentNullException(nameof(selector))
            : (IsLeft ? selector(_left) : _right);

    /// <summary>
    /// Transforms the right value with <paramref name="selector"/> if this instance is
    /// <c>Right</c>; otherwise passes the left value through unchanged.
    /// </summary>
    /// <typeparam name="TRightResult">The type of the transformed right value.</typeparam>
    /// <param name="selector">The function used to transform the right value.</param>
    /// <returns>
    /// An <see cref="Either{TLeft, TRightResult}"/> with the transformed right value, or the
    /// original left value.
    /// </returns>
    public Either<TLeft, TRightResult> MapRight<TRightResult>(
        Func<TRight, TRightResult> selector
    ) =>
        selector == null
            ? throw new ArgumentNullException(nameof(selector))
            : (IsLeft ? _left : selector(_right));

    /// <summary>
    /// Transforms whichever value this instance holds, using <paramref name="selectorLeft"/> for
    /// a <c>Left</c> value or <paramref name="selectorRight"/> for a <c>Right</c> value.
    /// </summary>
    /// <typeparam name="TLeftResult">The type of the transformed left value.</typeparam>
    /// <typeparam name="TRightResult">The type of the transformed right value.</typeparam>
    /// <param name="selectorLeft">The function used to transform a left value.</param>
    /// <param name="selectorRight">The function used to transform a right value.</param>
    /// <returns>An <see cref="Either{TLeftResult, TRightResult}"/> holding the transformed value.</returns>
    public Either<TLeftResult, TRightResult> MapEither<TLeftResult, TRightResult>(
        Func<TLeft, TLeftResult> selectorLeft,
        Func<TRight, TRightResult> selectorRight
    ) =>
        selectorLeft == null
            ? throw new ArgumentNullException(nameof(selectorLeft))
            : (
                selectorRight == null
                    ? throw new ArgumentNullException(nameof(selectorRight))
                    : (IsLeft ? selectorLeft(_left) : selectorRight(_right))
            );

    /// <summary>
    /// Chains another <see cref="Either{TLeftResult, TRight}"/>-producing operation onto a
    /// <c>Left</c> value, without wrapping the result again; a <c>Right</c> value passes through
    /// unchanged.
    /// </summary>
    /// <typeparam name="TLeftResult">The left type produced by <paramref name="binder"/>.</typeparam>
    /// <param name="binder">
    /// The function used to produce the next <see cref="Either{TLeftResult, TRight}"/> from a
    /// left value.
    /// </param>
    /// <returns>
    /// The <see cref="Either{TLeftResult, TRight}"/> returned by <paramref name="binder"/>, or
    /// the original right value.
    /// </returns>
    public Either<TLeftResult, TRight> BindLeft<TLeftResult>(
        Func<TLeft, Either<TLeftResult, TRight>> binder
    ) =>
        binder == null
            ? throw new ArgumentNullException(nameof(binder))
            : (IsLeft ? binder(_left) : _right);

    /// <summary>
    /// Chains another <see cref="Either{TLeft, TRightResult}"/>-producing operation onto a
    /// <c>Right</c> value, without wrapping the result again; a <c>Left</c> value passes through
    /// unchanged.
    /// </summary>
    /// <typeparam name="TRightResult">The right type produced by <paramref name="binder"/>.</typeparam>
    /// <param name="binder">
    /// The function used to produce the next <see cref="Either{TLeft, TRightResult}"/> from a
    /// right value.
    /// </param>
    /// <returns>
    /// The <see cref="Either{TLeft, TRightResult}"/> returned by <paramref name="binder"/>, or
    /// the original left value.
    /// </returns>
    public Either<TLeft, TRightResult> BindRight<TRightResult>(
        Func<TRight, Either<TLeft, TRightResult>> binder
    ) =>
        binder == null
            ? throw new ArgumentNullException(nameof(binder))
            : (IsLeft ? _left : binder(_right));

    /// <summary>
    /// Returns <c>true</c> if this instance is <c>Left</c> and its value satisfies
    /// <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">The predicate to test the left value against.</param>
    /// <returns>
    /// <c>true</c> if this instance is <c>Left</c> and <paramref name="predicate"/> returns
    /// <c>true</c>; otherwise <c>false</c>.
    /// </returns>
    public bool IsLeftAnd(Predicate<TLeft> predicate) =>
        predicate == null
            ? throw new ArgumentNullException(nameof(predicate))
            : (IsLeft && predicate(_left));

    /// <summary>
    /// Returns <c>true</c> if this instance is <c>Right</c> and its value satisfies
    /// <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">The predicate to test the right value against.</param>
    /// <returns>
    /// <c>true</c> if this instance is <c>Right</c> and <paramref name="predicate"/> returns
    /// <c>true</c>; otherwise <c>false</c>.
    /// </returns>
    public bool IsRightAnd(Predicate<TRight> predicate) =>
        predicate == null
            ? throw new ArgumentNullException(nameof(predicate))
            : (IsRight && predicate(_right));

    /// <summary>
    /// Returns <paramref name="other"/> if this instance is <c>Left</c>; otherwise returns the
    /// original right value.
    /// </summary>
    /// <typeparam name="TOtherRight">The right type of <paramref name="other"/>.</typeparam>
    /// <param name="other">The value to return if this instance is <c>Left</c>.</param>
    /// <returns><paramref name="other"/> if this instance is <c>Left</c>; otherwise the original right value.</returns>
    public Either<TLeft, TOtherRight> LeftAnd<TOtherRight>(Either<TLeft, TOtherRight> other) =>
        other == null ? throw new ArgumentNullException(nameof(other)) : (IsLeft ? _left : other);

    /// <summary>
    /// Returns <paramref name="other"/> if this instance is <c>Right</c>; otherwise returns the
    /// original left value.
    /// </summary>
    /// <typeparam name="TOtherLeft">The left type of <paramref name="other"/>.</typeparam>
    /// <param name="other">The value to return if this instance is <c>Right</c>.</param>
    /// <returns><paramref name="other"/> if this instance is <c>Right</c>; otherwise the original left value.</returns>
    public Either<TOtherLeft, TRight> RightAnd<TOtherLeft>(Either<TOtherLeft, TRight> other) =>
        other == null ? throw new ArgumentNullException(nameof(other)) : (IsRight ? _right : other);

    /// <summary>
    /// Returns this instance if it is <c>Left</c>; otherwise returns <paramref name="other"/>
    /// wrapped as a <c>Left</c> value.
    /// </summary>
    /// <param name="other">The left value to fall back to.</param>
    /// <returns>
    /// This instance, or a new <c>Left</c> <see cref="Either{TLeft, TRight}"/> wrapping
    /// <paramref name="other"/>.
    /// </returns>
    public Either<TLeft, TRight> LeftOr(TLeft other) =>
        other == null ? throw new ArgumentNullException(nameof(other)) : (IsLeft ? this : other);

    /// <summary>
    /// Returns this instance if it is <c>Right</c>; otherwise returns <paramref name="other"/>
    /// wrapped as a <c>Right</c> value.
    /// </summary>
    /// <param name="other">The right value to fall back to.</param>
    /// <returns>
    /// This instance, or a new <c>Right</c> <see cref="Either{TLeft, TRight}"/> wrapping
    /// <paramref name="other"/>.
    /// </returns>
    public Either<TLeft, TRight> RightOr(TRight other) =>
        other == null ? throw new ArgumentNullException(nameof(other)) : (IsRight ? this : other);

    /// <summary>
    /// Returns the left value, or throws an <see cref="InvalidOperationException"/> if this
    /// instance is <c>Right</c>.
    /// </summary>
    /// <returns>The left value.</returns>
    public TLeft UnwrapLeft() => ExpectLeft("Called `UnwrapLeft()` on a `Right` value");

    /// <summary>
    /// Returns the right value, or throws an <see cref="InvalidOperationException"/> if this
    /// instance is <c>Left</c>.
    /// </summary>
    /// <returns>The right value.</returns>
    public TRight UnwrapRight() => ExpectRight("Called `UnwrapRight()` on a `Left` value");

    /// <summary>
    /// Returns the left value, or <paramref name="other"/> if this instance is <c>Right</c>.
    /// </summary>
    /// <param name="other">The fallback value.</param>
    /// <returns>The left value, or <paramref name="other"/>.</returns>
    public TLeft UnwrapLeftOr(TLeft other) =>
        other == null ? throw new ArgumentNullException(nameof(other)) : (IsLeft ? _left : other);

    /// <summary>
    /// Returns the right value, or <paramref name="other"/> if this instance is <c>Left</c>.
    /// </summary>
    /// <param name="other">The fallback value.</param>
    /// <returns>The right value, or <paramref name="other"/>.</returns>
    public TRight UnwrapRightOr(TRight other) =>
        other == null ? throw new ArgumentNullException(nameof(other)) : (IsRight ? _right : other);

    /// <summary>
    /// Returns the left value, or the result of invoking <paramref name="other"/> with the right
    /// value if this instance is <c>Right</c>.
    /// </summary>
    /// <param name="other">The function used to compute a fallback value from the right value.</param>
    /// <returns>The left value, or the value computed by <paramref name="other"/>.</returns>
    public TLeft UnwrapLeftOrElse(Func<TRight, TLeft> other) =>
        other == null
            ? throw new ArgumentNullException(nameof(other))
            : (IsLeft ? _left : other(_right));

    /// <summary>
    /// Returns the right value, or the result of invoking <paramref name="other"/> with the left
    /// value if this instance is <c>Left</c>.
    /// </summary>
    /// <param name="other">The function used to compute a fallback value from the left value.</param>
    /// <returns>The right value, or the value computed by <paramref name="other"/>.</returns>
    public TRight UnwrapRightOrElse(Func<TLeft, TRight> other) =>
        other == null
            ? throw new ArgumentNullException(nameof(other))
            : (IsRight ? _right : other(_left));

    /// <summary>
    /// Attempts to get the left value.
    /// </summary>
    /// <param name="left">
    /// When this method returns, contains the left value if this instance is <c>Left</c>;
    /// otherwise the default value.
    /// </param>
    /// <returns><c>true</c> if this instance is <c>Left</c>; otherwise <c>false</c>.</returns>
    public bool TryGetLeft(out TLeft left)
    {
        left = _left;
        return IsLeft;
    }

    /// <summary>
    /// Attempts to get the right value.
    /// </summary>
    /// <param name="right">
    /// When this method returns, contains the right value if this instance is <c>Right</c>;
    /// otherwise the default value.
    /// </param>
    /// <returns><c>true</c> if this instance is <c>Right</c>; otherwise <c>false</c>.</returns>
    public bool TryGetRight(out TRight right)
    {
        right = _right;
        return IsRight;
    }

    /// <summary>
    /// Deconstructs this instance into its constituent parts.
    /// </summary>
    /// <param name="isLeft"><c>true</c> if this instance is <c>Left</c>; otherwise <c>false</c>.</param>
    /// <param name="left">The left value if this instance is <c>Left</c>; otherwise the default value.</param>
    /// <param name="right">The right value if this instance is <c>Right</c>; otherwise the default value.</param>
    public void Deconstruct(out bool isLeft, out TLeft left, out TRight right)
    {
        isLeft = IsLeft;
        left = _left;
        right = _right;
    }

    /// <summary>
    /// Invokes <paramref name="action"/> with the left value if this instance is <c>Left</c>,
    /// then returns this instance unchanged.
    /// </summary>
    /// <param name="action">The action to invoke for a left value.</param>
    /// <returns>This instance.</returns>
    public Either<TLeft, TRight> IfLeft(Action<TLeft> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        if (IsLeft)
        {
            action(_left);
        }
        return this;
    }

    /// <summary>
    /// Invokes <paramref name="action"/> with the right value if this instance is <c>Right</c>,
    /// then returns this instance unchanged.
    /// </summary>
    /// <param name="action">The action to invoke for a right value.</param>
    /// <returns>This instance.</returns>
    public Either<TLeft, TRight> IfRight(Action<TRight> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        if (IsRight)
        {
            action(_right);
        }
        return this;
    }

    /// <summary>
    /// Returns the left value, or throws an <see cref="InvalidOperationException"/> with
    /// <paramref name="message"/> if this instance is <c>Right</c>.
    /// </summary>
    /// <param name="message">The exception message to use if this instance is <c>Right</c>.</param>
    /// <returns>The left value.</returns>
    public TLeft ExpectLeft(string message) =>
        IsRight ? throw new InvalidOperationException(message) : _left;

    /// <summary>
    /// Returns the right value, or throws an <see cref="InvalidOperationException"/> with
    /// <paramref name="message"/> if this instance is <c>Left</c>.
    /// </summary>
    /// <param name="message">The exception message to use if this instance is <c>Left</c>.</param>
    /// <returns>The right value.</returns>
    public TRight ExpectRight(string message) =>
        IsLeft ? throw new InvalidOperationException(message) : _right;

    /// <summary>
    /// Swaps the left and right sides of this instance.
    /// </summary>
    /// <returns>An <see cref="Either{TRight, TLeft}"/> holding the same value, with left and right reversed.</returns>
    public Either<TRight, TLeft> Flip() => IsLeft ? _left : _right;

    /// <summary>
    /// Determines whether this instance and <paramref name="other"/> hold the same side with
    /// equal values.
    /// </summary>
    /// <param name="other">The instance to compare against.</param>
    /// <returns>
    /// <c>true</c> if both instances hold the same side and their values are equal; otherwise
    /// <c>false</c>.
    /// </returns>
    public bool Equals(Either<TLeft, TRight>? other) =>
        other is not null
        && IsLeft == other.IsLeft
        && (
            IsLeft
                ? EqualityComparer<TLeft>.Default.Equals(_left, other._left)
                : EqualityComparer<TRight>.Default.Equals(_right, other._right)
        );

    /// <inheritdoc cref="Equals(Either{TLeft, TRight}?)"/>
    public override bool Equals(object? obj) => obj is Either<TLeft, TRight> other && Equals(other);

    /// <summary>
    /// Returns a hash code based on the side and the current value.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode() =>
        IsLeft ? HashCode.Combine(true, _left) : HashCode.Combine(false, _right);

    /// <summary>
    /// Determines whether two <see cref="Either{TLeft, TRight}"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise <c>false</c>.</returns>
    public static bool operator ==(Either<TLeft, TRight>? left, Either<TLeft, TRight>? right)
    {
        return left is null ? right is null : left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Either{TLeft, TRight}"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise <c>false</c>.</returns>
    public static bool operator !=(Either<TLeft, TRight>? left, Either<TLeft, TRight>? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Returns a string representation of this instance, indicating the side and its value.
    /// </summary>
    /// <returns>A string in the form <c>Left(value)</c> or <c>Right(value)</c>.</returns>
    public override string ToString() =>
        IsLeft
            ? $"Left({_left?.ToString() ?? "<null>"})"
            : $"Right({_right?.ToString() ?? "<null>"})";
}

/// <summary>
/// Provides extension methods for working with sequences of <see cref="Either{TLeft, TRight}"/> values.
/// </summary>
[SinceVersion(0, 22, 0)]
public static class EitherExtensions
{
    /// <summary>
    /// Filters a sequence of <see cref="Either{TLeft, TRight}"/> values down to the left values.
    /// </summary>
    /// <typeparam name="TLeft">The left type.</typeparam>
    /// <typeparam name="TRight">The right type.</typeparam>
    /// <param name="source">The sequence to filter.</param>
    /// <returns>A sequence containing the left value of each <c>Left</c> element in <paramref name="source"/>.</returns>
    public static IEnumerable<TLeft> Lefts<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> source
    )
    {
        return source == null
            ? throw new ArgumentNullException(nameof(source))
            : LeftsIterator(source);

        static IEnumerable<TLeft> LeftsIterator(IEnumerable<Either<TLeft, TRight>> source)
        {
            foreach (var either in source)
            {
                if (either.TryGetLeft(out var left))
                {
                    yield return left;
                }
            }
        }
    }

    /// <summary>
    /// Filters a sequence of <see cref="Either{TLeft, TRight}"/> values down to the right values.
    /// </summary>
    /// <typeparam name="TLeft">The left type.</typeparam>
    /// <typeparam name="TRight">The right type.</typeparam>
    /// <param name="source">The sequence to filter.</param>
    /// <returns>A sequence containing the right value of each <c>Right</c> element in <paramref name="source"/>.</returns>
    public static IEnumerable<TRight> Rights<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> source
    )
    {
        return source == null
            ? throw new ArgumentNullException(nameof(source))
            : RightsIterator(source);

        static IEnumerable<TRight> RightsIterator(IEnumerable<Either<TLeft, TRight>> source)
        {
            foreach (var either in source)
            {
                if (either.TryGetRight(out var right))
                {
                    yield return right;
                }
            }
        }
    }

    /// <summary>
    /// Splits a sequence of <see cref="Either{TLeft, TRight}"/> values into its left and right values.
    /// </summary>
    /// <typeparam name="TLeft">The left type.</typeparam>
    /// <typeparam name="TRight">The right type.</typeparam>
    /// <param name="source">The sequence to partition.</param>
    /// <returns>
    /// A tuple containing the left values and the right values, each in their original relative
    /// order.
    /// </returns>
    public static (List<TLeft> Lefts, List<TRight> Rights) Partition<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> source
    )
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var lefts = new List<TLeft>();
        var rights = new List<TRight>();
        foreach (var either in source)
        {
            if (either.TryGetLeft(out var left))
            {
                lefts.Add(left);
            }
            else if (either.TryGetRight(out var right))
            {
                rights.Add(right);
            }
        }
        return (lefts, rights);
    }
}

/// <summary>
/// Provides <c>Scribe</c> save/load support for <see cref="Either{TLeft, TRight}"/>, including a
/// variant that maps to intermediate types for (de)serialization.
/// </summary>
[SinceVersion(0, 22, 0)]
public static class Scribe_Either
{
    /// <summary>
    /// Saves or loads an <see cref="Either{TLeft, TRight}"/> by mapping it to an intermediate
    /// <see cref="Either{TLeftResult, TRightResult}"/> for (de)serialization, then mapping it back.
    /// </summary>
    /// <typeparam name="TLeft">The left type of the value being saved or loaded.</typeparam>
    /// <typeparam name="TRight">The right type of the value being saved or loaded.</typeparam>
    /// <typeparam name="TLeftResult">The intermediate left type used for (de)serialization.</typeparam>
    /// <typeparam name="TRightResult">The intermediate right type used for (de)serialization.</typeparam>
    /// <param name="either">The value being saved or loaded.</param>
    /// <param name="tmpMappedEither">Scratch storage for the intermediate mapped value.</param>
    /// <param name="label">The XML node label.</param>
    /// <param name="mapLeft">Converts a left value to its intermediate representation for saving.</param>
    /// <param name="mapRight">Converts a right value to its intermediate representation for saving.</param>
    /// <param name="mapLeftBack">Converts an intermediate left value back after loading.</param>
    /// <param name="mapRightBack">Converts an intermediate right value back after loading.</param>
    /// <param name="mappedLeftLookMode">The <see cref="LookMode"/> to use for the intermediate left value.</param>
    /// <param name="mappedRightLookMode">The <see cref="LookMode"/> to use for the intermediate right value.</param>
    /// <param name="forceSave">Whether to force saving even if the value equals its default.</param>
    /// <param name="saveDestroyedThings">Whether to save references to destroyed things.</param>
    /// <param name="preserveDefaultValues">Whether to preserve default values.</param>
    /// <param name="leftLabel">The XML node label for the left value.</param>
    /// <param name="rightLabel">The XML node label for the right value.</param>
    public static void LookMapped<TLeft, TRight, TLeftResult, TRightResult>(
        ref Either<TLeft, TRight>? either,
        ref Either<TLeftResult, TRightResult>? tmpMappedEither,
        string label,
        Func<TLeft, TLeftResult> mapLeft,
        Func<TRight, TRightResult> mapRight,
        Func<TLeftResult, TLeft> mapLeftBack,
        Func<TRightResult, TRight> mapRightBack,
        LookMode mappedLeftLookMode = LookMode.Undefined,
        LookMode mappedRightLookMode = LookMode.Undefined,
        bool forceSave = false,
        bool saveDestroyedThings = false,
        bool preserveDefaultValues = false,
        string leftLabel = "left",
        string rightLabel = "right"
    )
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            tmpMappedEither = either?.Match(
                left => Either<TLeftResult, TRightResult>.Left(mapLeft(left)),
                right => Either<TLeftResult, TRightResult>.Right(mapRight(right))
            );
        }

        Look(
            ref tmpMappedEither,
            label,
            default!,
            default!,
            mappedLeftLookMode,
            mappedRightLookMode,
            forceSave,
            saveDestroyedThings,
            preserveDefaultValues,
            leftLabel,
            rightLabel
        );

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            either = tmpMappedEither?.Match(
                left => Either<TLeft, TRight>.Left(mapLeftBack(left)),
                right => Either<TLeft, TRight>.Right(mapRightBack(right))
            );

            tmpMappedEither = null;
        }
    }
#pragma warning disable CA1062 // either is nullable by design; null is a valid, explicitly-handled state
    /// <summary>
    /// Saves or loads an <see cref="Either{TLeft, TRight}"/> directly.
    /// </summary>
    /// <typeparam name="TLeft">The left type.</typeparam>
    /// <typeparam name="TRight">The right type.</typeparam>
    /// <param name="either">The value being saved or loaded.</param>
    /// <param name="label">The XML node label.</param>
    /// <param name="defaultLeft">The default left value.</param>
    /// <param name="defaultRight">The default right value.</param>
    /// <param name="leftLookMode">The <see cref="LookMode"/> to use for the left value.</param>
    /// <param name="rightLookMode">The <see cref="LookMode"/> to use for the right value.</param>
    /// <param name="forceSave">Whether to force saving even if the value equals its default.</param>
    /// <param name="saveDestroyedThings">Whether to save references to destroyed things.</param>
    /// <param name="preserveDefaultValues">Whether to preserve default values.</param>
    /// <param name="leftLabel">The XML node label for the left value.</param>
    /// <param name="rightLabel">The XML node label for the right value.</param>
    public static void Look<TLeft, TRight>(
        ref Either<TLeft, TRight>? either,
        string label,
        TLeft defaultLeft = default!,
        TRight defaultRight = default!,
        LookMode leftLookMode = LookMode.Undefined,
        LookMode rightLookMode = LookMode.Undefined,
        bool forceSave = false,
        bool saveDestroyedThings = false,
        bool preserveDefaultValues = false,
        string leftLabel = "left",
        string rightLabel = "right"
    )
    {
        if (Scribe.EnterNode(label))
        {
            try
            {
                if (Scribe.mode == LoadSaveMode.Saving && either == null)
                {
                    Scribe.saver.WriteAttribute("IsNull", "True");
                }
                else
                {
                    if (Scribe.mode == LoadSaveMode.LoadingVars)
                    {
                        var xmlAttribute = Scribe.loader.curXmlParent.Attributes["IsNull"];
                        either =
                            xmlAttribute != null
                            && xmlAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase)
                                ? null
                                : new Either<TLeft, TRight>();
                    }
                    if (Scribe.mode == LoadSaveMode.Saving || either != null)
                    {
                        Scribe_Values.Look(
                            ref either!._isLeft,
                            $"is{leftLabel.CapitalizeFirst()}",
                            true,
                            true
                        );
                        if (either!._isLeft)
                        {
                            LookValue(
                                ref either!._left,
                                leftLabel,
                                leftLookMode,
                                defaultLeft,
                                forceSave,
                                saveDestroyedThings,
                                preserveDefaultValues
                            );
                        }
                        else
                        {
                            LookValue(
                                ref either!._right,
                                rightLabel,
                                rightLookMode,
                                defaultRight,
                                forceSave,
                                saveDestroyedThings,
                                preserveDefaultValues
                            );
                        }
                    }
                }
                return;
            }
            finally
            {
                Scribe.ExitNode();
            }
        }
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            either = null;
        }
    }
#pragma warning restore CA1062

    private static void LookValue<T>(
        ref T value,
        string label,
        LookMode lookMode,
        T defaultValue,
        bool forceSave,
        bool saveDestroyedThings,
        bool preserveDefaultValues
    )
    {
        if (
            lookMode == LookMode.Undefined
            && !Scribe_Universal.TryResolveLookMode(typeof(T), out lookMode)
        )
        {
            Log.Error(
                $"Scribe_Either.Look call with a value of type '{typeof(T).ToStringSafe()}' must have lookMode set explicitly."
            );
            return;
        }

        switch (lookMode)
        {
            case LookMode.Undefined:
                throw new InvalidOperationException(
                    "We've guarded against this case above, so it should never happen."
                );
            case LookMode.Value:
                Scribe_Values.Look(ref value, label, defaultValue, forceSave);
                break;
            case LookMode.Deep:
                Scribe_Deep.Look(ref value, label, forceSave);
                break;
            case LookMode.Reference:
                if (value is not null and not ILoadReferenceable)
                {
                    throw new InvalidOperationException(
                        $"Cannot save reference to value of type '{typeof(T).ToStringSafe()}' if it is not ILoadReferenceable"
                    );
                }
                var refee = value as ILoadReferenceable;
                Scribe_References.Look(ref refee, label, saveDestroyedThings);
                break;
            case LookMode.Def:
                if (value is not null and not Def)
                {
                    throw new InvalidOperationException(
                        $"Cannot save value of type '{typeof(T).ToStringSafe()}' if it is not a Def"
                    );
                }

                if (value is null && Scribe.mode == LoadSaveMode.Saving)
                {
                    var def = value as Def;
                    Scribe_Defs.Look(ref def, label);
                }
                else
                {
                    // We need to use the most specific Def type available when calling
                    // Scribe_Defs.Look, so we use reflection to get the most specific
                    // generic method definition that we can.
                    var method = AccessTools.Method(typeof(Scribe_Defs), nameof(Scribe_Defs.Look));
                    var genericMethod = method.MakeGenericMethod(typeof(T));

                    var parameters = new object?[] { value, label };

                    _ = genericMethod.Invoke(null, parameters);
                    value = (T)parameters[0]!;
                }
                break;
            case LookMode.LocalTargetInfo:
                if (value is not LocalTargetInfo localTargetInfo)
                {
                    throw new InvalidOperationException(
                        $"Cannot save value of type '{typeof(T).ToStringSafe()}' with lookMode LocalTargetInfo"
                    );
                }
                if (defaultValue is not LocalTargetInfo defaultLocalTargetInfo)
                {
                    defaultLocalTargetInfo = LocalTargetInfo.Invalid;
                }
                Scribe_TargetInfo.Look(
                    ref localTargetInfo,
                    saveDestroyedThings,
                    label,
                    defaultLocalTargetInfo,
                    preserveDefaultValues
                );
                break;
            case LookMode.TargetInfo:
                if (value is not TargetInfo targetInfo)
                {
                    throw new InvalidOperationException(
                        $"Cannot save value of type '{typeof(T).ToStringSafe()}' with lookMode TargetInfo"
                    );
                }
                if (defaultValue is not TargetInfo defaultTargetInfo)
                {
                    defaultTargetInfo = TargetInfo.Invalid;
                }
                Scribe_TargetInfo.Look(
                    ref targetInfo,
                    saveDestroyedThings,
                    label,
                    defaultTargetInfo,
                    preserveDefaultValues
                );
                break;
            case LookMode.GlobalTargetInfo:
                if (value is not GlobalTargetInfo globalTargetInfo)
                {
                    throw new InvalidOperationException(
                        $"Cannot save value of type '{typeof(T).ToStringSafe()}' with lookMode GlobalTargetInfo"
                    );
                }
                if (defaultValue is not GlobalTargetInfo defaultGlobalTargetInfo)
                {
                    defaultGlobalTargetInfo = GlobalTargetInfo.Invalid;
                }
                Scribe_TargetInfo.Look(
                    ref globalTargetInfo,
                    saveDestroyedThings,
                    label,
                    defaultGlobalTargetInfo,
                    preserveDefaultValues
                );
                break;
            case LookMode.BodyPart:
                if (value is not BodyPartRecord bodyPartRecord)
                {
                    throw new InvalidOperationException(
                        $"Cannot save value of type '{typeof(T).ToStringSafe()}' with lookMode BodyPart"
                    );
                }
                var defaultBodyPartRecord = defaultValue as BodyPartRecord;
                Scribe_BodyParts.Look(ref bodyPartRecord, label, defaultBodyPartRecord);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lookMode), lookMode, null);
        }
    }
}
