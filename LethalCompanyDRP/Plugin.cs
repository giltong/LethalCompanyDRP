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
        
        private static Discord.Discord _discord;
        private static ActivityManager _activityManager;
        private static long _clientId = 1176318786194911254;
        private static Activity _activity = new Activity { Instance = true };
        private static bool InGame = false;
        
        
        public Activity defaultActivity = new Activity()
        {
            State = "Test State",
            Details = "Test Details",
        };
        private static void StartDiscordRPC()
        {
            /* Give some time for game to initialize */
            Thread.Sleep(5000);
            Plugin.log.LogInfo("Discord RPC Started");
            _discord = new Discord.Discord(_clientId, (UInt64)CreateFlags.NoRequireDiscord);
            _activityManager = _discord.GetActivityManager();
            if (_activityManager == null) return;
            _activityManager.RegisterSteam(1966720);
            _activity.Timestamps.Start = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            UpdateActivity();
            try
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(500);
                        _discord.RunCallbacks();
                        UpdateActivity();
                    }
                    catch(ResultException e)
                    {
                        log.LogError("Discord throws a ResultException: " + e.Message);
                    }
                }
            }
            finally
            {
                _discord.Dispose();
            }
        }
        
        private static void UpdateActivity()
        {
            if (_activityManager == null) return;

            CheckCurrentScene();
            
            _activity.Assets.LargeImage = "game_icon";
            _activity.Assets.LargeText = "Lethal Company";
            
            if (InGame)
            {
                _activity.Details = "Profit Quota: " + TimeOfDay.Instance.quotaFulfilled + " / " + TimeOfDay.Instance.profitQuota;
                _activity.State = TimeOfDay.Instance.daysUntilDeadline + " days remaining";
            }
            else
            {
                _activity.State = "in Main Menu";
                _activity.Secrets.Join = null;
            }
            
            try
            {
                _activityManager.UpdateActivity(_activity, result => {});
            }
            catch (Exception e)
            {
                log.LogError("Discord::UpdateActivity throws a " + e.GetType() + ":\n" + e.Message);
                throw;
            }
        }

        private static void CheckCurrentScene()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name is "MainMenu")
                {
                    if (InGame)
                    {
                        _activity.Timestamps.Start = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                    }
                    InGame = false;
                }
                else if (SceneManager.GetSceneAt(i).name == "SampleSceneRelay" && SceneManager.GetSceneAt(i).isLoaded)
                {
                    if (!InGame)
                    {
                        _activity.Timestamps.Start = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                    }
                    InGame = true;
                }
            } 
        }

        private void Awake()
        {
            Plugin.log = base.Logger;
            Logger.LogInfo($"Started {PluginInfo.PLUGIN_GUID}");
            Thread discordThread = new Thread(StartDiscordRPC);
            discordThread.Start();
        }
        
    }
}
