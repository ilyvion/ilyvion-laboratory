using System.Buffers;

namespace ilyvion.Laboratory.Extensions;

public static class ArrayPoolExtensions
{
    public static SelfReturningRent<T> RentWithSelfReturn<T>(
        this ArrayPool<T> arrayPool,
        int minimumLength
    ) => new(arrayPool, minimumLength);
}

public readonly struct SelfReturningRent<T>(ArrayPool<T> arrayPool, int minimumLength)
    : IDisposable,
        IEquatable<SelfReturningRent<T>>
{
    // Holding the rented array behind a shared reference-type box (rather than
    // directly in a struct field) means every copy of this struct sees the same
    // disposal state, so returning the array through one copy correctly prevents
    // any other copy from also returning it.
    private sealed class Box(T[] arr)
    {
        public T[]? Arr = arr;
    }

    private readonly ArrayPool<T> arrayPool = arrayPool;
    private readonly Box box = new(arrayPool.Rent(minimumLength));

#pragma warning disable CA1819
    public readonly T[] Arr => box.Arr!;
#pragma warning restore CA1819

    public readonly T this[int index]
    {
        get => box.Arr![index];
        set => box.Arr![index] = value;
    }

    public readonly void Dispose()
    {
        var arr = box.Arr;
        if (arr == null)
        {
            return;
        }
        box.Arr = null;
        arrayPool.Return(arr);
    }

    public override readonly bool Equals(object obj) =>
        obj is SelfReturningRent<T> safeRent && Equals(safeRent);

    public override readonly int GetHashCode() => box.Arr?.GetHashCode() ?? 0;

    public static bool operator ==(SelfReturningRent<T> left, SelfReturningRent<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SelfReturningRent<T> left, SelfReturningRent<T> right)
    {
        return !(left == right);
    }

    public readonly bool Equals(SelfReturningRent<T> other) =>
        box.Arr?.Equals(other.box.Arr) ?? false;
}
