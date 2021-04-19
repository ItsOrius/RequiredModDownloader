using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using TMPro;
using UnityEngine;

namespace RequiredModInstaller
{
    public class MainViewController : BSMLResourceViewController
    {
        public string sourceLink;
        private bool inAction = false;
        [UIParams]
        BSMLParserParams parserParams = null;
        public override string ResourceName => InstallSucceeded;

        // paths to different .bsml files
        public string SinglePluginNeeded => "RequiredModInstaller.Views.SinglePluginNeeded.bsml";
        public string MultiplePluginsNeeded => "RequiredModInstaller.Views.MultiplePluginsNeeded.bsml";
        public string InstallFailed => "RequiredModInstaller.Views.InstallFailed.bsml";
        public string InstallSucceeded => "RequiredModInstaller.Views.InstallSucceeded.bsml";





        // Global Actions
        [UIAction("reboot")]
        public void Reboot()
        {
            if (inAction) return;
            Application.Quit();
        }

        public void ToggleMenuVisible(string menuId, bool isVisible)
        {
            switch(menuId.ToLower())
            {
                case "spn":
                    if (isVisible) { parserParams.EmitEvent("show-spn"); } else { parserParams.EmitEvent("hide-spn"); }
                    break;
                case "mpn":
                    if (isVisible) { parserParams.EmitEvent("show-mpn"); } else { parserParams.EmitEvent("hide-mpn"); }
                    break;
                case "if":
                    if (isVisible) { parserParams.EmitEvent("show-if"); } else { parserParams.EmitEvent("hide-if"); }
                    break;
                case "is":
                    if (isVisible) { parserParams.EmitEvent("show-is"); } else { parserParams.EmitEvent("hide-is"); }
                    break;
            }
         }

        [UIAction("view-source")]
        public void ViewSource()
        {
            if (!string.IsNullOrWhiteSpace(sourceLink)) { Application.OpenURL(sourceLink); }
        }

        [UIAction("install-plugins")]
        private void InstallPlugins()
        {
            if (inAction) return;
            spnText.text = $"\n\n{mpnText.text}\n\nInstalling plugins...";
            inAction = true;
            Plugin.Instance.InstallCachedMods();
        }





        // SinglePluginNeeded.bsml
        [UIComponent("spn-text")]
        public TextMeshProUGUI spnText;

        [UIAction("spn-close-menu")]
        public void SpnCloseMenu()
        {
            if (inAction) return;
            parserParams.EmitEvent("hide-spn");
        }





        // MultiplePluginsNeeded.bsml
        [UIComponent("mpn-text")]
        public TextMeshProUGUI mpnText;

        [UIAction("mpn-close-menu")]
        public void MpnCloseMenu()
        {
            if (inAction) return;
            parserParams.EmitEvent("hide-mpn");
        }





        // InstallFailed.bsml
        [UIComponent("if-text")]
        public TextMeshProUGUI ifText;

        [UIAction("if-close-menu")]
        public void IfCloseMenu()
        {
            inAction = false;
            parserParams.EmitEvent("hide-if");
        }





        // InstallSucceeded.bsml
        [UIComponent("is-text")]
        public TextMeshProUGUI isText;

        [UIAction("is-close-menu")]
        public void IsCloseMenu()
        {
            inAction = false;
            parserParams.EmitEvent("hide-is");
        }
    }
}
