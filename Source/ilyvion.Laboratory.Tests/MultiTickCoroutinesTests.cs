// MultiTickCoroutinesTests.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using RimTestRedux;

namespace ilyvion.Laboratory.Tests;

[TestSuite]
internal static class MultiTickCoroutinesTests
{
    private static Coroutine ImmediatelyDone()
    {
        yield return ResumeImmediately.Singleton;
    }

    // Regression guard for BUG-16: a coroutine started via StartCoroutine() while another
    // coroutine is being force-completed by RunCoroutineImmediatelyToCompletion() used to always
    // be scheduled onto the manager's normal per-tick list, which never advances while the
    // immediate-completion loop is spinning synchronously. Waiting on such a child therefore
    // hung the calling thread forever (hard game freeze). The child must be routed into the same
    // immediate-completion batch as its parent so it gets force-completed too.
    [Test]
    public static void ChildCoroutineStartedDuringImmediateRunCompletesSynchronously()
    {
        var childCompleted = false;

        Coroutine Parent()
        {
            var child = MultiTickCoroutineManager.StartCoroutine(
                ImmediatelyDone(),
                coroutineFinishedCallback: () => childCompleted = true
            );
            yield return child.ResumeWhenOtherCoroutineIsCompleted();
        }

        MultiTickCoroutineManager.RunCoroutineImmediatelyToCompletion(Parent());

        Assert.That(childCompleted).Is.True();
    }

    // Regression guard for BUG-16: RunCoroutineImmediatelyToCompletion() used to track its batch
    // in a single static list, so a coroutine that itself called
    // RunCoroutineImmediatelyToCompletion() (reentrancy) shared that list with the outer call,
    // corrupting both batches. Each call must use its own independent batch.
    [Test]
    public static void ReentrantImmediateCompletionDoesNotCorruptOuterBatch()
    {
        var innerCompleted = false;
        var outerCompleted = false;

        Coroutine Outer()
        {
            MultiTickCoroutineManager.RunCoroutineImmediatelyToCompletion(
                InnerCoroutine(() => innerCompleted = true)
            );
            outerCompleted = true;
            yield return ResumeImmediately.Singleton;
        }

        static Coroutine InnerCoroutine(Action onDone)
        {
            yield return ResumeImmediately.Singleton;
            onDone();
        }

        MultiTickCoroutineManager.RunCoroutineImmediatelyToCompletion(Outer());

        Assert.That(innerCompleted).Is.True();
        Assert.That(outerCompleted).Is.True();
    }

    // Regression guard for BUG-16: a coroutine waiting on a condition that can never resolve
    // synchronously (e.g. one depending on real ticks passing, or on another coroutine that was
    // never scheduled into this batch) used to spin RunCoroutineImmediatelyToCompletion() forever.
    // It must give up after a bounded number of iterations instead of hanging the caller.
    [Test]
    public static void NeverResumingCoroutineDoesNotHangImmediateCompletion()
    {
        static Coroutine NeverResumes()
        {
            yield return new ResumeWhenTrue(() => false);
        }

        Assert
            .ThatFunc(() =>
                MultiTickCoroutineManager.RunCoroutineImmediatelyToCompletion(NeverResumes())
            )
            .Not.Throw();
    }
}
