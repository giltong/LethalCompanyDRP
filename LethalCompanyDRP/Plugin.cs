using System;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using Discord;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        }

        private void OnDestroy()
        {
            GameObject discordGameObject = new GameObject();
            DontDestroyOnLoad(discordGameObject);
            discordGameObject.AddComponent<DiscordRP>();
        }
    }
}
