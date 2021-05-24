using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequiredModInstaller
{
    public class Msg
    {
        public static string SingleModNeeded(string mod, string verifiedBy)
        {
            return $"This level requires {mod}, a plugin that you haven't installed yet!\n{verifiedBy}\nWould you like to install it and reboot the game?";
        }

        public static string MultipleModsNeeded(string[] mods, string verifiedBy)
        {
            string modList = "";
            for (int i = mods.Length; i >= 0; i--) modList += $", {mods[i]}";
            return $"This level requires {mods.Length} plugins that you haven't installed yet!\n{verifiedBy}\nWould you like to install them and reboot the game?\n\nRequired Mods: {modList.Substring(2)}";
        }
    }
}
