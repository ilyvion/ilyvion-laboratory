namespace ilyvion.Laboratory;

/// <summary>
/// This type exists mainly because you're not allowed to pass parameters by reference in methods
/// that use yield (return|break) statements (also known as generators), but that means we need a
/// different mechanism for passing value types around -- I opted for manually boxing them.
/// </summary>
/// <typeparam name="T">the boxed value type</typeparam>
/// <param name="initialValue">the boxed value type's initial value</param>
public class Boxed<T>(T initialValue = default)
    where T : struct
{
    private T value = initialValue;
    public T Value
    {
        get => value;
        set => this.value = value;
    }

    [SinceVersion(0, 19, 0)]
    public ref T RefValue => ref value;

    public AnyBoxed<T> ToAnyBoxed()
    {
        return new(Value);
    }

#pragma warning disable CA2225, CA1062
    public static implicit operator T(Boxed<T> b) => b.Value;

    public static explicit operator Boxed<T>(T v) => new(v);
#pragma warning restore CA1062, CA2225
}

/// <summary>
/// This type exists mainly because you're not allowed to pass parameters by reference in methods
/// that use yield (return|break) statements (also known as generators), but that means we need a
/// different mechanism for passing value types around -- I opted for manually boxing them.
/// </summary>
/// <typeparam name="T">the boxed value type</typeparam>
/// <param name="initialValue">the boxed value type's initial value</param>
public class AnyBoxed<T>(T initialValue)
{
    private T value = initialValue;
    public T Value
    {
        get => value;
        set => this.value = value;
    }

    [SinceVersion(0, 19, 0)]
    public ref T RefValue => ref value;

#pragma warning disable CA2225, CA1062
    public static implicit operator T(AnyBoxed<T> b) => b.Value;

    public static explicit operator AnyBoxed<T>(T v) => new(v);
#pragma warning restore CA1062, CA2225
}
