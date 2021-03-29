using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RequiredModInstaller
{
    public class CustomViewController : BSMLResourceViewController
    {
        public string sourceLink;
        private bool inAction = false;
        public override string ResourceName => InstallSucceeded;

        // paths to different .bsml files
        public string SinglePluginNeeded => "Namespace.Views.CustomPluginNeeded.bsml";
        public string InstallFailed => "Namespace.Views.InstallFailed.bsml";
        public string InstallSucceeded => "Namespace.Views.InstallSucceeded.bsml";





        // Global Actions
        [UIAction("reboot")]
        private void Reboot()
        {
            if (inAction) return;
            Application.Quit();
        }





        // SinglePluginNeeded.bsml
        [UIObject("spn")]
        public GameObject spnObject;

        [UIComponent("spn-text")]
        public TextMeshProUGUI spnText;

        [UIAction("spn-view-source")]
        private void SpnViewSource()
        {
            if (sourceLink != "") Application.OpenURL(sourceLink);
        }

        [UIAction("spn-install")]
        private void SpnInstall()
        {
            if (inAction) return;
            spnText.text = $"\n\n{spnText.text}\n\nInstalling plugins...";
            Plugin.Instance.InstallCachedMods();
            inAction = true;
        }

        [UIAction("spn-change-active")]
        public void SpnChangeActive()
        {
            if (inAction) return;
            if (spnObject.activeSelf) spnObject.SetActive(false);
            else spnObject.SetActive(true);
        }





        // MultiplePluginsNeeded.bsml
        [UIObject("mpn")]
        public GameObject mpnObject;

        [UIComponent("mpn-text")]
        public TextMeshProUGUI mpnText;

        [UIAction("mpn-view-source")]
        private void MpnViewSource()
        {
            Application.OpenURL(sourceLink);
        }

        [UIAction("mpn-install")]
        private void MpnInstall()
        {
            if (inAction) return;
            spnText.text = $"\n\n{mpnText.text}\n\nInstalling plugins...";
            Plugin.Instance.InstallCachedMods();
            inAction = true;
        }

        [UIAction("mpn-change-active")]
        public void MpnChangeActive()
        {
            if (inAction) return;
            if (spnObject.activeSelf) spnObject.SetActive(false);
            else spnObject.SetActive(true);
        }





        // InstallFailed.bsml
        [UIObject("if")]
        public GameObject ifObject;

        [UIComponent("if-text")]
        public TextMeshProUGUI ifText;

        [UIAction("if-change-active")]
        public void IfChangeActive()
        {
            inAction = false;
            if (ifObject.activeSelf) ifObject.SetActive(false);
            else ifObject.SetActive(true);
        }





        // InstallSucceeded.bsml
        [UIObject("is")]
        public GameObject isObject;

        [UIComponent("is-text")]
        public TextMeshProUGUI isText;

        [UIAction("is-change-active")]
        public void IsChangeActive()
        {
            inAction = false;
            if (isObject.activeSelf) isObject.SetActive(false);
            else isObject.SetActive(true);
        }
    }
}
