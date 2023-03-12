using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;

namespace PlayableDracula
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<int> BaseMinDamage;
        public static ConfigEntry<int> BaseMaxDamage;
        public static ConfigEntry<int> BleedChancePercent;

        public static ConfigEntry<float> AbilityCooldown;

        public static ConfigEntry<int> OnBleedHealing;
        public static ConfigEntry<float> FullGaugeHealingMultiplyBy;

        public static ConfigEntry<string> DummySwapSkullName;
        public static ConfigEntry<string> DummySwapName;
        public static ConfigEntry<string> DummySwapDescription;
        public static ConfigEntry<float> DummySwapVFXDuration;


        public void Awake()
        {
            BaseMinDamage = Config.Bind("Attack", "BaseMinDamage", 15, "Minimal base damage");
            BaseMaxDamage = Config.Bind("Attack", "BaseMaxDamage", 15, "Maximal base damage");
            BleedChancePercent = Config.Bind("Attack", "BleedChancePercent", 10, "Chance to apply wound (in percent)");

            AbilityCooldown = Config.Bind("Abilities", "AbilityCooldown", 3f, "Ability cooldown (in seconds)");

            OnBleedHealing = Config.Bind("Abilities", "OnBleedHealing", 1, "Amount of HP to heal on bleeding applied");
            FullGaugeHealingMultiplyBy = Config.Bind("Abilities", "FullGaugeHealingMultiplyBy", 3f, "Healing multiplier on full gauge");

            DummySwapSkullName = Config.Bind("Abilities", "DummySwapSkullName", "Skeleton_Sword_2", "WeaponReference.name of a skull to get the swap from (Swap must be a SimpleAction)");
            DummySwapName = Config.Bind("Abilities", "DummySwapName", "Bloodlust", "Name of a swap ability");
            DummySwapDescription = Config.Bind("Abilities", "DummySwapDescription", "", "Description of a swap ability (Will auto-load if empty)");
            DummySwapVFXDuration = Config.Bind("Abilities", "DummySwapVFXDuration", 8f, "Duration of visual effects on swap");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
