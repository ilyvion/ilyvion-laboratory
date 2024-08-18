namespace ilyvion.Laboratory;

/// <summary>
/// This type exists mainly because you're not allowed to pass parameters by reference in methods
/// that use yield (return|break) statements (also known as generators), but that means we need a
/// different mechanism for passing value types around -- I opted for manually boxing them.
/// </summary>
/// <typeparam name="T">the boxed value type</typeparam>
/// <param name="initialValue">the boxed value type's initial value</param>
public class Boxed<T>(T initialValue = default) where T : struct
{
    public T Value { get; set; } = initialValue;

#pragma warning disable CA2225, CA1062
    public static implicit operator T(Boxed<T> b) => b.Value;
    public static explicit operator Boxed<T>(T v) => new(v);
#pragma warning restore CA1062, CA2225
}
