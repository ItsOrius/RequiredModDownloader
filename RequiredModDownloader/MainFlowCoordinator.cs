using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BeatSaberMarkupLanguage;

namespace RequiredModInstaller
{
    class MainFlowCoordinator : FlowCoordinator
    {
        //private MainPageViewController mainPageView;
        //private CreationPageViewController creationPageView;
        //private ProfilePageViewController profilePageView;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("RequiredModInstaller");
                showBackButton = true;
                // LevelSelectionFlowCoordinator.State does what?
            }
        }
        
        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this, null);
        }
    }
}
