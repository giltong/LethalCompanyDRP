using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace LethalCompanyDRP;

public class DiscordRP : MonoBehaviour
{
    private Discord.Discord _discord;
    private ActivityManager _activityManager;
    private readonly long _clientId = 1176318786194911254;
    private Activity _activity = new() { Instance = true };
    private bool _inGame;

    public static Dictionary<string, string> planets = new()
    {
        {"CompanyBuilding", "The Company Building"},
        {"Level1Experimentation","41-Experimentation"},
        {"Level2Assurance","220-Assurance"},
        {"Level3Vow","56-Vow"},
        {"Level4March","61-March"},
        {"Level5Rend","85-Rend"},
        {"Level6Dine","7-Dine"},
        {"Level7Offense","21-Offense"},
        {"Level8Titan","8-Titan"},
    };

    private string _currentPlanet;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    

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
            Plugin.log.LogError("Discord throws an exception: " + e.Message);
        }
    }
    
    private void UpdateActivity()
    {
        if (_activityManager == null) return;
        
        
        _activity.Assets.LargeImage = "game_icon";
        _activity.Assets.LargeText = "Lethal Company";
        
        if (_inGame)
        {
            _activity.Details = "Profit Quota: " + TimeOfDay.Instance.quotaFulfilled + " / " + TimeOfDay.Instance.profitQuota;
            var s = TimeOfDay.Instance.daysUntilDeadline != 1 ? "s " : " ";
            _activity.State = TimeOfDay.Instance.daysUntilDeadline + " day" + s + "remaining";
            if (_currentPlanet != null)
            {
                _activity.Assets.SmallText = planets[_currentPlanet];
                _activity.Assets.SmallImage = _currentPlanet.ToLower();
            }
            else
            {
                _activity.Assets.SmallText = null;
                _activity.Assets.SmallImage = null;
            }
        }
        else
        {
            _activity.State = "in Main Menu";
            _activity.Details = null;
            _activity.Assets.SmallText = null;
            _activity.Assets.SmallImage = null;
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
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
            var currentScene = scene.name;
            if (currentScene is "MainMenu")
            {
                if (_inGame)
                {
                    _activity.Timestamps.Start = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                }
                _inGame = false;
            }
            else if (currentScene == "SampleSceneRelay")
            {
                if (!_inGame)
                {
                    _activity.Timestamps.Start = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                }
                _inGame = true;
            }
            if (planets.ContainsKey(currentScene))
                _currentPlanet = currentScene;
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        if (_currentPlanet == scene.name)
            _currentPlanet = null;
    }
    
}