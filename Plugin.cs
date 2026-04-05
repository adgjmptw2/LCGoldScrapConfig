using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GoldScrapConfig.Patches;
using HarmonyLib;

namespace GoldScrapConfig;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("LCGoldScrapMod", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;

    internal static ConfigEntry<float> OreBonusCapMultiplier = null!;
    internal static ConfigEntry<float> CrownPercentMultiplier = null!;

    private Harmony harmony = null!;

    private void Awake()
    {
        Log = Logger;
        OreBonusCapMultiplier = Config.Bind(
            "Ore",
            "Ore Bonus multiplier",
            1f,
            "Ore bonus cap ×. 1 = vanilla.");
        CrownPercentMultiplier = Config.Bind(
            "Crown",
            "Crown multiplier",
            1f,
            "Crown value % ×. 1 = vanilla.");

        StorePricePatch.RegisterPerItemPrices(Config);

        harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(Plugin).Assembly);
        Log.LogInfo(
            PluginInfo.PLUGIN_NAME + " " + PluginInfo.PLUGIN_VERSION +
            " ore×" + OreBonusCapMultiplier.Value +
            " crown×" + CrownPercentMultiplier.Value +
            " " + Config.ConfigFilePath);
    }
}

internal static class PluginInfo
{
    public const string PLUGIN_GUID = "mine9289.GoldScrapConfig";
    public const string PLUGIN_NAME = "GoldScrapConfig";
    public const string PLUGIN_VERSION = "0.1.0";
}
