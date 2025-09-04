using System;
using BeatSaberMarkupLanguage.MenuButtons;
using Zenject;

namespace CustomWalls.Settings.UI;

internal class MenuButtonManager : IInitializable, IDisposable
{
    private readonly MenuButtons menuButtons;
    private readonly MainFlowCoordinator mainFlowCoordinator;
    private readonly MaterialsFlowCoordinator materialsFlowCoordinator;
    private readonly MenuButton menuButton;

    public MenuButtonManager(
        MenuButtons menuButtons,
        MainFlowCoordinator mainFlowCoordinator,
        MaterialsFlowCoordinator materialsFlowCoordinator)
    {
        this.menuButtons = menuButtons;
        this.mainFlowCoordinator = mainFlowCoordinator;
        this.materialsFlowCoordinator = materialsFlowCoordinator;
        menuButton = new("Custom Walls", PresentFlowCoordinator);
    }
    
    public void Initialize()
    {
        menuButtons.RegisterButton(menuButton);
    }

    public void Dispose()
    {
        menuButtons.UnregisterButton(menuButton);
    }

    private void PresentFlowCoordinator()
    {
        mainFlowCoordinator.PresentFlowCoordinator(materialsFlowCoordinator);
    }
}