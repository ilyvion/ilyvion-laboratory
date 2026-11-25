// InjectedFieldStorage.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using System.ComponentModel;

namespace ilyvion.Laboratory;

/// <summary>
/// Holds the values of injected fields (see <c>Waylith.Injection.InjectedFieldAttribute</c>) on
/// RimWorld 1.6 and earlier. The accessors' replaced bodies call these methods; mods don't call
/// them directly.
/// </summary>
[SinceVersion(0, 29, 0)]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class InjectedFieldStorage
{
    private static readonly Lock gate = new();

    /// <summary>Gets a reference to the value of field <paramref name="slot"/> on <paramref name="target"/>.</summary>
    /// <typeparam name="TTarget">The class the field is on.</typeparam>
    /// <typeparam name="TField">The field's type.</typeparam>
    /// <param name="slot">The field's slot.</param>
    /// <param name="target">The instance whose field to get.</param>
    /// <returns>A reference to the value.</returns>
    public static ref TField Ref<TTarget, TField>(int slot, TTarget target)
        where TTarget : class => ref Slots<TTarget, TField>.All[slot].Ref(target);

    /// <summary>Gets the value of field <paramref name="slot"/> on <paramref name="target"/>.</summary>
    /// <typeparam name="TTarget">The class the field is on.</typeparam>
    /// <typeparam name="TField">The field's type.</typeparam>
    /// <param name="slot">The field's slot.</param>
    /// <param name="target">The instance whose field to get.</param>
    /// <returns>The value.</returns>
    public static TField Get<TTarget, TField>(int slot, TTarget target)
        where TTarget : class => Slots<TTarget, TField>.All[slot].Ref(target);

    /// <summary>Sets the value of field <paramref name="slot"/> on <paramref name="target"/>.</summary>
    /// <typeparam name="TTarget">The class the field is on.</typeparam>
    /// <typeparam name="TField">The field's type.</typeparam>
    /// <param name="slot">The field's slot.</param>
    /// <param name="target">The instance whose field to set.</param>
    /// <param name="value">The new value.</param>
    public static void Set<TTarget, TField>(int slot, TTarget target, TField value)
        where TTarget : class => Slots<TTarget, TField>.All[slot].Ref(target) = value;

    /// <summary>
    /// Adds a field of type <typeparamref name="TField"/> to <typeparamref name="TTarget"/>.
    /// </summary>
    /// <param name="defaultValue">The initial value, already converted to <typeparamref name="TField"/>.</param>
    /// <param name="initializer">
    /// A static method making the initial value, taking the instance or nothing; it takes
    /// precedence over <paramref name="defaultValue"/>.
    /// </param>
    /// <returns>The new field's slot.</returns>
    internal static int AddSlot<TTarget, TField>(object? defaultValue, MethodInfo? initializer)
        where TTarget : class
    {
        Func<TTarget, TField> initialValue;
        if (initializer is null)
        {
            var value = defaultValue is null ? default! : (TField)defaultValue;
            initialValue = _ => value;
        }
        else if (initializer.GetParameters().Length == 0)
        {
            var create = (Func<TField>)Delegate.CreateDelegate(typeof(Func<TField>), initializer);
            initialValue = _ => create();
        }
        else
        {
            initialValue =
                (Func<TTarget, TField>)
                    Delegate.CreateDelegate(typeof(Func<TTarget, TField>), initializer);
        }

        lock (gate)
        {
            var slots = Slots<TTarget, TField>.All;
            var grown = new Slot<TTarget, TField>[slots.Length + 1];
            Array.Copy(slots, grown, slots.Length);
            grown[slots.Length] = new Slot<TTarget, TField>(initialValue);
            Slots<TTarget, TField>.All = grown;
            return slots.Length;
        }
    }

    private static class Slots<TTarget, TField>
        where TTarget : class
    {
        internal static Slot<TTarget, TField>[] All = [];
    }

    private sealed class Slot<TTarget, TField>(Func<TTarget, TField> initialValue)
        where TTarget : class
    {
#if v1_6_OR_GREATER
        private readonly ConditionalWeakTable<TTarget, Box> values = [];
#else
        private readonly ConditionalWeakTable<TTarget, Box> values = new();
#endif
        private readonly ConditionalWeakTable<TTarget, Box>.CreateValueCallback create =
            target => new Box(initialValue(target));

        public ref TField Ref(TTarget target) => ref values.GetValue(target, create).Value;

        private sealed class Box(TField value)
        {
            public TField Value = value;
        }
    }
}
