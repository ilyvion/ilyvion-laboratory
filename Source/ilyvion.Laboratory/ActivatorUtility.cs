// ActivatorUtility.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

namespace ilyvion.Laboratory;

/// <summary>
/// Provides a non-null, typed wrapper around <see cref="Activator.CreateInstance(Type, object[])"/>.
/// </summary>
[SinceVersion(0, 26, 0)]
public static class ActivatorUtility
{
    /// <summary>
    /// Creates an instance of <paramref name="type"/> with <paramref name="args"/> as its
    /// constructor arguments, cast to <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="type"/> is null, or no instance was created.
    /// </exception>
    public static T CreateInstance<T>(Type? type, params object?[] args) =>
        (T)(
            Activator.CreateInstance(
                type
                    ?? throw new InvalidOperationException(
                        $"Cannot create a {typeof(T).Name} instance without a type."
                    ),
                args
            ) ?? throw new InvalidOperationException($"Could not create an instance of {type}.")
        );
}
