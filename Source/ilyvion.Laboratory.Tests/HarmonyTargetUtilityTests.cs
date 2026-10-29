// HarmonyTargetUtilityTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using DevTools.Testing;

namespace ilyvion.Laboratory.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class HarmonyTargetUtilityTests
{
    private static class Outer
    {
        public static int OuterTarget() => 0;

        public static Func<int> WithLocalFunction(int captured)
        {
            return CapturingLocalTarget;

            int CapturingLocalTarget()
            {
                return captured++;
            }
        }

        private static class Middle
        {
            public static void MiddleTarget() { }

            private static class Inner
            {
                public static void InnerTarget() { }

                public static void InnerOther() { }
            }
        }
    }

    private static List<string> Names(IEnumerable<MethodInfo> methods) =>
        [.. methods.Select(m => m.Name).OrderBy(n => n, StringComparer.Ordinal)];

    [Test]
    public static void FindsMethodsOnTypeAndNestedTypesAtAnyDepth()
    {
        var result = HarmonyTargetUtility.FindMethods(typeof(Outer), "Target");

        Expect.AreEqual(4, result.Count);
        var names = Names(result);
        Expect.IsTrue(names.Contains("OuterTarget"));
        Expect.IsTrue(names.Contains("MiddleTarget"));
        Expect.IsTrue(names.Contains("InnerTarget"));
        Expect.IsTrue(names.Any(n => n.Contains("CapturingLocalTarget", StringComparison.Ordinal)));
    }

    [Test]
    public static void FindsCompilerGeneratedLocalFunction()
    {
        var result = HarmonyTargetUtility.FindMethods(typeof(Outer), "CapturingLocalTarget");

        Expect.AreEqual(1, result.Count);
        Expect.IsTrue(result[0].DeclaringType != typeof(Outer));
    }

    [Test]
    public static void RequiresEveryNameFragment()
    {
        var result = HarmonyTargetUtility.FindMethods(typeof(Outer), "Inner", "Target");

        Expect.AreEqual(1, result.Count);
        Expect.AreEqual("InnerTarget", result[0].Name);
    }

    [Test]
    public static void NameFragmentsAreCaseSensitive()
    {
        var result = HarmonyTargetUtility.FindMethods(typeof(Outer), "innertarget");

        Expect.AreEqual(0, result.Count);
    }

    [Test]
    public static void FindsMethodsMatchingPredicate()
    {
        var result = HarmonyTargetUtility.FindMethods(
            typeof(Outer),
            m => m.Name.StartsWith("Inner", StringComparison.Ordinal)
        );

        Expect.AreEqual(2, result.Count);
        var names = Names(result);
        Expect.AreEqual("InnerOther", names[0]);
        Expect.AreEqual("InnerTarget", names[1]);
    }

    [Test]
    public static void ExpectSingleAcceptsOneMethodWithoutLogging()
    {
        List<string> errors = [];

        var result = HarmonyTargetUtility.ExpectSingle(
            HarmonyTargetUtility.FindMethods(typeof(Outer), "InnerTarget"),
            "patch Outer.InnerTarget",
            errors.Add
        );

        Expect.IsTrue(result);
        Expect.AreEqual(0, errors.Count);
    }

    [Test]
    public static void ExpectSingleRejectsNoMethodsAndLogs()
    {
        List<string> errors = [];

        var result = HarmonyTargetUtility.ExpectSingle(
            HarmonyTargetUtility.FindMethods(typeof(Outer), "Missing"),
            "patch Outer.Missing",
            errors.Add
        );

        Expect.IsFalse(result);
        Expect.AreEqual(1, errors.Count);
        Expect.AreEqual(
            "Could not patch Outer.Missing, expected exactly one target method but found 0",
            errors[0]
        );
    }

    [Test]
    public static void ExpectSingleRejectsSeveralMethodsAndLogsThem()
    {
        List<string> errors = [];

        var result = HarmonyTargetUtility.ExpectSingle(
            HarmonyTargetUtility.FindMethods(typeof(Outer), "Inner"),
            "patch Outer.Inner",
            errors.Add
        );

        Expect.IsFalse(result);
        Expect.AreEqual(1, errors.Count);
        Expect.IsTrue(
            errors[0]
                .StartsWith(
                    "Could not patch Outer.Inner, expected exactly one target method but found 2: ",
                    StringComparison.Ordinal
                )
        );
        Expect.IsTrue(errors[0].Contains(".InnerTarget", StringComparison.Ordinal));
        Expect.IsTrue(errors[0].Contains(".InnerOther", StringComparison.Ordinal));
    }
}
