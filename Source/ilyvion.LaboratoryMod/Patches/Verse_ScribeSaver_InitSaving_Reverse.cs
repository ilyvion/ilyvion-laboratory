using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using Logger = ilyvion.Laboratory.Logger;

namespace ilyvion.LaboratoryMod;

[HarmonyPatch]
internal static class Verse_ScribeSaver_InitSaving_Reverse
{
    private static void LogException(Exception ex)
    {
        Logger.LogError($"Exception while init saving using custom Stream:\n{ex}");
    }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    private static readonly MethodInfo _method_LogException = SymbolExtensions.GetMethodInfo(() => LogException(default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    private static readonly FieldInfo _field_Scribe_Mode = AccessTools.Field(typeof(Scribe), "mode");
    private static readonly FieldInfo _field_ScribeSaver_SaveStream = AccessTools.Field(typeof(ScribeSaver), "saveStream");
    private static readonly MethodInfo _method_XmlwriterSettings_setIndent = AccessTools.PropertySetter(typeof(XmlWriterSettings), nameof(XmlWriterSettings.Indent));

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ScribeSaver), nameof(ScribeSaver.InitSaving))]
    internal static void InitSavingWithCustomStream(ScribeSaver scribeLoader, Stream stream, string documentElementName, bool useIndentation)
    {
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            // Locate where Scribe.mode is assigned.
            codeMatcher.SearchForward(i => i.opcode == OpCodes.Stsfld && i.operand is FieldInfo f && f == _field_Scribe_Mode);
            if (!codeMatcher.IsValid)
            {
                Log.Error("Could not reverse patch ScribeSaver.InitSaving, IL does not match expectations: assignment of Scribe.mode not found.");
                return codeMatcher.Instructions();
            }
            codeMatcher.Advance(1);
            if (!codeMatcher.IsValid || codeMatcher.Instruction.opcode != OpCodes.Ldarg_1)
            {
                Log.Error("Could not reverse patch ScribeSaver.InitSaving, IL does not match expectations: instantiation of FileStream not found [ldarg.1].");
                return codeMatcher.Instructions();
            }
            var fileStreamCreateStartPos = codeMatcher.Pos;

            // stfld indicates the end of the FileStream creation
            codeMatcher.SearchForward(i => i.opcode == OpCodes.Stfld && i.operand is FieldInfo f && f == _field_ScribeSaver_SaveStream);
            if (!codeMatcher.IsValid)
            {
                Log.Error("Could not reverse patch ScribeSaver.InitSaving, IL does not match expectations: instantiation of FileStream not found [stfld].");
                return codeMatcher.Instructions();
            }
            codeMatcher.Advance(-1);
            var fileStreamCreateEndPos = codeMatcher.Pos;

            // With the prep-work out of the way, let's go!
            // Remove the creation of the StreamReader using the file path
            codeMatcher.RemoveInstructionsInRange(fileStreamCreateStartPos, fileStreamCreateEndPos);

            // Store our custom stream reader in the expected field instead.
            codeMatcher.Start();
            codeMatcher.Advance(fileStreamCreateStartPos);
            codeMatcher.Insert([
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldarg_1),
                // We don't need the store instruction because we didn't remove the
                // original one, so we'll just make use of that one.
            ]);

            // Next, add support for not using indentation in the output
            codeMatcher.SearchForward(i => i.opcode == OpCodes.Ldloc_1);
            if (!codeMatcher.IsValid)
            {
                Log.Error("Could not reverse patch ScribeSaver.InitSaving, IL does not match expectations: setting XmlWriterSettings fields not found [ldloc.1].");
                return codeMatcher.Instructions();
            }
            var setXmlWriterSetIndentStartPos = codeMatcher.Pos;
            codeMatcher.CreateLabel(out var xmlWriterSetIndentLabel);

            codeMatcher.SearchForward(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == _method_XmlwriterSettings_setIndent);
            if (!codeMatcher.IsValid)
            {
                Log.Error("Could not reverse patch ScribeSaver.InitSaving, IL does not match expectations: setting XmlWriterSettings fields not found [callvirt].");
                return codeMatcher.Instructions();
            }
            codeMatcher.Advance(1);
            codeMatcher.CreateLabel(out var xmlWriterIndentCharsLabel);

            codeMatcher.Start();
            codeMatcher.Advance(setXmlWriterSetIndentStartPos);
            codeMatcher.Insert([
                // if (useIndentation) {
                new(OpCodes.Ldarg_3),
                //     <old code>
                new(OpCodes.Brtrue_S, xmlWriterSetIndentLabel),
                // } else {
                //     <skip old code>
                new(OpCodes.Br_S, xmlWriterIndentCharsLabel)
                // }
            ]);

            // Finally, we need to fix the exception handler
            codeMatcher.SearchForward(i => i.opcode == OpCodes.Stloc_2);
            if (!codeMatcher.IsValid)
            {
                Log.Error("Could not reverse patch ScribeSaver.InitSaving, IL does not match expectations: storing of Exception not found [stloc.2].");
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
                new(OpCodes.Ldloc_2),
                new(OpCodes.Call, _method_LogException),
            ]);

            return codeMatcher.Instructions();
        }

        // Make compiler happy. This gets patched out anyway.
        _ = scribeLoader;
        _ = stream;
        _ = documentElementName;
        _ = useIndentation;
        Transpiler(null!, null!);
    }
}

/*

/// <summary>
/// Adds the ability to use a different type of Stream than a FileStream to scribe.
/// See CustomStreamScribeSaver for usage.
/// </summary>
[HarmonyPatch(typeof(ScribeSaver), nameof(ScribeSaver.InitSaving))]
internal static class Verse_ScribeSaver_InitSaving
{
    private static bool UseAlternateStream()
    {
        return CustomStreamScribeSaver.customSaveStream != null;
    }
    private static Stream ProvideAlternateStream()
    {
        // Only called if the above method returns true; won't be null
        return CustomStreamScribeSaver.customSaveStream!;
    }

    private static readonly FieldInfo _fieldScribeMode = AccessTools.Field(typeof(Scribe), "mode");
    private static readonly FieldInfo _fieldSaveStream = AccessTools.Field(typeof(ScribeSaver), "saveStream");
    private static readonly MethodInfo _methodUseAlternateStream = SymbolExtensions.GetMethodInfo(() => UseAlternateStream());
    private static readonly MethodInfo _methodProvideAlternateStream = SymbolExtensions.GetMethodInfo(() => ProvideAlternateStream());
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codeMatcher = new CodeMatcher(instructions, generator);

        // Locate where Scribe.mode is assigned.
        codeMatcher.SearchForward(i => i.opcode == OpCodes.Stsfld && i.operand is FieldInfo f && f == _fieldScribeMode);
        if (!codeMatcher.IsValid)
        {
            Log.Error("Could not patch ScribeSaver.InitSaving, IL does not match expectations: assignment of Scribe.mode not found.");
            return codeMatcher.Instructions();
        }
        var scribeModeAssignmentPosition = codeMatcher.Pos;

        // The next ldarg.0 indicates the start of the creation of the FileStream
        codeMatcher.SearchForward(i => i.opcode == OpCodes.Ldarg_0);
        if (!codeMatcher.IsValid)
        {
            Log.Error("Could not patch ScribeSaver.InitSaving, IL does not match expectations: ldarg.0 never used???");
            return codeMatcher.Instructions();
        }
        var fileStreamCreateStartPos = codeMatcher.Pos;
        // Let's save it in a label too, so we can jump to it when we're *not*
        // overriding the stream.
        codeMatcher.CreateLabel(out var createFileStreamLabel);

        // stfld indicates the end of the FileStream creation
        codeMatcher.SearchForward(i => i.opcode == OpCodes.Stfld && i.operand is FieldInfo f && f == _fieldSaveStream);
        if (!codeMatcher.IsValid)
        {
            Log.Error("Could not patch ScribeSaver.InitSaving, IL does not match expectations: stfld never used???");
            return codeMatcher.Instructions();
        }
        var fileStreamCreateEndPos = codeMatcher.Pos;

        // We want to make a label at the instantiation of the XmlWriterSettings, as it's where
        // we'll jump to if we are going to override the stream. (i.e. over the creation of the
        // FileStream.)
        codeMatcher.Advance(1);
        if (codeMatcher.Opcode != OpCodes.Newobj || codeMatcher.Operand is not ConstructorInfo c || c.DeclaringType != typeof(XmlWriterSettings))
        {
            Log.Error("Could not patch ScribeSaver.InitSaving, IL does not match expectations: XmlWriterSettings never instantiated.");
            return codeMatcher.Instructions();
        }
        codeMatcher.CreateLabel(out var postFileStreamLabel);

        // With the prep-work out of the way, let's do some manipulation.
        // First, we add a check for whether we should do anything differently, and then
        // if we should, we store our custom stream instead of the file stream.
        codeMatcher.Start();
        codeMatcher.Advance(fileStreamCreateStartPos);
        codeMatcher.Insert([
            // if (UseAlternateStream()) {
            new(OpCodes.Call, _methodUseAlternateStream),
            new(OpCodes.Brfalse_S, createFileStreamLabel),
            //     saveStream = ProvideAlternateStream()
            new(OpCodes.Ldarg_0),
            new(OpCodes.Call, _methodProvideAlternateStream),
            new(OpCodes.Stfld, _fieldSaveStream),
            new(OpCodes.Br_S, postFileStreamLabel),
            // }
        ]);

        return codeMatcher.Instructions();
    }
}

*/
