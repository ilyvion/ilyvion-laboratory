namespace ilyvion.Laboratory;

#pragma warning disable CA1815
public struct DoOnDispose(Action action) : IDisposable
#pragma warning restore CA1815
{
    public readonly void Dispose()
    {
        action();
    }
}
