// InjectedFieldAttribute.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

#if !v1_7_OR_GREATER
namespace Waylith.Injection;

/// <summary>
/// RimWorld 1.7's <c>Waylith.Injection.InjectedFieldAttribute</c>, for RimWorld 1.6 and earlier.
/// An instance extension property (or a <see langword="static"/> accessor method in a
/// <see langword="static"/> class taking the target instance) marked with it gets its own
/// storage on each instance of the target class: on RimWorld 1.7 the game adds a real field to
/// the class; on earlier versions ilyvion's Laboratory keeps the values in a
/// <see cref="System.Runtime.CompilerServices.ConditionalWeakTable{TKey, TValue}"/>.
/// </summary>
/// <remarks>
/// The accessors' own bodies are replaced at startup, so they only need to compile; give them a
/// body that throws. On RimWorld 1.6 and earlier, accessors declared <see langword="extern"/>
/// can't be replaced.
/// </remarks>
[ilyvion.Laboratory.SinceVersion(0, 29, 0)]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class InjectedFieldAttribute : Attribute
{
    /// <summary>
    /// The field's initial value. Only a primitive, enum or <see cref="string"/> field can have
    /// one; use <see cref="Initializer"/> for other types.
    /// </summary>
    public object? Default { get; set; }

    /// <summary>
    /// The name of a <see langword="static"/> method on the class declaring the accessor that
    /// makes the field's initial value. It takes the target instance or nothing, and returns the
    /// field's type.
    /// </summary>
    public string? Initializer { get; set; }
}
#endif
