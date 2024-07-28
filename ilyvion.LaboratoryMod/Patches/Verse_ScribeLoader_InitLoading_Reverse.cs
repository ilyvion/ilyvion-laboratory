using System.Reflection;
using System.Reflection.Emit;
using Logger = ilyvion.Laboratory.Logger;

namespace ilyvion.LaboratoryMod;

[HarmonyPatch]
internal static class Verse_ScribeLoader_InitLoading_Reverse
{
    private static void LogException(Exception ex)
    {
        Logger.LogError($"Exception while init loading using custom StreamReader:\n{ex}");
    }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    private static readonly MethodInfo _method_LogException = SymbolExtensions.GetMethodInfo(() => LogException(default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ScribeLoader), nameof(ScribeLoader.InitLoading))]
    internal static void InitLoadingWithCustomStreamReader(ScribeLoader scribeLoader, StreamReader streamReader)
    {
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            // Locate where the original StreamReader is instantiated
            codeMatcher.SearchForward(i => i.opcode == OpCodes.Newobj && codeMatcher.Operand is ConstructorInfo c && c.DeclaringType == typeof(StreamReader));
            if (!codeMatcher.IsValid)
            {
                Log.Error("Could not reverse patch ScribeLoader.InitLoading, IL does not match expectations: instantiation of StreamReader not found [newobj].");
                return codeMatcher.Instructions();
            }
            codeMatcher.Advance(-1);
            if (!codeMatcher.IsValid || codeMatcher.Instruction.opcode != OpCodes.Ldarg_1)
            {
                Log.Error("Could not reverse patch ScribeLoader.InitLoading, IL does not match expectations: instantiation of StreamReader not found [ldarg.1].");
                return codeMatcher.Instructions();
            }
            var streamReaderInstantiationStartPosition = codeMatcher.Pos;
            var startBlocks = codeMatcher.Instruction.ExtractBlocks();

            codeMatcher.Advance(2);
            if (!codeMatcher.IsValid || codeMatcher.Instruction.opcode != OpCodes.Stloc_0)
            {
                Log.Error("Could not reverse patch ScribeLoader.InitLoading, IL does not match expectations: instantiation of StreamReader not found [stloc.0].");
                return codeMatcher.Instructions();
            }
            var streamReaderInstantiationEndPosition = codeMatcher.Pos;

            // With the prep-work out of the way, let's go!
            // Remove the creation of the StreamReader using the file path
            codeMatcher.RemoveInstructionsInRange(streamReaderInstantiationStartPosition, streamReaderInstantiationEndPosition);

            // Store our custom stream reader in the expected local.
            codeMatcher.Start();
            codeMatcher.Advance(streamReaderInstantiationStartPosition);
            codeMatcher.Insert([
                new(OpCodes.Ldarg_1) { blocks = startBlocks },
                new(OpCodes.Stloc_0),
            ]);

            // Finally, we need to fix the exception handler
            codeMatcher.SearchForward(i => i.opcode == OpCodes.Stloc_3);
            if (!codeMatcher.IsValid)
            {
                Log.Error("Could not reverse patch ScribeLoader.InitLoading, IL does not match expectations: storing of Exception not found [stloc.3].");
                return codeMatcher.Instructions();
            }

            codeMatcher.Advance(1);
            var logErrorStartPosition = codeMatcher.Pos;

            codeMatcher.SearchForward(i => i.opcode == OpCodes.Ldarg_0);
            if (!codeMatcher.IsValid)
            {
                Log.Error("Could not reverse patch ScribeLoader.InitLoading, IL does not match expectations: accessing this not found after storing Exception [ldarg.0].");
                return codeMatcher.Instructions();
            }
            codeMatcher.Advance(-1);
            var logErrorEndPosition = codeMatcher.Pos;

            // Remove original call to Log.Error
            codeMatcher.RemoveInstructionsInRange(logErrorStartPosition, logErrorEndPosition);

            // Insert our own Log.Error message
            codeMatcher.Start();
            codeMatcher.Advance(logErrorStartPosition);
            codeMatcher.Insert([
                new(OpCodes.Ldloc_3),
                new(OpCodes.Call, _method_LogException),
            ]);

            return codeMatcher.Instructions();
        }

        // Make compiler happy. This gets patched out anyway.
        _ = scribeLoader;
        _ = streamReader;
        Transpiler(null!, null!);
    }
}
