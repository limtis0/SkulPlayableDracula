using BepInEx;
using HarmonyLib;

namespace PlayableDracula
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Harmony harmony = new(PluginInfo.PLUGIN_GUID);
            PlayableDracula.PatchAll(harmony);
        }
    }
}
