using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SpecialPowerUtilities.Helpers;
using SpecialPowerUtilities.Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Triggers;
using Type = System.Type;

namespace SpecialPowerUtilities;

public class DynamicPatcher
{
    public static Harmony DynamicHarmony { get; set; } = null!;

    public static Dictionary<MethodBase, List<SimpleDynamicPatch>> Prefixes { get; set; } =
        new Dictionary<MethodBase, List<SimpleDynamicPatch>>();

    public static Dictionary<MethodBase, List<SimpleDynamicPatch>> Postfixes { get; set; } =
        new Dictionary<MethodBase, List<SimpleDynamicPatch>>();

    public static bool IsInitialized = false;

    public static void Initialize(IManifest manifest)
    {
        DynamicHarmony = new Harmony($"{manifest.UniqueID}_DynamicPatcher");
        var AllPatches =
            SpecialPowerUtilities.ModHelper.GameContent.Load<List<SimpleDynamicPatch>>(
                "Spiderbuttons.SpecialPowerUtilities/SimplePatches");

        if (AllPatches.Count == 0)
        {
            Log.Warn("No patches found.");
            return;
        }

        var prefixes = AllPatches.Where(p => p.Type == "Prefix" && (p.Action is not null || p.Actions is not null))
            .OrderByDescending(p => $"{p.Target.Type} {p.Target.Method}")
            .ThenByDescending(p => p.Priority).ToList();
        var postfixes = AllPatches.Where(p => p.Type == "Postfix" && (p.Action is not null || p.Actions is not null))
            .OrderBy(p => $"{p.Target.Type} {p.Target.Method}").ThenBy(p => p.Priority).ToList();

        foreach (var prefix in prefixes)
        {
            var prefixMethod = GetMethodFromString(prefix.Target, out string error);
            if (prefixMethod is null)
            {
                Log.Error($"failed to get method from string '{prefix.Target}': {error}");
                continue;
            }

            if (!Prefixes.ContainsKey(prefixMethod))
            {
                Prefixes.TryAdd(prefixMethod, new List<SimpleDynamicPatch> { prefix });
            }
            else
            {
                Prefixes[prefixMethod].Add(prefix);
            }
        }

        foreach (var postfix in postfixes)
        {
            var postfixMethod = GetMethodFromString(postfix.Target, out string error);
            if (postfixMethod is null)
            {
                Log.Error($"failed to get method from string '{postfix.Target}': {error}");
                continue;
            }
        
            if (!Postfixes.ContainsKey(postfixMethod))
            {
                Postfixes.TryAdd(postfixMethod, new List<SimpleDynamicPatch> {postfix});
            }
            else
            {
                Postfixes[postfixMethod].Add(postfix);
            }
        }

        var simpleFactory = AccessTools.Method(typeof(DynamicPatcher), nameof(SimpleFactory));
        try
        {
            foreach (var prefix in Prefixes)
            {
                DynamicHarmony.Patch(original: prefix.Key, postfix: new HarmonyMethod(simpleFactory));
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to patch prefixes: {ex}");
        }
        
        try
        {
            foreach (var postfix in Postfixes)
            {
                DynamicHarmony.Patch(original: postfix.Key, postfix: new HarmonyMethod(simpleFactory));
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to patch postfixes: {ex}");
        }

        IsInitialized = true;
    }

    public static DynamicMethod SimpleFactory(MethodBase method)
    {
        var info = method as MethodInfo;
        List<SimpleDynamicPatch> patches;
        if (!Prefixes.TryGetValue(method, out patches))
        {
            if (!Postfixes.TryGetValue(method, out patches))
            {
                Log.Warn($"No patches found for method '{method.Name}' in simple dictionaries.");
                return null;
            }
        }

        if (!TryGetMethodInfo(method, out var returnType, out var declaringType, out var parameterList,
                out var parseMethod))
        {
            return null;
        }

        DynamicMethod dynamicMethod =
            new DynamicMethod($"SPU_{info?.Name}_Prefix", typeof(void), parameterList.Select(p => p.ParameterType)
                .ToArray(), declaringType);

        foreach (var param in parameterList)
        {
            dynamicMethod.DefineParameter(param.Position + 1, param.Attributes, param.ToString().Split(" ")[1]);
        }

        ILGenerator il = dynamicMethod.GetILGenerator();

        foreach (var patchInfo in patches)
        {
            var skipBecauseConditionsLabel = il.DefineLabel();
            il.Emit(OpCodes.Ldstr, $"Adding {patchInfo.Type} patch '{patchInfo.Id}' with priority {patchInfo.Priority.ToString()} to method '{info?.Name}'");;
            il.Emit(OpCodes.Call,
                AccessTools.Method(typeof(Log), nameof(Log.Trace), null, new Type[] { typeof(string) }));
            il.Emit(OpCodes.Ldstr, patchInfo.Condition ?? "true");
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Call, AccessTools.Method(typeof(GameStateQuery), nameof(GameStateQuery.CheckConditions),
                new Type[]
                {
                    typeof(string), typeof(GameLocation), typeof(Farmer), typeof(Item), typeof(Item), typeof(Random),
                    typeof(HashSet<string>)
                }));
            il.Emit(OpCodes.Brfalse, skipBecauseConditionsLabel);
            
            if (patchInfo.Action is not null)
            {
                il.Emit(OpCodes.Ldstr, patchInfo.Id);
                il.Emit(OpCodes.Ldstr, patchInfo.Action);
                il.Emit(OpCodes.Call,
                    AccessTools.Method(typeof(DynamicPatcher), nameof(TryRunSimplePatchAction)));
            }
            else if (patchInfo.Actions is not null)
            {
                foreach (var action in patchInfo.Actions)
                {
                    il.Emit(OpCodes.Ldstr, patchInfo.Id);
                    il.Emit(OpCodes.Ldstr, action);
                    il.Emit(OpCodes.Call,
                        AccessTools.Method(typeof(DynamicPatcher), nameof(TryRunSimplePatchAction)));
                }
            }
            
            il.MarkLabel(skipBecauseConditionsLabel);
        }
        il.Emit(OpCodes.Ret);

        return dynamicMethod;
    }

    public static void TryRunSimplePatchAction(string patchID, string action)
     {
        if (TriggerActionManager.TryRunAction(action, out var error, out var ex)) return;
        Log.Warn($"Failed to run action '{action}' for simple patch '{patchID}': {error}");
    }

    public static bool TryGetMethodInfo(MethodBase method, out Type returnType, out Type declaringType,
        out ParameterInfo[] parameterList, out MethodInfo parseMethod)
    {
        var info = method as MethodInfo;
        if (info is null)
        {
            returnType = typeof(void);
            declaringType = null;
            parameterList = null;
            parseMethod = null;
            return false;
        }

        returnType = info.ReturnType;
        declaringType = info.DeclaringType;
        parameterList = info.GetParameters();
        parseMethod = returnType.GetMethod("TryParse", new Type[] { typeof(string), returnType.MakeByRefType() });
        
        return true;
    }

    public static MethodInfo GetMethodFromString(TargetMethod target, out string error)
    {
        if (string.IsNullOrWhiteSpace(target.Type))
        {
            error = "the type name can't be empty";
            return null;
        }
        if (string.IsNullOrWhiteSpace(target.Method))
        {
            error = "the method name can't be empty";
            return null;
        }

        string fullTypeName =  $"{target.Type}, {target.Assembly}";

        Type type = Type.GetType(fullTypeName);
        if (type == null)
        {
            error = $"could not find type '{fullTypeName}'";
            return null;
        }
        
        if (target.PropertyGetter)
        {
            var property = type.GetProperty(target.Method);
            if (property is null)
            {
                error = $"could not find property '{target.Method}' on type '{fullTypeName}'";
                return null;
            }

            var getter = property.GetGetMethod();
            if (getter is null)
            {
                error = $"property '{target.Method}' on type '{fullTypeName}' does not have a getter";
                return null;
            }

            error = null;
            return getter;
        }
        
        if (target.PropertySetter)
        {
            var property = type.GetProperty(target.Method);
            if (property is null)
            {
                error = $"could not find property '{target.Method}' on type '{fullTypeName}'";
                return null;
            }

            var setter = property.GetSetMethod();
            if (setter is null)
            {
                error = $"property '{target.Method}' on type '{fullTypeName}' does not have a setter";
                return null;
            }

            error = null;
            return setter;
        }
        
        var methods = type.GetMethods();
        if (methods.Length == 0)
        {
            error = $"could not find any methods on type '{fullTypeName}'";
            return null;
        }

        var method = methods.FirstOrDefault(m =>
        {
            if (m.Name != target.Method)
            {
                return false;
            }

            var parameters = m.GetParameters();
            if (parameters.Length != target.Parameters.Length)
            {
                return false;
            }

            return !parameters.Where((t, i) => t.ParameterType.Name != target.Parameters[i] && t.ParameterType.FullName != target.Parameters[i]).Any();
        });
        
        if (method is not null)
        {
            error = null;
            return method;
        }
        
        error = $"could not find method '{target.Method}' with the specified parameters {string.Join(", ", target.Parameters)} on type '{fullTypeName}'";
        return null;
    }
}