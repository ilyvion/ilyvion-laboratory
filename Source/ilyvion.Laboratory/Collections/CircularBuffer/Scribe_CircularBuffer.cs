using System.Xml;
using RimWorld.Planet;

namespace ilyvion.Laboratory.Collections;

#pragma warning disable CA1707
public static class Scribe_CircularBuffer
#pragma warning restore CA1707
{
    private class CircularBufferSerialized<T> : IExposable
    {
        public int capacity;
        public List<T> values = [];

        public void ExposeData()
        {
            Scribe_Values.Look(ref capacity, "capacity");
            Scribe_Collections.Look(ref values, "values");
        }
    }

    public static void Look<T>(ref CircularBuffer<T>? circularBuffer, string label, LookMode lookMode = LookMode.Undefined)
    {
        Look(ref circularBuffer, saveDestroyedThings: false, label, lookMode);
    }

    public static void Look<T>(ref CircularBuffer<T>? circularBuffer, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined)
    {
        CircularBufferSerialized<T>? serialized = null;
        if (Scribe.mode == LoadSaveMode.Saving && circularBuffer != null)
        {
            serialized = new CircularBufferSerialized<T>()
            {
                capacity = circularBuffer.Capacity,
                values = [.. circularBuffer]
            };
        }
        Scribe_Deep.Look(ref serialized, saveDestroyedThings, label);
        if ((lookMode != LookMode.Reference || Scribe.mode != LoadSaveMode.ResolvingCrossRefs) && (lookMode == LookMode.Reference || Scribe.mode != LoadSaveMode.LoadingVars))
        {
            return;
        }
        if (serialized == null)
        {
            circularBuffer = null;
            return;
        }
        circularBuffer = new CircularBuffer<T>(serialized.capacity);
        for (int i = 0; i < serialized.values.Count; i++)
        {
            circularBuffer.PushBack(serialized.values[i]);
        }
    }
}
