using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BS_Utils.Utilities;
using System;

namespace CustomWalls.Settings.UI
{
    internal class SettingsUI
    {
        private static readonly MenuButton menuButton = new MenuButton("Custom Walls", "Change Custom Walls Here!", MaterialsMenuButtonPressed, true);

        public static MaterialsFlowCoordinator materialsFlowCoordinator;

        public static void CreateMenu()
        {
            BSEvents.lateMenuSceneLoadedFresh += RegisterButtonOnMenuSceneLoaded;
        }

        public static void RemoveMenu()
        {
            MenuButtons.Instance.UnregisterButton(menuButton);
        }

        public static void ShowMaterialsFlow()
        {
            if (materialsFlowCoordinator == null)
            {
                materialsFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<MaterialsFlowCoordinator>();
            }

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(materialsFlowCoordinator);
        }

        private static void RegisterButtonOnMenuSceneLoaded(ScenesTransitionSetupDataSO s)
        {
            MenuButtons.Instance.RegisterButton(menuButton);
        }

        private static void MaterialsMenuButtonPressed() => ShowMaterialsFlow();
    }
}
