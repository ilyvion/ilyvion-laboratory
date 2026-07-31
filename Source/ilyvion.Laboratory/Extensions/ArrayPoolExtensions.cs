using System.Buffers;

namespace ilyvion.Laboratory.Extensions;

public static class ArrayPoolExtensions
{
    public static SelfReturningRent<T> RentWithSelfReturn<T>(
        this ArrayPool<T> arrayPool,
        int minimumLength
    ) => new(arrayPool, minimumLength);
}

public struct SelfReturningRent<T>(ArrayPool<T> arrayPool, int minimumLength)
    : IDisposable,
        IEquatable<SelfReturningRent<T>>
{
    private readonly ArrayPool<T> arrayPool = arrayPool;
    private T[]? arr = arrayPool.Rent(minimumLength);
#pragma warning disable CA1819
    public readonly T[] Arr => arr!;
#pragma warning restore CA1819

    public readonly T this[int index]
    {
        get => arr![index];
        set => arr![index] = value;
    }

    public void Dispose()
    {
        if (arr == null)
        {
            return;
        }
        arrayPool.Return(arr);
        arr = null;
    }

    public override readonly bool Equals(object obj) =>
        obj is SelfReturningRent<T> safeRent && Equals(safeRent);

    public override readonly int GetHashCode() => arr?.GetHashCode() ?? 0;

    public static bool operator ==(SelfReturningRent<T> left, SelfReturningRent<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SelfReturningRent<T> left, SelfReturningRent<T> right)
    {
        return !(left == right);
    }

    public readonly bool Equals(SelfReturningRent<T> other) => arr?.Equals(other.arr) ?? false;
}
