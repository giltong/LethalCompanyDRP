using System;
using Discord;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalCompanyDRP;

public class DiscordRP : MonoBehaviour
{
    private Discord.Discord _discord;
    private ActivityManager _activityManager;
    private readonly long _clientId = 1176318786194911254;
    private Activity _activity = new() { Instance = true };
    private bool _inGame;
    private void Start()
    {
        _discord = new Discord.Discord(_clientId, (UInt64)CreateFlags.NoRequireDiscord);
        _activityManager = _discord.GetActivityManager();
        if (_activityManager == null) return;
        _activityManager.RegisterSteam(1966720);
        _activity.Timestamps.Start = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
        Plugin.log.LogInfo("Discord RPC Started");
    }

    private void Update()
    {
        try
        {
            _discord.RunCallbacks();
            UpdateActivity();
        }
        catch(ResultException e)
        {
            Plugin.log.LogError("Discord throws a ResultException: " + e.Message);
        }
    }
    
    private void UpdateActivity()
    {
        if (_activityManager == null) return;
    
        CheckCurrentScene();
        
        _activity.Assets.LargeImage = "game_icon";
        _activity.Assets.LargeText = "Lethal Company";
        
        if (_inGame)
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
            Plugin.log.LogError("Discord::UpdateActivity throws a " + e.GetType() + ":\n" + e.Message);
            throw;
        }
    }
    
    private void CheckCurrentScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name is "MainMenu")
            {
                if (_inGame)
                {
                    _activity.Timestamps.Start = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                }
                _inGame = false;
            }
            else if (SceneManager.GetSceneAt(i).name == "SampleSceneRelay" && SceneManager.GetSceneAt(i).isLoaded)
            {
                if (!_inGame)
                {
                    _activity.Timestamps.Start = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                }
                _inGame = true;
            }
        } 
    }
}