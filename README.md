# RequiredModInstaller
The purpose of this plugin is to download any mods that a level requires that isn't currently installed, but also to give mappers and modders more creative freedom with their levels!

For example, imagine someone tries to play ANALYS but they don't have Noodle Extensions. When they click on the level, a notification will appear telling the user they need Noodle Extensions to play it. Then there's an option for you to download the mod and reboot the game without having to switch to a PC! All approved plugins on BeatMods will automatically be capable of downloading within the game, if the level requires it.

But this plugin also allows for the creator of their map to require their own custom plugins. The download link takes the form of a GitHub link as shown below, and this could result in insane modcharts we hadn't even imagined possible.
```dat
"_customData" : {
    "_requirements" : [
        "MyCoolMod"
    ],
    "_customPlugins" : [
        "MyCoolUsername.MyCoolModRepository.MyCoolModFile",
        "MyCoolerUsername.MyCoolerModRepository.MyCoolerModFile"
    ]
}
```
The settings above would download ``https://github.com/MyCoolUsername/MyCoolModRepository/releases/latest/download/MyCoolModFile.dll`` and ``https://github.com/MyCoolerUsername/MyCoolerModRepository/releases/latest/download/MyCoolerModFile.dll``.

In normal terms, the plugins follow a ``(username).(repositoryName).(dllFileName)`` template. The reason for this is that a custom download link for the plugins could result in somebody using it maliciously, i.e. putting some form of malware on the PC. That system still doesn't protect you from viruses uploaded to GitHub, which is why ".dll" is automatically added to the end of ``dllFileName``. Even with all of this, you still aren't safe because someone could hijack the DLL file with some form of a virus. However, at that point it would just be uploading malware to GitHub which is likely to get the attacker in trouble.

The plugin is still in development and hasn't been publicly released yet, but hopefully you'll stick with us through it all!
(P.S. This plugin may be delayed by the fact that I want to make a level showing the possibilities that comes with the plugin. Thanks for your patience!)
