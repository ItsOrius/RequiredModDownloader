# RequiredModDownloader
The purpose of this plugin is to download any mods that a level requires that isn't currently installed, but also to give mappers and modders more creative freedom with their levels!

For example, imagine someone tries to play ANALYS but they don't have Noodle Extensions. When they click on the level, a notification will appear telling the user they need Noodle Extensions to play it. Then there's an option for you to download the mod and reboot the game without having to switch to a PC! All approved plugins on BeatMods will automatically be capable of downloading within the game, if the level requires it.

But this plugin also allows for the creator of their map to require their own custom plugin. For security reasons, the download link takes the form of a GitHub link as shown below. This could result in insane modcharts we hadn't even imagined possible.
```dat
"_customData" : {
    "_requirements" : [
        "MyCoolMod"
    ],
    "_customPlugin" : [
        "_username": "CoolDude123",
        "_repository": "MyCoolModRepo",
        "_fileName": "MyCoolMod"
    ]
}
```
The settings above would download ``https://github.com/CoolDude123/MyCoolModRepo/releases/latest/download/MyCoolMod.dll`` 

Make sure that ``_fileName`` doesn't include ``.dll`` at the end, or it would result in downloading the file ``MyCoolMod.dll.dll``! This is here to stop someone from putting a download link to something more malicious, such as a .exe file.

The plugin is still in development and hasn't been released yet, but hopefully you'll stick with us through development!
