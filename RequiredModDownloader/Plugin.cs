using System;
using System.Collections.Generic;
using System.Linq;
using IPA;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using CustomJSONData.CustomLevelInfo;

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
        MainViewController controller;

        [Init]
        public void Init(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            Log.Info("RequiredModInstaller initialized.");
            controller = new MainViewController();
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

        private void OnLevelSelected(LevelCollectionViewController levelCollection, IPreviewBeatmapLevel level)
        {
            List<string> requiredMods = new List<string>();
            List<string> customMods = new List<string>();
            if (level is CustomPreviewBeatmapLevel customLevel)
            {
                var saveData = customLevel.standardLevelInfoSaveData as CustomLevelInfoSaveData;
                foreach (CustomLevelInfoSaveData.DifficultyBeatmapSet difficulties in saveData.difficultyBeatmapSets)
                {
                    foreach (CustomLevelInfoSaveData.DifficultyBeatmap difficulty in difficulties.difficultyBeatmaps)
                    {
                        List<object> requirements = CustomJSONData.Trees.at(difficulty.customData, "_requirements");
                        if (requirements != null && requirements.Count > 0)
                        {
                            foreach (object i in requirements)
                            {
                                requiredMods.Add(i.ToString());
                            }
                        }
                        List<object> customPlugins = CustomJSONData.Trees.at(difficulty.customData, "_customPlugins");
                        if (customPlugins != null && customPlugins.Count > 0)
                        {
                            foreach (object i in customPlugins)
                            {
                                customMods.Add(i.ToString());
                            }
                        }
                    }
                }
                CheckCustomMods(requiredMods.ToArray(), customMods.ToArray());
            }
        }

        public void CheckCustomMods(String[] requiredMods, String[] customMods)
        {
            if (requiredMods.Length < 1 || customMods.Length < 1 || Application.internetReachability == NetworkReachability.NotReachable) return;
            
            List<string> totalModsNeeded = new List<string>();
            List<string> verifiedModsNeeded = new List<string>();
            List<string> communityModsNeeded = new List<string>();
            JObject specialPluginNames = JObject.Parse(new WebClient().DownloadString("https://raw.githubusercontent.com/ItsOrius/RequiredModInstaller/master/RequiredModDownloader/specialPluginNames.json"));
            JObject devSupportedModsOutput = JObject.Parse(new WebClient().DownloadString("https://raw.githubusercontent.com/ItsOrius/RequiredModInstaller/master/RequiredModDownloader/devSupportedMods.json"));
            JObject blacklistedModsOutput = JObject.Parse(new WebClient().DownloadString("https://raw.githubusercontent.com/ItsOrius/RequiredModInstaller/master/RequiredModDownloader/BlacklistedMods.json"));

            bool beatmodsVerified = false;
            bool devVerified = false;
            bool unverified = false;

            // check for any required BeatMods plugins that arent installed
            for (int i = 0; i < requiredMods.Length; i++)
            {
                string editedName = requiredMods[i].Replace(" ", "");
                string beatModsOutput = new WebClient().DownloadString($"https://beatmods.com/api/v1/mod?status=approved&name={editedName}&gameVersion={gameVersion}");

                if (beatModsOutput != "[]" && !pluginInstalled(editedName))
                {
                    Log.Info($"Found required BeatMods mod {editedName}");
                    beatmodsVerified = true;
                    totalModsNeeded.Add(editedName);
                    verifiedModsNeeded.Add(editedName);
                    try
                    {
                        JObject beatModsJson;
                        beatModsOutput = beatModsOutput.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                        beatModsJson = JObject.Parse(beatModsOutput);
                        for (int j = 0; j < beatModsJson.SelectToken("$.dependencies").Count(); j++)
                        {
                            string nextDependency = beatModsJson.SelectToken($"$.dependencies[{j}].name").ToString();
                            string specialPluginName = "";
                            bool passedCheck = true;
                            try { specialPluginName = specialPluginNames.SelectToken($"$.{nextDependency}").ToString(); }
                            catch (System.Exception e) { }
                            if (pluginInstalled(nextDependency) && specialPluginName == "") { passedCheck = false; }
                            else if (totalModsNeeded.ToArray().Contains(nextDependency)) { passedCheck = false; }
                            else if (nextDependency == "BSIPA") { passedCheck = false; }
                            else if (pluginInstalled(specialPluginName) && specialPluginName != "") passedCheck = false;
                            if (passedCheck) {
                                Log.Info($"Found required dependency {nextDependency}");
                                totalModsNeeded.Add(nextDependency);
                                verifiedModsNeeded.Add(nextDependency);
                            }
                        }
                    } catch (System.Exception e)
                    {
                        Log.Info($"Failed to get dependencies: {e}");
                        return;
                    }
                }
            }

            // check for custom plugins and if its dev supported or installed
            List<string> devSupportedMods = new List<string>();
            for (int i = 0; i < devSupportedModsOutput.Count; i++) devSupportedMods.Add(devSupportedModsOutput.SelectToken($"$.devSupportedMods[{i}]").ToString());
            List<string> blacklistedMods = new List<string>();
            for (int i = 0; i < blacklistedModsOutput.Count; i++) blacklistedMods.Add(blacklistedModsOutput.SelectToken($"$.blacklistedMods[{i}]").ToString());

            for (int i = 0; i < customMods.Length; i++)
            {
                String[] urlArgs = customMods[i].Split('.');
                if (blacklistedMods.Contains(customMods[i])) {
                    Log.Info($"Level requires custom mod {customMods[i]} but it's blacklisted!");
                } else if (!pluginInstalled($"{urlArgs[2]}.dll")) {
                    Log.Info($"Found required custom mod {customMods[i]}");
                    totalModsNeeded.Add(customMods[i]);
                    communityModsNeeded.Add(customMods[i]);
                    if (devSupportedMods.Contains(customMods[i])) {
                        devVerified = true;
                    } else {
                        unverified = true;
                    }
                }
            }

            //
            //
            // REMOVE THIS BEFORE RELEASE
            //
            //
            DownloadMods(verifiedModsNeeded.ToArray(), communityModsNeeded.ToArray());
            Log.Info("Downloading the mods automatically! REMOVE THIS BEFORE RELEASE!");

            // finally, send a notification if needed
            Log.Info($"Total mods needed: {totalModsNeeded.Count}");
            if (totalModsNeeded.Count < 1) return;
            if (totalModsNeeded.Count > 1)
            {
                Log.Info("Attempting to send MPN notification");
                controller.mpnObject.SetActive(true);
                Log.Info("line 175");
                controller.mpnText.text = Msg.MultipleModsNeeded(totalModsNeeded.ToArray(), verifierText(beatmodsVerified, devVerified, unverified));
                Log.Info("line 177");
                WebsiteBuilder builder = new WebsiteBuilder();
                List<String> verifiedNames = new List<string>();
                List<String> verifiedSources = new List<string>();
                List<String> customNames = new List<string>();
                List<String> customSources = new List<string>();

                if (verifiedModsNeeded.Count < 1) { verifiedNames.Add("Your mother!"); verifiedSources.Add("https://www.youtube.com/watch?v=ZDDAtkg-s-g"); }
                Log.Info("Attempting to start verified mods loop");
                for (int i = 0; i < verifiedModsNeeded.Count; i++)
                {
                    string beatModsOutput = new WebClient().DownloadString($"https://beatmods.com/api/v1/mod?status=approved&name={verifiedModsNeeded[i]}&gameVersion={gameVersion}");
                    beatModsOutput = beatModsOutput.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                    JObject beatModsJson = JObject.Parse(beatModsOutput);
                    verifiedNames.Add(verifiedModsNeeded[i]);
                    verifiedSources.Add(beatModsJson.SelectToken("$.link").ToString());
                }
                Log.Info("Attempting to start community mods loop");
                if (communityModsNeeded.Count < 1) { customNames.Add("Your mother!"); customSources.Add("https://www.youtube.com/watch?v=ZDDAtkg-s-g"); }
                for (int i = 0; i < communityModsNeeded.Count; i++)
                {
                    String[] urlArgs = communityModsNeeded[i].Split('.');
                    customNames.Add(urlArgs[1]);
                    customSources.Add($"https://github.com/{urlArgs[0]}/{urlArgs[1]}");
                }
                Log.Info("Attempting create website and set source link");
                builder.CreateWebsite(verifiedNames.ToArray(), verifiedSources.ToArray(), customNames.ToArray(), customSources.ToArray(), Path.Combine(IPA.Utilities.UnityGame.InstallPath, "UserData", "RequiredModInstaller", "requiredmodinstaller.html"));
                controller.sourceLink = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "UserData", "RequiredModInstaller", "requiredmodinstaller.html");
            } else {
                Log.Info("Attempting to send SPN notification");
                controller.spnObject.SetActive(true);
                if (beatmodsVerified) {
                    string beatModsOutput = new WebClient().DownloadString($"https://beatmods.com/api/v1/mod?status=approved&name={verifiedModsNeeded[0]}&gameVersion={gameVersion}");
                    beatModsOutput = beatModsOutput.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                    JObject beatModsJson = JObject.Parse(beatModsOutput);
                    controller.sourceLink = beatModsJson.SelectToken("$.link").ToString();
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
                        beatModsOutput = beatModsOutput.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                        JObject beatModsJson = JObject.Parse(beatModsOutput);
                        string downloadLink = $"https://www.beatmods.com{beatModsJson.SelectToken("$.downloads[0].url").ToString()}";
                        String[] fileNameArgs = beatModsJson.SelectToken("$.downloads[0].url").ToString().Split('/');
                        String fileName = fileNameArgs[fileNameArgs.Length - 1];
                        Log.Info($"Attempting to download {verifiedMods[i]} from {downloadLink}");
                        new WebClient().DownloadFile(downloadLink, Path.Combine(IPA.Utilities.UnityGame.InstallPath, fileName));
                        string zipFileLocation = Path.Combine(IPA.Utilities.UnityGame.InstallPath, fileName);
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
                        new WebClient().DownloadFile($"https://github.com/{urlArgs[0]}/{urlArgs[1]}/releases/latest/download/{urlArgs[2]}.dll", Path.Combine(IPA.Utilities.UnityGame.InstallPath, "Plugins", $"{urlArgs[2]}.dll"));
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
            if (s != 1) sp = "s";
            if (f != 1) fp = "s";
            Log.Info($"{s} mod{sp} successfully installed, {f} mod{fp} failed to install.");
            controller.spnObject.SetActive(false);
            controller.mpnObject.SetActive(false);
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
