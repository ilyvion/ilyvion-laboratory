// InjectedFieldShimTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

/// <summary>
/// Stands in for <c>Waylith.Injection.InjectedFieldAttribute</c>, so the shim can be run on these
/// accessors on every RimWorld version without the game's own field injection picking them up.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
internal sealed class TestInjectedFieldAttribute : Attribute
{
    public object? Default { get; set; }
    public string? Initializer { get; set; }
}

internal sealed class InjectionTarget(string name)
{
    public string Name { get; } = name;
}

internal enum InjectedMood
{
    Calm,
    Angry,
}

internal struct InjectedPair
{
    public int A;
    public object? B;
}

// An injected field accessor's body never reads its receiver.
#pragma warning disable CA1822 // Mark members as static
internal static class InjectionTargetFields
{
    internal static int ParameterlessInitializerCalls;

    extension(InjectionTarget target)
    {
        [TestInjectedField(Default = 7)]
        public int Count
        {
            get => throw new InvalidOperationException("not injected");
            set => throw new InvalidOperationException("not injected");
        }

        [TestInjectedField]
        public int Other
        {
            get => throw new InvalidOperationException("not injected");
            set => throw new InvalidOperationException("not injected");
        }

        [TestInjectedField]
        public ref InjectedPair Pair => throw new InvalidOperationException("not injected");

        [TestInjectedField(Default = 5L)]
        public ref long Counter => throw new InvalidOperationException("not injected");

        [TestInjectedField(Initializer = nameof(InitNames))]
        public List<string> Names
        {
            get => throw new InvalidOperationException("not injected");
            set => throw new InvalidOperationException("not injected");
        }

        [TestInjectedField(Initializer = nameof(InitTag))]
        public string Tag => throw new InvalidOperationException("not injected");

        [TestInjectedField(Default = InjectedMood.Angry)]
        public InjectedMood Mood
        {
            get => throw new InvalidOperationException("not injected");
            set => throw new InvalidOperationException("not injected");
        }

        [TestInjectedField(Default = "unnamed")]
        public string Label
        {
            get => throw new InvalidOperationException("not injected");
            set => throw new InvalidOperationException("not injected");
        }
    }

    extension(Thing thing)
    {
        [TestInjectedField(Default = 3)]
        public int InjectedThingValue
        {
            get => throw new InvalidOperationException("not injected");
            set => throw new InvalidOperationException("not injected");
        }
    }

    [TestInjectedField]
    public static ref int Hits(InjectionTarget target) =>
        throw new InvalidOperationException("not injected");

    public static List<string> InitNames(InjectionTarget target) => [target.Name];

    public static string InitTag()
    {
        ParameterlessInitializerCalls++;
        return "tag";
    }
}

internal static class InvalidDefaultFields
{
    extension(InjectionTarget target)
    {
        [TestInjectedField(Default = 1)]
        public List<string> WithDefault => throw new InvalidOperationException("not injected");
    }
}

internal static class MissingInitializerFields
{
    extension(InjectionTarget target)
    {
        [TestInjectedField(Initializer = "DoesNotExist")]
        public List<string> WithMissingInitializer =>
            throw new InvalidOperationException("not injected");
    }
}

internal static class StructTargetFields
{
    [TestInjectedField]
    public static ref int OnStruct(InjectedPair pair) =>
        throw new InvalidOperationException("not injected");
}

internal static class GenericTargetFields
{
    extension<T>(List<T> list)
    {
        [TestInjectedField]
        public int OnGeneric => throw new InvalidOperationException("not injected");
    }
}

internal static class ClosedGenericTargetFields
{
    [TestInjectedField]
    public static ref int OnClosedGeneric(List<int> list) =>
        throw new InvalidOperationException("not injected");
}

internal static class ExternFields
{
    extension(InjectionTarget target)
    {
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
        [TestInjectedField]
        public extern int ExternField { get; }
#pragma warning restore CS0626 // Method, operator, or accessor is marked external and has no attributes on it
    }
}

internal static class StaticExtensionFields
{
    extension(InjectionTarget)
    {
        [TestInjectedField]
        public static int StaticField => throw new InvalidOperationException("not injected");
    }
}

internal ref struct InjectedRefStruct;

internal static class RefStructFieldTypeFields
{
    [TestInjectedField]
    public static InjectedRefStruct RefStructField(InjectionTarget target) =>
        throw new InvalidOperationException("not injected");
}

internal static class WrongParameterCountFields
{
    [TestInjectedField]
    public static ref int TwoParameters(InjectionTarget target, int extra) =>
        throw new InvalidOperationException("not injected");
}

internal static class VoidReturnFields
{
    [TestInjectedField]
    public static void ReturnsNothing(InjectionTarget target) =>
        throw new InvalidOperationException("not injected");
}

#pragma warning restore CA1822 // Mark members as static

internal sealed class NonStaticFieldHost
{
    [TestInjectedField]
    public static ref int InNonStaticClass(InjectionTarget target) =>
        throw new InvalidOperationException("not injected");
}

[TestFixture(TestType.MainMenu)]
internal sealed class InjectedFieldShimTests
{
    private const string AttributeName = "ilyvion.Laboratory.Tests.TestInjectedFieldAttribute";

    private static InjectedFieldShim.Result? validResult;

    [OneTimeSetUp]
    public static void BindValidDeclarations() =>
        validResult ??= InjectedFieldShim.Process(
            WithNested(typeof(InjectionTargetFields)),
            AttributeName
        );

    // Extension properties are declared on compiler-generated types nested in the class.
    private static Type[] WithNested(Type type) =>
        [type, .. type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)];

    private static List<string> ErrorsFor(Type type) =>
        InjectedFieldShim.Process(WithNested(type), AttributeName).Errors;

    [Test]
    public static void ValidDeclarationsBindWithoutErrors()
    {
        Expect.IsEmpty(validResult!.Errors);
        // Count, Other, Pair, Counter, Names, Tag, Mood, Label, InjectedThingValue and Hits.
        Expect.AreEqual(10, validResult.Bound);
    }

    [Test]
    public static void ProcessingAgainBindsNothing()
    {
        var result = InjectedFieldShim.Process(
            WithNested(typeof(InjectionTargetFields)),
            AttributeName
        );

        Expect.AreEqual(0, result.Bound);
        Expect.IsEmpty(result.Errors);
    }

    [Test]
    public static void DefaultIsTheInitialValue()
    {
        var target = new InjectionTarget("a");

        Expect.AreEqual(7, target.Count);
        Expect.AreEqual(5L, target.Counter);
        Expect.AreEqual(InjectedMood.Angry, target.Mood);
        Expect.AreEqual("unnamed", target.Label);
        Expect.AreEqual(0, target.Other);
    }

    [Test]
    public static void SetterStoresTheValuePerInstance()
    {
        var first = new InjectionTarget("first");
        var second = new InjectionTarget("second");

        first.Count = 3;
        first.Label = "changed";

        Expect.AreEqual(3, first.Count);
        Expect.AreEqual(7, second.Count);
        Expect.AreEqual("changed", first.Label);
        Expect.AreEqual("unnamed", second.Label);
    }

    [Test]
    public static void FieldsOfTheSameTypeAreIndependent()
    {
        var target = new InjectionTarget("a") { Other = 11 };

        Expect.AreEqual(7, target.Count);
        Expect.AreEqual(11, target.Other);
    }

    [Test]
    public static void RefAccessorChangesTheStoredValueInPlace()
    {
        var target = new InjectionTarget("a");

        target.Pair.A = 4;
        target.Pair.B = "b";
        target.Counter++;
        ref var counter = ref target.Counter;
        counter += 10;

        Expect.AreEqual(4, target.Pair.A);
        Expect.AreEqual("b", target.Pair.B);
        Expect.AreEqual(16L, target.Counter);
    }

    [Test]
    public static void InitializerTakingTheTargetRunsOncePerInstance()
    {
        var target = new InjectionTarget("named");

        var names = target.Names;
        names.Add("added");

        Expect.ReferencesAreEqual(names, target.Names);
        Expect.That(() => target.Names.SequenceEqual(["named", "added"]));
        Expect.That(() => new InjectionTarget("other").Names.SequenceEqual(["other"]));
    }

    [Test]
    public static void SetterReplacesAnInitializedValue()
    {
        var target = new InjectionTarget("a");
        List<string> replacement = ["replacement"];

        target.Names = replacement;

        Expect.ReferencesAreEqual(replacement, target.Names);
    }

    [Test]
    public static void ParameterlessInitializerRunsOncePerInstance()
    {
        var target = new InjectionTarget("a");
        var callsBefore = InjectionTargetFields.ParameterlessInitializerCalls;

        Expect.AreEqual("tag", target.Tag);
        Expect.AreEqual("tag", target.Tag);

        Expect.AreEqual(callsBefore + 1, InjectionTargetFields.ParameterlessInitializerCalls);
    }

    [Test]
    public static void StaticAccessorMethodGetsAField()
    {
        var target = new InjectionTarget("a");

        InjectionTargetFields.Hits(target) += 2;
        InjectionTargetFields.Hits(target)++;

        Expect.AreEqual(3, InjectionTargetFields.Hits(target));
        Expect.AreEqual(0, InjectionTargetFields.Hits(new InjectionTarget("b")));
    }

    [Test]
    public static void GameTypesCanBeTargets()
    {
        var thing = new Thing();

        Expect.AreEqual(3, thing.InjectedThingValue);
        thing.InjectedThingValue = 9;
        Expect.AreEqual(9, thing.InjectedThingValue);
    }

    [Test]
    public static void DefaultOnANonPrimitiveFieldIsRejected()
    {
        var errors = ErrorsFor(typeof(InvalidDefaultFields));

        Expect.AreEqual(1, errors.Count);
        Expect.That(() =>
            errors.Any(e => e.Contains("Use an initializer instead", StringComparison.Ordinal))
        );
    }

    [Test]
    public static void MissingInitializerIsRejected()
    {
        var errors = ErrorsFor(typeof(MissingInitializerFields));

        Expect.AreEqual(1, errors.Count);
        Expect.That(() =>
            errors.Any(e => e.Contains("initializer DoesNotExist", StringComparison.Ordinal))
        );
    }

    [Test]
    public static void StructTargetIsRejected()
    {
        var errors = ErrorsFor(typeof(StructTargetFields));

        Expect.AreEqual(1, errors.Count);
        Expect.That(() => errors.Any(e => e.Contains("must be a class", StringComparison.Ordinal)));
    }

    [Test]
    public static void GenericTargetIsRejected()
    {
        var errors = ErrorsFor(typeof(GenericTargetFields));

        Expect.AreEqual(1, errors.Count);
        Expect.That(() =>
            errors.Any(e => e.Contains("extends a generic type", StringComparison.Ordinal))
        );
    }

    [Test]
    public static void AccessorOutsideAStaticClassIsRejected()
    {
        var errors = ErrorsFor(typeof(NonStaticFieldHost));

        Expect.AreEqual(1, errors.Count);
        Expect.That(() =>
            errors.Any(e =>
                e.Contains("must be declared in a static class", StringComparison.Ordinal)
            )
        );
    }

    [Test]
    public static void ClosedGenericTargetIsRejected() =>
        ExpectSingleError(typeof(ClosedGenericTargetFields), "is generic");

    [Test]
    public static void ExternAccessorIsRejected() =>
        ExpectSingleError(typeof(ExternFields), "has no method body (extern)");

    [Test]
    public static void StaticExtensionPropertyIsRejected() =>
        ExpectSingleError(typeof(StaticExtensionFields), "is a static extension property");

    [Test]
    public static void RefStructFieldTypeIsRejected() =>
        ExpectSingleError(typeof(RefStructFieldTypeFields), "cannot be stored in a field");

    [Test]
    public static void AccessorWithTwoParametersIsRejected() =>
        ExpectSingleError(
            typeof(WrongParameterCountFields),
            "must take exactly the target instance"
        );

    [Test]
    public static void AccessorReturningNothingIsRejected() =>
        ExpectSingleError(typeof(VoidReturnFields), "must take exactly the target instance");

    private static void ExpectSingleError(Type type, string text)
    {
        var errors = ErrorsFor(type);

        Expect.AreEqual(1, errors.Count, string.Join("\n", errors));
        Expect.That(
            () => errors.Any(e => e.Contains(text, StringComparison.Ordinal)),
            string.Join("\n", errors)
        );
    }
}
