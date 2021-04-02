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
using Newtonsoft.Json;

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

        private void OnLevelSelected(LevelCollectionViewController levelCollection, IPreviewBeatmapLevel level)
        {
            Log.Info($"Selected level: {level.songName}");
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
            String[] devSupportedMods;
            JObject specialPluginNames = JObject.Parse(new WebClient().DownloadString("https://raw.githubusercontent.com/ItsOrius/RequiredModInstaller/master/RequiredModDownloader/specialPluginNames.json"));

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
                            try { specialPluginName = specialPluginNames.SelectToken($"$.{nextDependency}").ToString(); }
                            catch (System.Exception e)
                            {
                                Log.Info("");
                            }
                            if (!pluginInstalled(nextDependency) && !totalModsNeeded.ToArray().Contains(nextDependency) && nextDependency != "BSIPA")
                            {
                                Log.Info($"Found required dependency {nextDependency}");
                                totalModsNeeded.Add(nextDependency);
                                verifiedModsNeeded.Add(nextDependency);
                            } else if (!pluginInstalled(specialPluginName) && !totalModsNeeded.ToArray().Contains(specialPluginName))
                            {
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
            Log.Info("Checking for custom plugins");

            JObject devSupportedModsOutput = JObject.Parse(new WebClient().DownloadString("https://raw.githubusercontent.com/ItsOrius/RequiredModInstaller/master/RequiredModDownloader/devSupportedMods.json"));
            List<string> devSupportedModsList = new List<string>();
            for (int i = 0; i < devSupportedModsOutput.Count; i++) devSupportedModsList.Add(devSupportedModsOutput.SelectToken($"$.devSupportedMods[{i}]").ToString());
            devSupportedMods = devSupportedModsList.ToArray();

            for (int i = 0; i < customMods.Length; i++)
            {
                String[] urlArgs = customMods[i].Split('.');
                if (!pluginInstalled($"{urlArgs[2]}.dll"))
                {
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

            // finally, send a notification if needed
            Log.Info("Attempting to send notification");
            if (totalModsNeeded.ToArray().Length < 1) return;
            if (totalModsNeeded.ToArray().Length > 1)
            {
                controller.mpnObject.SetActive(true);
                controller.mpnText.text = Msg.MultipleModsNeeded(totalModsNeeded.ToArray(), verifierText(beatmodsVerified, devVerified, unverified));
                WebsiteBuilder builder = new WebsiteBuilder();
                List<String> names = new List<string>();
                List<String> sources = new List<string>();
                if (verifiedModsNeeded.ToArray().Length < 1) { names.Add("Your mother!"); sources.Add("https://www.youtube.com/watch?v=ZDDAtkg-s-g"); }
                for (int i = 0; i < verifiedModsNeeded.ToArray().Length; i++)
                {
                    string beatModsOutput = new WebClient().DownloadString($"https://beatmods.com/api/v1/mod?status=approved&name={verifiedModsNeeded[i]}&gameVersion={gameVersion}");
                    beatModsOutput = beatModsOutput.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                    JObject beatModsJson = JObject.Parse(beatModsOutput);
                    names.Add(verifiedModsNeeded[i]);
                    sources.Add(beatModsJson.SelectToken("$.link").ToString());
                }
                names.Add("<h2>Community Mods</h2>");
                sources.Add("");
                if (communityModsNeeded.ToArray().Length < 1) { names.Add("Your mother!"); sources.Add("https://www.youtube.com/watch?v=ZDDAtkg-s-g"); }
                for (int i = 0; i < communityModsNeeded.ToArray().Length; i++)
                {
                        String[] urlArgs = communityModsNeeded[i].Split('.');
                        names.Add(communityModsNeeded[i].Split('.')[1]);
                        sources.Add($"https://github.com/{urlArgs[0]}/{urlArgs[1]}");
                }
                builder.CreateWebsite(names.ToArray(), sources.ToArray(), Path.Combine(IPA.Utilities.UnityGame.InstallPath, "UserData", "RequiredModInstaller", "requiredmodinstaller.html"));
                controller.sourceLink = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "UserData", "RequiredModInstaller", "requiredmodinstaller.html");
            } else {
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
                        string downloadLink = $"https://www.beatmods.com{beatModsJson.SelectToken("$.downloads.url").ToString()}";
                        new WebClient().DownloadFile(downloadLink, IPA.Utilities.UnityGame.InstallPath);
                        String[] fileNameArgs = beatModsJson.SelectToken("$.downloads.url").ToString().Split('/');
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
