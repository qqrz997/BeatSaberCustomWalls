using CustomWalls.Settings.UI;
using Zenject;

namespace CustomWalls.Installers;

public class MenuInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<MaterialsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
        Container.Bind<MaterialDetailsViewController>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<MaterialListViewController>().FromNewComponentAsViewController().AsSingle();
        Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
    }
}