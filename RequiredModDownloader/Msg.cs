using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequiredModDownloader
{
    public class Msg
    {
        public static string ApprovedModNeeded(string modName)
        {
            return $"This level requires {modName}, a plugin that you haven't installed yet!\nThis plugin was verified by the BeatMods community.\nWould you like to install it and reboot the game?";
        }

        public static string DevSupportedModNeeded(string modName)
        {
            return $"This level requires {modName}, a plugin that you haven't installed yet!\nThis mod was verified by the creator of RequiredModDownloader.\nWould you like to install it and reboot the game?";
        }

        public static string UnapprovedModNeeded()
        {
            return "WARNING\n\nThis level requires a custom plugin that you haven't installed yet!\nThis mod hasn't been verified, and could be malicious.\nWould you like to install it and reboot the game?";
        }

        public static string MultipleApprovedModsNeeded(string[] modNames, string verifier)
        {
            string modList = "";
            for (int i = modNames.Length - 1; i > 0; i--)
            {
                modList += $", {modNames[i]}";
            }
            modList.Substring(2);
            return $"This level requires {modNames.Length} plugins that you haven't installed yet!\nThese plugins were verified by {verifier}.\nWould you like to install them and reboot the game?\n\nMods Required: {modList}";
        }
    }
}
