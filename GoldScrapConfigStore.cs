using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GoldScrapConfig.Store;

internal static class GoldStoreDefaults
{
    internal const string NoPriceOverrideFolder = "GoldenTicket";

    internal static readonly Dictionary<string, int> StoreDefaultPriceByFolder = new(StringComparer.Ordinal)
    {
        ["BronzeSuit"] = 2500,
        ["CatOGold"] = 777,
        ["CreditsCard"] = -1,
        ["GoldCrown"] = 5000,
        ["GoldfatherClock"] = 612,
        ["GoldMedal"] = 3333,
        ["GoldNugget"] = 100,
        ["GoldOre"] = 500,
        ["GoldSuit"] = 10000,
        ["GoldenGlove"] = 800,
        ["GoldenHourglass"] = 133,
        ["GoldenPickaxe"] = 75,
        ["GoldenThrone"] = 25000,
        ["GoldToilet"] = 450,
        ["GoldTrophy"] = 2000,
        ["GroovyGold"] = 150,
        ["SafeBox"] = 1000,
        ["SilverSuit"] = 5000,
    };

    internal static int DefaultPriceOrMinusOne(string folderName) =>
        StoreDefaultPriceByFolder.TryGetValue(folderName, out int v) ? v : -1;
}

internal static class GoldStoreItemIds
{
    internal static readonly string[] All =
    {
        "BronzeSuit",
        "CatOGold",
        "CreditsCard",
        "GoldCrown",
        "GoldfatherClock",
        "GoldMedal",
        "GoldNugget",
        "GoldOre",
        "GoldSuit",
        "GoldenGlove",
        "GoldenHourglass",
        "GoldenPickaxe",
        "GoldenThrone",
        "GoldenTicket",
        "GoldToilet",
        "GoldTrophy",
        "GroovyGold",
        "SafeBox",
        "SilverSuit",
    };
}

internal static class StorePriceApply
{
    internal static void ApplyCreditsToItem(object itemData, int credits)
    {
        if (credits < 0 || itemData == null)
            return;

        var tr = Traverse.Create(itemData);
        tr.Field("localStorePrice").SetValue(credits);

        var itemProps = tr.Field("itemProperties").GetValue();
        if (itemProps != null && IsUnityNull(itemProps) == false)
            Traverse.Create(itemProps).Field("creditsWorth").SetValue(credits);

        var nodesObj = tr.Field("storeTerminalNodes").GetValue();
        if (nodesObj is not Array nodes)
            return;

        foreach (var node in nodes)
        {
            if (node != null && IsUnityNull(node) == false)
                Traverse.Create(node).Field("itemCost").SetValue(credits);
        }
    }

    private static bool IsUnityNull(object o) => (Object)o == null;
}
