using System;
using CustomWalls.Data;
using CustomWalls.Utilities;
using SiraUtil.Extras;
using SiraUtil.Objects.Beatmap;
using UnityEngine;
using Zenject;

namespace CustomWalls.Installers;

internal class PlayerInstaller : Installer
{
    private readonly PluginConfig pluginConfig;
    private readonly MaterialAssetLoader materialAssetLoader;
    private readonly GameplayCoreSceneSetupData gameplayCoreSceneSetupData;

    public PlayerInstaller(MaterialAssetLoader materialAssetLoader, GameplayCoreSceneSetupData gameplayCoreSceneSetupData, PluginConfig pluginConfig)
    {
        this.materialAssetLoader = materialAssetLoader;
        this.gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
        this.pluginConfig = pluginConfig;
    }

    public override void InstallBindings()
    {
        Container.BindInterfacesTo<SubmissionsManager>().AsSingle();
        Container.BindInstance(materialAssetLoader.GetSelectedMaterial()).AsSingle();
        
        var redecorator = new ObstacleRegistration(RedecorateObstacleController, 50);
        Container.RegisterRedecorator(redecorator);
    }

    private ObstacleController RedecorateObstacleController(ObstacleController obstacleController)
    {
        try
        {
            obstacleController._stretchableObstacle._obstacleFrame.enabled = pluginConfig.EnableObstacleFrame;

            var customMaterial = materialAssetLoader.GetSelectedMaterial();
            if (customMaterial.FileName == CustomMaterial.DefaultMaterial.FileName)
            {
                // Default wall selected
                return obstacleController;
            }
            
            // Start customizing wall
            obstacleController.name += " (CustomWalls)";

            var obstacleMaterialSetter = obstacleController.GetComponent<ObstacleMaterialSetter>();
            if (obstacleMaterialSetter == null)
            {
                throw new("ObstacleMaterialSetter not found");
            }

            var color = gameplayCoreSceneSetupData.colorScheme.obstaclesColor;
            var coreRenderer = obstacleMaterialSetter._obstacleCoreRenderer;

            if (customMaterial.Descriptor.Overlay)
            {
                var overlay = MeshUtils.CreateOverlay(
                    coreRenderer,
                    customMaterial.MaterialRenderer,
                    customMaterial.Descriptor.OverlayOffset);

                overlay.GetComponent<Renderer>().materials.SetColors(color);

                if (customMaterial.Descriptor.ReplaceMesh)
                {
                    overlay.GetComponent<MeshFilter>().ReplaceMesh(customMaterial.MaterialMeshFilter,
                        customMaterial.Descriptor.MeshScaleMultiplier);
                    if (!customMaterial.Descriptor.ReplaceOnlyOverlayMesh)
                    {
                        coreRenderer.GetComponent<MeshFilter>().ReplaceMesh(customMaterial.MaterialMeshFilter,
                            customMaterial.Descriptor.MeshScaleMultiplier);
                    }
                }
            }
            else
            {
                Plugin.Log.Notice("Replacing material");
                customMaterial.MaterialRenderer.materials.SetColors(color);
                var material = customMaterial.MaterialRenderer.material;
                obstacleMaterialSetter._hwCoreMaterial = material;
                obstacleMaterialSetter._lwCoreMaterial = material;
                obstacleMaterialSetter._texturedCoreMaterial = material;
                if (customMaterial.Descriptor.ReplaceMesh)
                {
                    Plugin.Log.Notice("Replacing mesh");
                    coreRenderer.GetComponent<MeshFilter>().ReplaceMesh(customMaterial.MaterialMeshFilter,
                        customMaterial.Descriptor.MeshScaleMultiplier);
                }
            }

            Plugin.Log.Notice("Wall Redecorated");
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"Problem encountered when redecorating walls\n{ex}");
        }
        
        return obstacleController;
    }
}