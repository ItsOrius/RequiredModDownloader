# RequiredModInstaller
The purpose of this plugin is to download any mods that a level requires that isn't currently installed, but also to give mappers and modders more creative freedom with their levels!

For example, imagine someone tries to play ANALYS but they don't have Noodle Extensions. When they click on the level, a notification will appear telling the user they need Noodle Extensions to play it. Then there's an option for you to download the mod and reboot the game without having to switch to a PC! All approved plugins on BeatMods will automatically be capable of downloading within the game, if the level requires it.

But this plugin also allows for the creator of their map to require their own custom plugins. The download link takes the form of a GitHub link as shown below.
```dat
"_customData" : {
    "_requirements" : [
        "MyCoolModName",
        "MyCoolerModName",
        "CoolModDependency",
        "CoolerModDependency"
    ],
    "_customPlugins" : [
        "MyCoolUsername.MyCoolModRepository.MyCoolModFileName",
        "MyCoolerUsername.MyCoolerModRepository.MyCoolerModFileName"
    ]
}
```
The settings above would download ``https://github.com/MyCoolUsername/MyCoolModRepository/releases/latest/download/MyCoolModFileName.dll`` and ``https://github.com/MyCoolerUsername/MyCoolerModRepository/releases/latest/download/MyCoolerModFileName.dll``, as well as CoolModDependency and CoolerModDependency if they're available on BeatMods. Make sure to include your custom mod names in ``_requirements``, or they may play the level without them!

Basically, the plugins follow a ``(username).(repositoryName).(dllFileName)`` template. The reason for this is that a custom download link for the plugins could result in somebody using it maliciously, i.e. putting some form of malware on the PC. That system still doesn't protect you from viruses uploaded to GitHub, which is why ``.dll`` is automatically added to the end of ``dllFileName``. Even with all of this, you still aren't safe because someone could hijack the DLL file with some form of a virus (we all saw what happened with HitSoundPlus).

In order to remind people of the inherent risk, we will always notify them if an unverified plugin is needed to play the level. We also allow them to generate a .html file that includes give links to the sources of all the different required mods, whether it's BeatMods or GitHub. If you want your plugin to be verified, you must message ``Orius#0001`` with a link to the plugin's GitHub repository. The plugin will be tested manually on a Virtual Machine and a VPN for their safety. If a plugin is found to be malicious, the plugin can become blacklisted and undownloadable. Again, please message ``Orius#0001`` if you find a level with a malicious GitHub plugin.

Please keep in mind that if your custom mod needs dependencies (like nearly all mods) you'll have to include them in ``_requirements`` (with the exception of BSIPA), as we can't tell what dependencies you have linked in your mod. If your custom plugin relies on another custom plugin that isn't on BeatMods, include the proprietary GitHub link to it in ``_customPlugins``.

When giving your releases a "version," please use the format of ``v(Beat Saber Version).(Version Release Number)`` so that we can make sure players only download the plugin if the version is up-to-date. We also plan on adding the ability for players to be notified when a new version releases of your plugin.![image](https://user-images.githubusercontent.com/32719997/115302575-94177700-a130-11eb-8d9f-540e82c48fa4.png)
