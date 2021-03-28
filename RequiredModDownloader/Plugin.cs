using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using UnityEngine.SceneManagement;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using System.Net;

namespace RequiredModDownloader
{
    // Copyright Orius 2021
    // This code is super unfinished please don't murder me yet
    // But still I hope yall enjoy what you see
    // It's 11PM and I'm tired aaaaaaaah

    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        string gameVersion = "1.13.4";

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            Log.Info("RequiredModDownloader initialized.");
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
            new GameObject("RequiredModDownloaderController").AddComponent<RequiredModDownloaderController>();

        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");

        }

        public void CheckCustomMods(string input)
        {
            // imagine input = "Noodle Extensions|Chroma"
            String[] requiredMods = input.Split('|');
            // make this get "_customPlugin" and it's three variables
            String[] modArgs = new string[] {"CoolPerson123", "MyCoolRepo", "MyCoolMod"};
            List<string> modsNeeded = new List<string>;
            String customMod = $"https://github.com/{modArgs[1]}/{modArgs[2]}/releases/latest/download/{modArgs[3]}.dll";

            bool beatmodsVerified = false;
            bool devVerified = false;
            bool unverified = false;

            // check for any required BeatMods plugins that arent installed
            for (int i = requiredMods.Length; i > 0; i--)
            {
                // make this check if the mod is installed
                bool modInstalled = true;
                string beatModsOutput = new WebClient().DownloadString($"https://beatmods.com/api/v1/mod?status=approved&name={requiredMods[i]}&gameVersion={gameVersion}");

                if (beatModsOutput != "[]" && !modInstalled)
                {
                    Log.Log(IPALogger.Level.Debug, Msg.ApprovedModNeeded(requiredMods[i]));
                    beatmodsVerified = true;
                    modsNeeded.Add(requiredMods[i]);
                }
            }
            
            // check for a custom plugin and if its dev supported
            if (modArgs != null)
            {

            }
        }

        public void DownloadCustomMod(string url)
        {

        }
    }
}
