using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace LethalCompanyDRP
{
    
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource log;
        
        private void Awake()
        {
            log = Logger;
            log.LogInfo($"Started {PluginInfo.PLUGIN_GUID}");
            GameObject discordGameObject = new GameObject();
            discordGameObject.AddComponent<DiscordRP>();
            DontDestroyOnLoad(discordGameObject);
            discordGameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
