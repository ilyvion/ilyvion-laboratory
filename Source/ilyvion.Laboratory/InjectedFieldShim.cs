// InjectedFieldShim.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

using System.Reflection.Emit;

namespace ilyvion.Laboratory;

/// <summary>
/// Gives <c>Waylith.Injection.InjectedFieldAttribute</c> accessors storage on RimWorld 1.6 and
/// earlier, where the game doesn't inject fields: it finds the accessors the way RimWorld 1.7
/// does and replaces their bodies with calls into <see cref="InjectedFieldStorage"/>.
/// </summary>
internal static class InjectedFieldShim
{
    internal const string AttributeFullName = "Waylith.Injection.InjectedFieldAttribute";

    private const string ExtensionMarkerAttributeFullName =
        "System.Runtime.CompilerServices.ExtensionMarkerAttribute";

    private const BindingFlags AllStatic =
        BindingFlags.DeclaredOnly
        | BindingFlags.Static
        | BindingFlags.Public
        | BindingFlags.NonPublic;

    private static readonly Lock gate = new();
    private static readonly HashSet<MethodBase> processed = [];
    private static readonly Dictionary<MethodBase, Plan> plans = [];
    private static readonly Harmony harmony = new("ilyvion.Laboratory.InjectedFields");

    private enum AccessorKind
    {
        Ref,
        Get,
        Set,
    }

    private sealed class Request(
        string owner,
        MethodInfo getter,
        MethodInfo? setter,
        Type target,
        Type fieldType
    )
    {
        public string Owner { get; } = owner;
        public MethodInfo Getter { get; } = getter;
        public MethodInfo? Setter { get; } = setter;
        public Type Target { get; } = target;
        public Type FieldType { get; } = fieldType;
        public object? Default { get; set; }
        public MethodInfo? Initializer { get; set; }
    }

    private sealed class Plan(MethodInfo storageMethod, int slot, bool targetByRef)
    {
        public MethodInfo StorageMethod { get; } = storageMethod;
        public int Slot { get; } = slot;
        public bool TargetByRef { get; } = targetByRef;
    }

    internal sealed class Result
    {
        public int Bound { get; internal set; }
        public List<string> Errors { get; } = [];
    }

    /// <summary>
    /// Binds the injected field accessors in every running mod's assemblies that reference
    /// ilyvion's Laboratory, and logs the ones that can't be bound.
    /// </summary>
    internal static void ProcessRunningMods()
    {
        var laboratoryName = typeof(InjectedFieldShim).Assembly.GetName().Name;
        var types = LoadedModManager
            .RunningMods.SelectMany(mod => mod.assemblies.loadedAssemblies)
            .Distinct()
            .Where(assembly =>
                assembly.GetReferencedAssemblies().Any(name => name.Name == laboratoryName)
            )
            .SelectMany(GetTypes);

        var result = Process(types, AttributeFullName);
        foreach (var error in result.Errors)
        {
            Logger.LogError("Injected field " + error);
        }
        if (result.Bound > 0)
        {
            Logger.LogDevMessage($"Bound {result.Bound} injected field(s).");
        }
    }

    /// <summary>
    /// Binds the injected field accessors declared in <paramref name="types"/>, recognizing them
    /// by the attribute named <paramref name="attributeFullName"/>. Accessors bound before are
    /// skipped.
    /// </summary>
    internal static Result Process(IEnumerable<Type> types, string attributeFullName)
    {
        var result = new Result();
        lock (gate)
        {
            List<Request> requests = [];
            foreach (var type in types)
            {
                try
                {
                    CollectType(type, attributeFullName, requests, result.Errors);
                }
                catch (Exception e)
                    when (e is TypeLoadException or FileNotFoundException or BadImageFormatException
                    )
                {
                    result.Errors.Add(
                        $"scan skipped {type.FullName} ({e.GetType().Name}: {e.Message})"
                    );
                }
            }

            foreach (var request in requests)
            {
                try
                {
                    Bind(request);
                    result.Bound++;
                }
                catch (Exception e)
                {
                    result.Errors.Add($"{request.Owner}: binding failed: {e}");
                }
            }
        }
        return result;
    }

    private static IEnumerable<Type> GetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.OfType<Type>();
        }
    }

    private static void CollectType(
        Type type,
        string attributeFullName,
        List<Request> requests,
        List<string> errors
    )
    {
        var isStaticClass = type.IsAbstract && type.IsSealed;
        foreach (var method in type.GetMethods(AllStatic))
        {
            var attribute = FindAttribute(method, attributeFullName);
            if (attribute is null || !processed.Add(method))
            {
                continue;
            }
            if (!isStaticClass)
            {
                errors.Add(Owner(method) + " must be declared in a static class.");
                continue;
            }
            CollectAccessorMethod(method, type, attribute, requests, errors);
        }

        if (
            type.IsNested
            && type.IsSealed
            && type.DeclaringType is { IsAbstract: true, IsSealed: true } container
            && type.IsDefined(typeof(ExtensionAttribute), false)
        )
        {
            CollectExtensionProperties(type, container, attributeFullName, requests, errors);
        }
    }

    private static void CollectAccessorMethod(
        MethodInfo accessor,
        Type declaringType,
        CustomAttributeData attribute,
        List<Request> requests,
        List<string> errors
    )
    {
        var owner = Owner(accessor);
        var parameters = accessor.GetParameters();
        if (parameters.Length != 1 || accessor.ReturnType == typeof(void))
        {
            errors.Add(
                owner
                    + " must take exactly the target instance and return the field (by value or by ref)."
            );
            return;
        }
        if (!IsBodyReplaceable(accessor, owner, errors))
        {
            return;
        }

        var request = new Request(
            owner,
            accessor,
            null,
            parameters[0].ParameterType,
            StripByRef(accessor.ReturnType)
        );
        if (Validate(request, declaringType, attribute, errors))
        {
            requests.Add(request);
        }
    }

    private static void CollectExtensionProperties(
        Type marker,
        Type container,
        string attributeFullName,
        List<Request> requests,
        List<string> errors
    )
    {
        var properties = marker.GetProperties(
            BindingFlags.DeclaredOnly
                | BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic
        );
        foreach (var property in properties)
        {
            var attribute = FindAttribute(property, attributeFullName);
            if (attribute is null)
            {
                continue;
            }
            var getter = property.GetGetMethod(true);
            var setter = property.GetSetMethod(true);
            var first = getter ?? setter;
            if (first is null || !processed.Add(first))
            {
                continue;
            }

            var owner =
                $"{container.Assembly.GetName().Name}: {container.FullName}.{property.Name}";
            if (getter is null)
            {
                errors.Add(owner + " needs a getter.");
                continue;
            }
            if (getter.IsStatic)
            {
                errors.Add(
                    owner
                        + " is a static extension property. Only instance extension properties can be used for injected fields."
                );
                continue;
            }
            if (marker.IsGenericTypeDefinition || container.IsGenericTypeDefinition)
            {
                errors.Add(
                    owner
                        + " extends a generic type. ilyvion's Laboratory only injects fields into non-generic classes on RimWorld 1.6 and earlier."
                );
                continue;
            }

            var receiver = FindReceiver(marker, property);
            if (receiver is null)
            {
                errors.Add(
                    owner
                        + ": couldn't find the extension receiver type. The compiler output isn't what we expected."
                );
                continue;
            }

            var getterImplementation = FindImplementation(container, getter, receiver);
            var setterImplementation = setter is null
                ? null
                : FindImplementation(container, setter, receiver);
            if (
                getterImplementation is null
                || (setter is not null && setterImplementation is null)
            )
            {
                errors.Add(
                    $"{owner}: could not find the static implementation method(s) on {container.FullName}."
                );
                continue;
            }
            if (
                !IsBodyReplaceable(getterImplementation, owner, errors)
                || (
                    setterImplementation is not null
                    && !IsBodyReplaceable(setterImplementation, owner, errors)
                )
            )
            {
                continue;
            }

            var request = new Request(
                owner,
                getterImplementation,
                setterImplementation,
                receiver,
                StripByRef(getterImplementation.ReturnType)
            );
            if (Validate(request, container, attribute, errors))
            {
                requests.Add(request);
            }
        }
    }

    private static Type? FindReceiver(Type marker, PropertyInfo property)
    {
        string? groupName = null;
        foreach (
            var member in new MemberInfo?[]
            {
                property,
                property.GetGetMethod(true),
                property.GetSetMethod(true),
            }
        )
        {
            if (member is null)
            {
                continue;
            }
            var markerAttribute = FindAttribute(member, ExtensionMarkerAttributeFullName);
            if (markerAttribute is { ConstructorArguments.Count: 1 })
            {
                groupName = markerAttribute.ConstructorArguments[0].Value as string;
                break;
            }
        }

        var nested = marker.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
        var named = groupName is null ? null : nested.FirstOrDefault(n => n.Name == groupName);
        foreach (var candidate in named is null ? nested : [named])
        {
            var parameters = candidate.GetMethod("<Extension>$", AllStatic)?.GetParameters();
            if (parameters is { Length: 1 })
            {
                return parameters[0].ParameterType;
            }
        }
        return null;
    }

    private static MethodInfo? FindImplementation(
        Type container,
        MethodInfo skeleton,
        Type receiver
    )
    {
        var skeletonParameters = skeleton.GetParameters();
        foreach (var method in container.GetMethods(AllStatic))
        {
            if (
                method.Name != skeleton.Name
                || method.IsGenericMethodDefinition
                || method.ReturnType != skeleton.ReturnType
            )
            {
                continue;
            }
            var parameters = method.GetParameters();
            if (
                parameters.Length == skeletonParameters.Length + 1
                && parameters[0].ParameterType == receiver
                && skeletonParameters
                    .Select((p, i) => parameters[i + 1].ParameterType == p.ParameterType)
                    .All(matches => matches)
            )
            {
                return method;
            }
        }
        return null;
    }

    private static bool IsBodyReplaceable(MethodInfo method, string owner, List<string> errors)
    {
        if (!HasMethodBody(method))
        {
            errors.Add(
                owner
                    + " has no method body (extern), so ilyvion's Laboratory can't replace it on RimWorld 1.6 and earlier. Give it a body that throws an exception instead."
            );
            return false;
        }
        return true;
    }

    private static bool HasMethodBody(MethodInfo method)
    {
        try
        {
            return method.GetMethodBody() is not null;
        }
        // Mono throws this for an extern method instead of returning null.
        catch (BadImageFormatException)
        {
            return false;
        }
    }

    private static bool Validate(
        Request request,
        Type initializerHost,
        CustomAttributeData attribute,
        List<string> errors
    )
    {
        var target = StripByRef(request.Target);
        if (target.IsInterface || target.IsValueType || !target.IsClass || target.IsArray)
        {
            errors.Add(
                $"{request.Owner}: target {target} must be a class (interfaces and structs are not injectable)."
            );
            return false;
        }
        if (target.IsGenericType)
        {
            errors.Add(
                $"{request.Owner}: target {target} is generic. ilyvion's Laboratory only injects fields into non-generic classes on RimWorld 1.6 and earlier."
            );
            return false;
        }
        var fieldType = request.FieldType;
        if (
            fieldType.ContainsGenericParameters
            || fieldType.IsByRef
            || fieldType.IsPointer
            || IsByRefLike(fieldType)
        )
        {
            errors.Add(
                $"{request.Owner}: field type {fieldType} cannot be stored in a field (open generic, byref, byref-like or pointer)."
            );
            return false;
        }

        object? defaultValue = null;
        string? initializerName = null;
        foreach (var argument in attribute.NamedArguments)
        {
            switch (argument.MemberName)
            {
                case "Default":
                    defaultValue = argument.TypedValue.Value;
                    break;
                case "Initializer":
                    initializerName = argument.TypedValue.Value as string;
                    break;
                default:
                    break;
            }
        }

        if (defaultValue is not null)
        {
            request.Default = NormalizeDefault(defaultValue, fieldType, out var error);
            if (request.Default is null)
            {
                errors.Add($"{request.Owner}: {error}");
                return false;
            }
        }

        if (initializerName is not null)
        {
            var candidates = initializerHost
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m =>
                    m.Name == initializerName
                    && !m.IsGenericMethodDefinition
                    && fieldType.IsAssignableFrom(m.ReturnType)
                    && (m.ReturnType == fieldType || !fieldType.IsValueType)
                )
                .Where(m =>
                {
                    var parameters = m.GetParameters();
                    return parameters.Length == 0
                        || (
                            parameters.Length == 1
                            && parameters[0].ParameterType.IsAssignableFrom(target)
                        );
                })
                .ToList();
            if (candidates.Count != 1)
            {
                errors.Add(
                    $"{request.Owner}: initializer {initializerName} must be exactly one static method on {initializerHost.FullName} returning {fieldType} and taking the target or nothing ({candidates.Count} found)."
                );
                return false;
            }
            request.Initializer = candidates[0];
        }
        return true;
    }

    private static object? NormalizeDefault(object value, Type fieldType, out string error)
    {
        error = "";
        try
        {
            if (fieldType == typeof(string))
            {
                if (value is string)
                {
                    return value;
                }
            }
            else if (fieldType.IsEnum)
            {
                return Enum.ToObject(
                    fieldType,
                    Convert.ChangeType(
                        value,
                        Enum.GetUnderlyingType(fieldType),
                        CultureInfo.InvariantCulture
                    )
                );
            }
            else if (
                fieldType.IsPrimitive
                && fieldType != typeof(IntPtr)
                && fieldType != typeof(UIntPtr)
            )
            {
                return Convert.ChangeType(value, fieldType, CultureInfo.InvariantCulture);
            }
        }
        catch (Exception e) when (e is InvalidCastException or FormatException or OverflowException)
        {
            error = $"default value {value} does not convert to {fieldType}: {e.Message}";
            return null;
        }
        error =
            $"a default value only works on a primitive, enum or string field, not {fieldType}. Use an initializer instead.";
        return null;
    }

    private static void Bind(Request request)
    {
        var target = StripByRef(request.Target);
        var targetByRef = request.Target.IsByRef;
        if (
            RequireMethod(
                    typeof(InjectedFieldStorage),
                    nameof(InjectedFieldStorage.AddSlot),
                    BindingFlags.Static | BindingFlags.NonPublic
                )
                .MakeGenericMethod(target, request.FieldType)
                .Invoke(null, [request.Default, request.Initializer])
            is not int slot
        )
        {
            throw new InvalidOperationException("InjectedFieldStorage.AddSlot returned no slot.");
        }

        var getterKind = request.Getter.ReturnType.IsByRef ? AccessorKind.Ref : AccessorKind.Get;
        Patch(request.Getter, new Plan(StorageMethod(getterKind), slot, targetByRef));
        if (request.Setter is not null)
        {
            Patch(request.Setter, new Plan(StorageMethod(AccessorKind.Set), slot, targetByRef));
        }

        MethodInfo StorageMethod(AccessorKind kind)
        {
            return RequireMethod(
                    typeof(InjectedFieldStorage),
                    kind.ToString(),
                    BindingFlags.Static | BindingFlags.Public
                )
                .MakeGenericMethod(target, request.FieldType);
        }
    }

    private static void Patch(MethodInfo accessor, Plan plan)
    {
        plans[accessor] = plan;
        try
        {
            _ = harmony.Patch(
                accessor,
                transpiler: new HarmonyMethod(
                    RequireMethod(
                        typeof(InjectedFieldShim),
                        nameof(Transpiler),
                        BindingFlags.Static | BindingFlags.NonPublic
                    )
                )
            );
        }
        catch
        {
            _ = plans.Remove(accessor);
            throw;
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> _,
        MethodBase original
    )
    {
        Plan plan;
        lock (gate)
        {
            plan = plans[original];
        }

        yield return new CodeInstruction(OpCodes.Ldc_I4, plan.Slot);
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        if (plan.TargetByRef)
        {
            yield return new CodeInstruction(OpCodes.Ldind_Ref);
        }
        if (plan.StorageMethod.GetParameters().Length == 3)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
        }
        yield return new CodeInstruction(OpCodes.Call, plan.StorageMethod);
        yield return new CodeInstruction(OpCodes.Ret);
    }

    private static CustomAttributeData? FindAttribute(
        MemberInfo member,
        string attributeFullName
    ) =>
        member
            .GetCustomAttributesData()
            .FirstOrDefault(a => a.AttributeType.FullName == attributeFullName);

    private static bool IsByRefLike(Type type) =>
        FindAttribute(type, "System.Runtime.CompilerServices.IsByRefLikeAttribute") is not null;

    private static Type StripByRef(Type type) =>
        type.IsByRef ? type.GetElementType() ?? type : type;

    private static MethodInfo RequireMethod(Type type, string name, BindingFlags flags) =>
        type.GetMethod(name, flags) ?? throw new MissingMethodException(type.FullName, name);

    private static string Owner(MethodBase method) =>
        $"{method.Module.Assembly.GetName().Name}: {method.DeclaringType?.FullName}.{method.Name}";
}
