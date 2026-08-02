namespace ilyvion.Laboratory.Collections;

#pragma warning disable CA1707
public static class Scribe_CircularBuffer
#pragma warning restore CA1707
{
    public static void Look<T>(
        ref CircularBuffer<T>? circularBuffer,
        string label,
        LookMode lookMode = LookMode.Undefined
    ) => Look(ref circularBuffer, saveDestroyedThings: false, label, lookMode);

    // "capacity" and "values" are scribed directly under our own EnterNode/ExitNode pair, rather
    // than through Scribe_Deep, because Scribe_Deep.Look only ever (re-)runs its wrapped
    // ExposeData during the Saving and LoadingVars passes. LookMode.Reference elements only
    // become available during the later ResolvingCrossRefs pass, so "values" must be scribed
    // directly here (via Scribe_Collections.Look, forwarding the outer lookMode) so that it gets
    // called again - and re-enters the same XML node - during that pass too.
    public static void Look<T>(
        ref CircularBuffer<T>? circularBuffer,
        bool saveDestroyedThings,
        string label,
        LookMode lookMode = LookMode.Undefined
    )
    {
        if (Scribe.EnterNode(label))
        {
            try
            {
                if (Scribe.mode == LoadSaveMode.Saving && circularBuffer == null)
                {
                    Scribe.saver.WriteAttribute("IsNull", "True");
                }
                else
                {
                    var capacity = 0;
                    List<T>? values = null;
                    if (Scribe.mode == LoadSaveMode.Saving && circularBuffer is { } bufferToSave)
                    {
                        capacity = bufferToSave.Capacity;
                        values = [.. bufferToSave];
                    }
                    else if (Scribe.mode == LoadSaveMode.LoadingVars)
                    {
                        var xmlAttribute = Scribe.loader.curXmlParent.Attributes["IsNull"];
                        if (
                            xmlAttribute != null
                            && xmlAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            circularBuffer = null;
                            return;
                        }
                    }

                    Scribe_Values.Look(ref capacity, "capacity");
#if v1_3 || v1_4
                    Scribe_Collections.Look(ref values, saveDestroyedThings, "values", lookMode);
#else
                    Scribe_Collections.Look(ref values, "values", saveDestroyedThings, lookMode);
#endif

                    if (Scribe.mode == LoadSaveMode.LoadingVars)
                    {
                        var effectiveCapacity = Math.Max(Math.Max(capacity, values?.Count ?? 0), 1);
                        circularBuffer = new CircularBuffer<T>(effectiveCapacity);
                        if (lookMode != LookMode.Reference && values != null)
                        {
                            foreach (var value in values)
                            {
                                circularBuffer.PushBack(value);
                            }
                        }
                    }
                    else if (
                        Scribe.mode == LoadSaveMode.ResolvingCrossRefs
                        && lookMode == LookMode.Reference
                        && circularBuffer != null
                        && values != null
                    )
                    {
                        foreach (var value in values)
                        {
                            circularBuffer.PushBack(value);
                        }
                    }
                }
            }
            finally
            {
                Scribe.ExitNode();
            }
            return;
        }
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            circularBuffer = null;
        }
    }
}
