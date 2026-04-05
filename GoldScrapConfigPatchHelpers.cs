using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace GoldScrapConfig.Patches;

internal static class HarmonyTargetHelpers
{
    private static readonly Lazy<MethodBase?> SetCustomGoldStorePrices = new(() =>
    {
        var t = AccessTools.TypeByName("GoldScrapConfigs");
        return t == null ? null : AccessTools.Method(t, "SetCustomGoldStorePrices", new[] { typeof(float) });
    });

    private static readonly Lazy<MethodBase?> GoldNuggetStart = new(() =>
    {
        var t = AccessTools.TypeByName("GoldNuggetScript");
        return t == null ? null : AccessTools.Method(t, "Start");
    });

    private static readonly Lazy<MethodBase?> CrownStart = new(() =>
    {
        var t = AccessTools.TypeByName("CrownScript");
        return t == null ? null : AccessTools.Method(t, "Start");
    });

    private static readonly Dictionary<Type, object> GoldOreValueByEnumType = new();

    internal static MethodBase? GoldScrapConfigs_SetCustomGoldStorePrices() => SetCustomGoldStorePrices.Value;

    internal static MethodBase? GoldNuggetScript_Start() => GoldNuggetStart.Value;

    internal static MethodBase? CrownScript_Start() => CrownStart.Value;

    internal static bool IsGoldOreItem(object? itemType)
    {
        if (itemType == null)
            return false;

        Type t = itemType.GetType();
        lock (GoldOreValueByEnumType)
        {
            if (!GoldOreValueByEnumType.TryGetValue(t, out object? goldOre))
            {
                try
                {
                    goldOre = Enum.Parse(t, "GoldOre");
                }
                catch (ArgumentException)
                {
                    return false;
                }

                GoldOreValueByEnumType[t] = goldOre;
            }

            return goldOre.Equals(itemType);
        }
    }
}

internal static class StoreFieldCache
{
    private static readonly FieldInfo? AllItems = MakeField();

    private static FieldInfo? MakeField()
    {
        var t = AccessTools.TypeByName("StoreAndTerminal");
        return t == null ? null : AccessTools.Field(t, "allGoldStoreItemData");
    }

    internal static FieldInfo? AllGoldStoreItemData => AllItems;
}
