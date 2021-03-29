using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IPA;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using BS_Utils;

namespace RequiredModInstaller
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        string gameVersion = "1.13.4";
        List<string> verifiedModsCache = new List<string>();
        List<string> communityModsCache = new List<string>();
        CustomViewController controller;

        [Init]
        public void Init(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            Log.Info("RequiredModInstaller initialized.");
            controller = new CustomViewController();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
            new GameObject("RequiredModInstallerController").AddComponent<RequiredModInstallerController>();
            BS_Utils.Utilities.BSEvents.levelSelected += OnLevelSelected;
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
            BS_Utils.Utilities.BSEvents.levelSelected -= OnLevelSelected;
        }

        private void OnLevelSelected(LevelCollectionViewController levelCollection, IPreviewBeatmapLevel arg)
        {
            CustomJSONData.CustomLevelInfo.CustomLevelInfoSaveData beatmap = (CustomJSONData.CustomLevelInfo.CustomLevelInfoSaveData)arg;
            JObject CustomData = JObject.Parse(beatmap.customData);
            Log.Info($"CustomData variable is equal to...\n\n{CustomData.ToString()}");
            int requiredModsCount = CustomData["_requirements"].Count();
            int customModsCount = CustomData["_customPlugins"].Count();
            List<string> requiredMods = new List<string>();
            List<string> customMods = new List<string>();
            for (int i = 0; i < requiredModsCount; i++) requiredMods.Add(CustomData[$"_requirements[i]"].ToString());
            for (int i = 0; i < customModsCount; i++) customMods.Add(CustomData[$"_customPlugins[i]"].ToString());
            CheckCustomMods(requiredMods.ToArray(), customMods.ToArray());
        }

        public void CheckCustomMods(String[] requiredMods, String[] customMods)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return;

            List<string> totalModsNeeded = new List<string>();
            List<string> verifiedModsSource = new List<string>();
            List<string> verifiedModsNeeded = new List<string>();
            List<string> communityModsNeeded = new List<string>();
            String[] devSupportedMods = new WebClient().DownloadString("https://raw.githubusercontent.com/ItsOrius/RequiredModInstaller/master/RequiredModDownloader/devSupportedMods.txt").Split('/');

            bool beatmodsVerified = false;
            bool devVerified = false;
            bool unverified = false;

            // check for any required BeatMods plugins that arent installed
            for (int i = 0; i < requiredMods.Length; i++)
            {
                requiredMods[i].Replace(" ", "");
                string beatModsOutput = new WebClient().DownloadString($"https://beatmods.com/api/v1/mod?status=approved&name={requiredMods[i]}&gameVersion={gameVersion}");

                if (beatModsOutput != "[]" && !pluginInstalled(requiredMods[i]))
                {
                    beatmodsVerified = true;
                    totalModsNeeded.Add(requiredMods[i]);
                    verifiedModsNeeded.Add(requiredMods[i]);
                    JObject beatModsJson = JObject.Parse(beatModsOutput);
                    int dependencyCheck = beatModsJson["dependencies"].Count();
                    for (int j = 0; i < dependencyCheck; i++)
                    {
                        string nextDependency = beatModsJson.SelectToken($"dependencies[{j}].name").ToString();
                        if (!pluginInstalled(nextDependency) && !totalModsNeeded.ToArray().Contains(nextDependency) || nextDependency != "BSIPA")
                        {
                            totalModsNeeded.Add(nextDependency);
                            verifiedModsNeeded.Add(nextDependency);
                        }
                    }
                }
            }

            // check for custom plugins and if its dev supported or installed
            if (customMods.Length > 0)
            {
                for (int i = 0; i < customMods.Length; i++)
                {
                    if (!pluginInstalled(customMods[i]))
                    {
                        totalModsNeeded.Add(requiredMods[i]);
                        communityModsNeeded.Add(customMods[i]);
                        if (devSupportedMods.Contains(customMods[i])) {
                            devVerified = true;
                        } else {
                            unverified = true;
                        }
                    }
                }
            }

            // finally, send a notification if needed
            if (totalModsNeeded.ToArray().Length < 1) return;
            controller = new CustomViewController();
            if (totalModsNeeded.ToArray().Length > 1)
            {
                controller.mpnObject.SetActive(true);
                controller.mpnText.text = Msg.MultipleModsNeeded(totalModsNeeded.ToArray(), verifierText(beatmodsVerified, devVerified, unverified));
                WebsiteBuilder builder = new WebsiteBuilder();
                if (beatmodsVerified && !devVerified && !unverified)
                {
                    controller.sourceLink = "https://www.beatmods.com";
                } else
                {
                    List<String> names = new List<string>();
                    List<String> sources = new List<string>();
                    for (int i = 0; i < verifiedModsNeeded.ToArray().Length; i++)
                    {
                        string beatModsOutput = new WebClient().DownloadString($"https://beatmods.com/api/v1/mod?status=approved&name={verifiedModsNeeded[i]}&gameVersion={gameVersion}");
                        var beatModsJson = JObject.Parse(beatModsOutput);
                        names.Add(verifiedModsNeeded[i]);
                        sources.Add(beatModsJson["link"].ToString());
                    }
                    names.Add("<h2>Community Mods</h2>");
                    sources.Add("");
                    for (int i = 0; i < communityModsNeeded.ToArray().Length; i++)
                    {
                        String[] urlArgs = communityModsNeeded[i].Split('.');
                        names.Add(communityModsNeeded[i].Split('.')[1]);
                        sources.Add($"https://github.com/{urlArgs[0]}/{urlArgs[1]}");
                    }
                    builder.CreateWebsite(names.ToArray(), sources.ToArray(), Path.Combine(IPA.Utilities.UnityGame.InstallPath, "UserData", "RequiredModInstaller", "requiredmodinstaller.html"));
                    controller.sourceLink = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "UserData", "RequiredModInstaller", "requiredmodinstaller.html");
                }
            }
            else {
                controller.spnObject.SetActive(true);
                if (beatmodsVerified) {
                    string beatModsOutput = new WebClient().DownloadString($"https://beatmods.com/api/v1/mod?status=approved&name={verifiedModsNeeded[0]}&gameVersion={gameVersion}");
                    var beatModsJson = JObject.Parse(beatModsOutput);
                    controller.sourceLink = beatModsJson["link"].ToString();
                    controller.spnText.text = Msg.SingleModNeeded(totalModsNeeded.ToArray()[0], "This mod was verified by the BeatMods community.");
                } else {
                    String[] urlArgs = customMods[0].Split('.');
                    controller.sourceLink = $"https://github.com/{urlArgs[0]}/{urlArgs[1]}";
                    if (devVerified) {
                        controller.spnText.text = Msg.SingleModNeeded(urlArgs[1], "This mod was verified by the creator of RequiredModInstaller.");
                    } else
                    {
                        controller.spnText.text = Msg.SingleModNeeded(urlArgs[1], "This mod is unverified and could be malicious.");
                    }
                }
            }

            verifiedModsCache = verifiedModsNeeded;
            communityModsCache = communityModsNeeded;
        }

        public void InstallCachedMods()
        {
            DownloadMods(verifiedModsCache.ToArray(), communityModsCache.ToArray());
        }

        public void DownloadMods(String[] verifiedMods, String[] communityMods)
        {
            int s = 0;
            int f = 0;
            string sp = "";
            string fp = "";

            // attempt to download all verified plugins
            if (verifiedMods.Length > 0)
            {
                for (int i = 0; i < verifiedMods.Length; i++)
                {
                    try
                    {
                        string beatModsOutput = new WebClient().DownloadString($"https://beatmods.com/api/v1/mod?status=approved&name={verifiedMods[i]}&gameVersion={gameVersion}");
                        var beatModsJson = JObject.Parse(beatModsOutput);
                        string downloadLink = $"https://www.beatmods.com{beatModsJson["downloads"]["url"].ToString()}";
                        new WebClient().DownloadFile(downloadLink, IPA.Utilities.UnityGame.InstallPath);
                        String[] fileNameArgs = beatModsJson["downloads"]["url"].ToString().Split('/');
                        String fileName = fileNameArgs[fileNameArgs.Length - 1];
                        var zipFileLocation = Path.Combine(IPA.Utilities.UnityGame.InstallPath, fileName);
                        ZipFile.ExtractToDirectory(zipFileLocation, IPA.Utilities.UnityGame.InstallPath);
                        File.Delete(zipFileLocation);
                        s++;
                    } catch (System.Exception e)
                    {
                        Log.Log(IPALogger.Level.Info, $"Failed to download verified plugin: {e}");
                        f++;
                    }
                }
            }

            // attempt to download all github plugins
            if (communityMods.Length > 0)
            {
                for (int i = 0; i < communityMods.Length; i++)
                {
                    String[] urlArgs = communityMods[i].Split('.');
                    try
                    {
                        new WebClient().DownloadFile($"https://github.com/{urlArgs[0]}/{urlArgs[1]}/releases/latest/download/{urlArgs[2]}.dll", Path.Combine(IPA.Utilities.UnityGame.InstallPath, "Plugins"));
                        s++;
                    }
                    catch (System.Exception e)
                    {
                        Log.Log(IPALogger.Level.Info, $"Failed to download custom plugin: {e}");
                        f++;
                    }
                }
            }

            // installsucceeded or installfailed screen
            controller.spnObject.SetActive(false);
            controller.mpnObject.SetActive(false);
            if (s != 1) sp = "s";
            if (f != 1) fp = "s";
            if (s > 0)
            {
                controller.isObject.SetActive(true);
                controller.isText.text = $"{s} mod{sp} successfully installed, {f} mod{fp} failed to install.";
            } else
            {
                controller.ifObject.SetActive(true);
                controller.ifText.text = $"Mod{fp} failed to install.";
            }
        }

        public bool pluginInstalled(string fileName)
        {
            bool isInstalled = false;
            if (File.Exists(Path.Combine(IPA.Utilities.UnityGame.InstallPath, "Plugins", $"{fileName}.dll"))) { isInstalled = true; }
            return isInstalled;
        }

        public string verifierText(bool a, bool b, bool c)
        {
            if (a && !b && !c)
            {
                return "These mods were verified by the BeatMods community.";
            } else if (a && b && !c || !a && b && !c)
            {
                return "These mods were verified by the creator of RequiredModInstaller.";
            } else if (a && b && c || !a && b && c || a && !b && c)
            {
                return "Some of these mods aren't verified, and could be malicious.";
            }
            return "All of these mods aren't verified, and could be malicious.";
        }
    }
}
