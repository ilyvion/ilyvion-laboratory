using System.Diagnostics.CodeAnalysis;

namespace ilyvion.Laboratory.Coroutines;

// This stuff is loosely based on Unity's Coroutines, but adapted to work on a
// tick-by-tick basis instead of a frame-by-frame basis.

/// <summary>
/// GameComponent responsible for orchestrating the execution of multi-tick coroutines. The main
/// purpose of these coroutines is to spread a heavy task across multiple ticks to reduce the
/// performance cost of the task.
/// </summary>
/// <remarks>
/// It's important to keep in mind that coroutine state is not saved across games, so if the
/// task cannot be interrupted safely, you must either have your own mechanism for safe resumption
/// upon a game load, or find another mechanism for executing your task.
/// </remarks>
public class MultiTickCoroutineManager : GameComponent
{
    private readonly List<CoroutineHandle> _coroutines = [];

    public MultiTickCoroutineManager(Game _)
    {
    }

    public override void GameComponentTick()
    {
        RunSingleTick(_coroutines);
    }

    private static CoroutineHandle? currentlyRunningCoroutine;
    private static void RunSingleTick(List<CoroutineHandle> coroutines)
    {
        // Bail early when there's nothing to do
        int coroutineCount = coroutines.Count;
        if (coroutineCount == 0)
        {
            return;
        }

        // Attempt avoiding unnecessary GC allocations by using stackalloc if the
        // coroutine count is reasonable.
        Span<bool> finishedCoroutines = coroutineCount < 256
            ? stackalloc bool[coroutineCount]
            : new bool[coroutineCount];

        Logger.LogDebug($"Running {coroutineCount} coroutines", "Coroutines");

        bool anyFinished = false;
        RunCoroutines(coroutines, 0, coroutineCount, finishedCoroutines, ref anyFinished);

        List<bool>? additionallyFinishedCoroutines = coroutines.Count > coroutineCount
            ? []
            : null;
        int additionalCoroutinesStartIndex = 0;
        while (coroutines.Count > coroutineCount)
        {
            // New coroutines were added by the coroutines that were ran. These should also be run
            // in this same tick.
            int newCoroutineCount = coroutines.Count - coroutineCount;
            Logger.LogDebug($"Running additional {newCoroutineCount} coroutines", "Coroutines");

            additionallyFinishedCoroutines!.AddRange(
                Enumerable.Repeat(false, newCoroutineCount));
            Span<bool> additionallyFinishedCoroutinesSpan = additionallyFinishedCoroutines._items;
            additionallyFinishedCoroutinesSpan =
                additionallyFinishedCoroutinesSpan.Slice(
                    additionalCoroutinesStartIndex, newCoroutineCount);

            RunCoroutines(
                coroutines,
                coroutineCount,
                coroutines.Count,
                additionallyFinishedCoroutinesSpan,
                ref anyFinished);

            additionalCoroutinesStartIndex += newCoroutineCount;
            coroutineCount += newCoroutineCount;
        }

        if (anyFinished)
        {
            additionalCoroutinesStartIndex = finishedCoroutines.Length;
            for (int i = finishedCoroutines.Length - 1; i >= 0; i--)
            {
                if (finishedCoroutines[i])
                {
                    Logger.LogDebug(
                        $"Removing coroutine {coroutines[i].DebugHandle} " +
                        "as it reported being finished", "Coroutines");
                    coroutines.RemoveAt(i);
                    additionalCoroutinesStartIndex--;
                }
            }

            if (additionallyFinishedCoroutines != null)
            {
                for (int i = additionallyFinishedCoroutines.Count - 1; i >= 0; i--)
                {
                    if (additionallyFinishedCoroutines[i])
                    {
                        int coroutineIndex = additionalCoroutinesStartIndex + i;

                        Logger.LogDebug(
                            $"Removing coroutine {coroutines[coroutineIndex].DebugHandle} " +
                            "as it reported being finished", "Coroutines");
                        coroutines.RemoveAt(coroutineIndex);
                    }
                }
            }
        }

        static void RunCoroutines(
            List<CoroutineHandle> coroutines,
            int start,
            int end,
            Span<bool> finishedCoroutines,
            ref bool anyFinished)
        {
            for (int i = start; i < end; i++)
            {
                var coroutine = coroutines[i];

                Logger.LogDebug($"Checking if we should run coroutine {coroutine.DebugHandle}...",
                    "Coroutines");
                if (coroutine.ShouldRun())
                {
                    Logger.LogDebug($"...YES! Resuming!", "Coroutines");
                    currentlyRunningCoroutine = coroutine;
                    coroutine.Resume();
                    currentlyRunningCoroutine = null;
                }
                else
                {
                    Logger.LogDebug($"...NO! Skipping!", "Coroutines");
                }
                anyFinished |= finishedCoroutines[i - start] = coroutine.IsCompleted;
            }
        }
    }

    public static CoroutineHandle StartCoroutine(
        IEnumerable<IResumeCondition> coroutine,
        Action? coroutineFinishedCallback = null,
        Action<Exception>? coroutineFailedCallback = null,
        string? debugHandle = null)
    {
        if (coroutine == null)
        {
            throw new ArgumentNullException(nameof(coroutine));
        }

        debugHandle ??= $"Unnamed({coroutine.GetHashCode()})";

        Logger.LogDebug($"Creating CoroutineHandle for {debugHandle}", "Coroutines");
        CoroutineHandle handle = new(
            coroutine.GetEnumerator(),
            currentlyRunningCoroutine,
            debugHandle,
            coroutineFinishedCallback,
            coroutineFailedCallback);
        Logger.LogDebug($"Adding CoroutineHandle for {debugHandle} to MultiTickCoroutineManager",
            "Coroutines");
        Current.Game.GetComponent<MultiTickCoroutineManager>()._coroutines.Add(handle);
        return handle;
    }

    private static readonly List<CoroutineHandle> immediateCompletionList = [];
    public static void RunCoroutineImmediatelyToCompletion(
        IEnumerable<IResumeCondition> coroutine)
    {
        if (coroutine == null)
        {
            throw new ArgumentNullException(nameof(coroutine));
        }

        immediateCompletionList.Add(new(
            coroutine.GetEnumerator(),
            null,
            $"Immediate({coroutine.GetHashCode()})",
            null,
            null));

        while (immediateCompletionList.Count > 0)
        {
            RunSingleTick(immediateCompletionList);
        }
    }
}
public static class MultiTickCoroutineManagerExtensions
{
    public static void RunImmediatelyToCompletion(this IEnumerable<IResumeCondition> coroutine)
    {
        MultiTickCoroutineManager.RunCoroutineImmediatelyToCompletion(coroutine);
    }
}

public class CoroutineHandle
{
    private IEnumerator<IResumeCondition>? _coroutine;
    private readonly Action? _coroutineFinishedCallback;
    private readonly Action<Exception>? _coroutineFailedCallback;
    private bool _isStarted;

    public bool IsStarted => _isStarted;
    [MemberNotNullWhen(false, nameof(_coroutine))]
    public bool IsCompleted
    {
        get
        {
            return _coroutine == null;
        }
    }

    private CoroutineHandle? _parent;
    internal CoroutineHandle? Parent => _parent;

    private readonly List<CoroutineHandle> _children = [];
    internal List<CoroutineHandle> Children => _children;

    private readonly string _debugHandle;
    internal string DebugHandle => _debugHandle;

    private Exception? _exception;

    public bool HasException => _exception != null;
    public Exception? Exception => _exception;

    internal CoroutineHandle(
        IEnumerator<IResumeCondition> coroutine,
        CoroutineHandle? parent,
        string debugHandle,
        Action? coroutineFinishedCallback,
        Action<Exception>? coroutineFailedCallback)
    {
        _coroutine = coroutine;
        _parent = parent;
        parent?._children.Add(this);
        _debugHandle = debugHandle;
        _coroutineFinishedCallback = coroutineFinishedCallback;
        _coroutineFailedCallback = coroutineFailedCallback;
    }

    /// <summary>
    /// Whether to run the coroutine.
    /// </summary>
    /// <returns>true if it should run</returns>
    internal bool ShouldRun()
    {
        if (IsCompleted)
        {
            return false;
        }
        else if (IsStarted)
        {
            return _coroutine.Current.ShouldResume();
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Resumes the coroutine.
    /// </summary>
    /// <returns>true when coroutine finishes</returns>
    internal void Resume()
    {
        if (IsCompleted)
        {
            Logger.LogWarning($"Called Resume on coroutine {_debugHandle} " +
                "after it already finished");
            return;
        }

#pragma warning disable CA1031
        bool finished;
        try
        {
            Logger.LogDebug($"Calling MoveNext on {_debugHandle}", "Coroutines");
            finished = !_coroutine.MoveNext();
            _isStarted = true;
        }
        catch (Exception e)
        {
            Logger.LogWarning($"MoveNext caused exception on coroutine {_debugHandle}:\n{e}");
            finished = true;
            _exception = e;
            _coroutineFailedCallback?.Invoke(e);
        }

        if (finished)
        {
            Logger.LogDebug($"Cleaning up coroutine {_debugHandle} since it finished",
                "Coroutines");
            _coroutineFinishedCallback?.Invoke();
            Cleanup();
        }
#pragma warning restore CA1031
    }

    public void Cancel()
    {
        Logger.LogDebug($"Cancelling coroutine {_debugHandle}", "Coroutines");
        foreach (var child in _children)
        {
            child.Cancel();
        }
        Cleanup();
    }

    private void Cleanup()
    {
        _parent = null;
        _children.Clear();
        _coroutine?.Dispose();
        _coroutine = null;
    }
}

/// <summary>
/// The method on this interface is checked by the coroutine manager each tick to decide whether or
/// not a coroutine should be resumed.
/// </summary>
public interface IResumeCondition
{
    /// <summary>
    /// Tells the coroutine manager whether or not to resume the coroutine that returned this value
    /// this tick.
    /// </summary>
    /// <returns>true if the coroutine should be resumed</returns>
    public bool ShouldResume();
}

/// <summary>
/// Resumes the coroutine immediately (which in terms of multi-tick coroutines means the next tick).
/// Because this type is so simple, it cannot be directly constructed and you are expected to return
/// its Singleton property value to use it.
/// </summary>
public class ResumeImmediately : IResumeCondition
{
    public static ResumeImmediately Singleton { get; } = new();
    public bool ShouldResume() => true;

    private ResumeImmediately() { }
}

/// <summary>
/// Resumes after a set number of ticks have elapsed.
/// </summary>
public class ResumeAfterTicks(int ticksToWait) : IResumeCondition
{
    private int _ticksLeft = ticksToWait;
    public bool ShouldResume()
    {
        return _ticksLeft-- == 0;
    }
}

/// <summary>
/// Resumes the returning coroutine after the coroutine given by the provided handle has finished
/// running.
/// </summary>
public class ResumeWhenOtherCoroutineIsCompleted(CoroutineHandle handle)
    : IResumeCondition
{
    private readonly CoroutineHandle _handle = handle;

    public bool ShouldResume()
    {
        return _handle.IsCompleted;
    }
}
public static class ResumeWhenOtherCoroutineIsCompletedExtensions
{
    public static ResumeWhenOtherCoroutineIsCompleted ResumeWhenOtherCoroutineIsCompleted(
        this IEnumerable<IResumeCondition> coroutine,
        Action? coroutineFinishedCallback = null,
        Action<Exception>? coroutineFailedCallback = null,
        string? debugHandle = null)
    {
        return MultiTickCoroutineManager.StartCoroutine(
            coroutine,
            coroutineFinishedCallback,
            coroutineFailedCallback,
            debugHandle)
            .ResumeWhenOtherCoroutineIsCompleted();
    }

    public static ResumeWhenOtherCoroutineIsCompleted ResumeWhenOtherCoroutineIsCompleted(
        this CoroutineHandle coroutineHandle)
    {
        return new ResumeWhenOtherCoroutineIsCompleted(coroutineHandle);
    }
}

public class ResumeWhenTrue(Func<bool> predicate) : IResumeCondition
{
    private readonly Func<bool> _predicate = predicate;

    public bool ShouldResume()
    {
        return _predicate();
    }
}
