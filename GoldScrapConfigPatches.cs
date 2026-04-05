using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using GoldScrapConfig.Store;
using HarmonyLib;
using UnityEngine;

namespace GoldScrapConfig.Patches;

[HarmonyPatch]
internal static class StorePricePatch
{
    private static MethodBase? TargetMethod() => HarmonyTargetHelpers.GoldScrapConfigs_SetCustomGoldStorePrices();

    private static bool Prepare() => TargetMethod() != null;

    private static readonly Dictionary<string, ConfigEntry<int>> PriceEntries = new();

    internal static void RegisterPerItemPrices(ConfigFile cfg)
    {
        PriceEntries.Clear();
        foreach (string id in GoldStoreItemIds.All)
        {
            if (string.Equals(id, GoldStoreDefaults.NoPriceOverrideFolder, StringComparison.Ordinal))
                continue;

            int def = GoldStoreDefaults.DefaultPriceOrMinusOne(id);
            PriceEntries[id] = cfg.Bind(
                "Store.Prices",
                id,
                def,
                new ConfigDescription("-1: LCGold default"));
        }
    }

    private static void Postfix()
    {
        var field = StoreFieldCache.AllGoldStoreItemData;
        if (field?.GetValue(null) is not Array arr)
            return;

        foreach (object? row in arr)
        {
            if (row == null)
                continue;

            string? folder = Traverse.Create(row).Field<string>("folderName").Value;
            if (string.IsNullOrEmpty(folder))
                continue;
            if (string.Equals(folder, GoldStoreDefaults.NoPriceOverrideFolder, StringComparison.Ordinal))
                continue;
            if (!PriceEntries.TryGetValue(folder, out ConfigEntry<int>? entry) || entry.Value < 0)
                continue;

            StorePriceApply.ApplyCreditsToItem(row, entry.Value);
        }
    }
}

[HarmonyPatch]
internal static class GoldOreFieldsPatch
{
    private static MethodBase? TargetMethod() => HarmonyTargetHelpers.GoldNuggetScript_Start();

    private static bool Prepare() => TargetMethod() != null;

    private static void Prefix(object __instance)
    {
        var tr = Traverse.Create(__instance);
        if (HarmonyTargetHelpers.IsGoldOreItem(tr.Field("itemType").GetValue()) == false)
            return;

        float bonus = Plugin.OreBonusCapMultiplier.Value;
        if (bonus > 0f && Mathf.Approximately(bonus, 1f) == false)
        {
            int cap = tr.Field<int>("oreBonusPerQuotaMaxIncrease").Value;
            tr.Field("oreBonusPerQuotaMaxIncrease").SetValue(Mathf.Max(1, Mathf.RoundToInt(cap * bonus)));
        }
    }
}

[HarmonyPatch]
internal static class CrownFieldsPatch
{
    private static MethodBase? TargetMethod() => HarmonyTargetHelpers.CrownScript_Start();

    private static bool Prepare() => TargetMethod() != null;

    private static void Prefix(object __instance)
    {
        var tr = Traverse.Create(__instance);

        float pct = Plugin.CrownPercentMultiplier.Value;
        if (pct > 0f && Mathf.Approximately(pct, 1f) == false)
        {
            int iv = tr.Field<int>("itemValuePercentage").Value;
            tr.Field("itemValuePercentage").SetValue(Mathf.Max(0, Mathf.RoundToInt(iv * pct)));
        }
    }
}
