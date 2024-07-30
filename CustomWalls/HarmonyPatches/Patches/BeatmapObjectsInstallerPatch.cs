using CustomWalls.Data;
using CustomWalls.Settings;
using CustomWalls.Utilities;
using HarmonyLib;
using System;
using UnityEngine;

namespace CustomWalls.HarmonyPatches.Patches
{
    [HarmonyPatch(typeof(BeatmapObjectsInstaller))]
    [HarmonyPatch(nameof(BeatmapObjectsInstaller.InstallBindings), MethodType.Normal)]
    internal class BeatmapObjectsInstallerPatch
    {
        private static ObstacleController originalObstacle;
        private static ObstacleController newObstacle;

        private static void Prefix(ref ObstacleController ____obstaclePrefab, GameplayCoreSceneSetupData ____sceneSetupData)
        {
            originalObstacle = ____obstaclePrefab;

            if (newObstacle) GameObject.Destroy(newObstacle);
            newObstacle = GameObject.Instantiate(originalObstacle);
            newObstacle.gameObject.SetActive(false);

            newObstacle._stretchableObstacle._obstacleFrame.enabled = Configuration.EnableObstacleFrame;

            CustomMaterial customMaterial = MaterialAssetLoader.CustomMaterialObjects[MaterialAssetLoader.SelectedMaterial];

            if (customMaterial.FileName != CustomMaterial.DefaultMaterial.FileName)
            {
                ObstacleMaterialSetter obstacleMaterialSetter = newObstacle.GetComponent<ObstacleMaterialSetter>();
                Renderer coreRenderer = obstacleMaterialSetter._obstacleCoreRenderer;
                Color color = ____sceneSetupData.colorScheme.obstaclesColor;

                try
                {
                    if (customMaterial.Descriptor.Overlay)
                    {
                        GameObject overlay = MeshUtils.CreateOverlay(coreRenderer, customMaterial.MaterialRenderer, customMaterial.Descriptor.OverlayOffset);
                        MaterialUtils.SetMaterialsColor(overlay?.GetComponent<Renderer>().materials, color);
                        if (customMaterial.Descriptor.ReplaceMesh)
                        {
                            MeshUtils.ReplaceMesh(overlay.GetComponent<MeshFilter>(), customMaterial.MaterialMeshFilter, customMaterial.Descriptor.MeshScaleMultiplier);
                            if (!customMaterial.Descriptor.ReplaceOnlyOverlayMesh)
                            {
                                MeshUtils.ReplaceMesh(coreRenderer.GetComponent<MeshFilter>(), customMaterial.MaterialMeshFilter, customMaterial.Descriptor.MeshScaleMultiplier);
                            }
                        }
                    }
                    else
                    {
                        MaterialUtils.SetMaterialsColor(customMaterial.MaterialRenderer.materials, color);
                        Material material = customMaterial.MaterialRenderer.material;
                        obstacleMaterialSetter._hwCoreMaterial = material;
                        obstacleMaterialSetter._lwCoreMaterial = material;
                        obstacleMaterialSetter._texturedCoreMaterial = material;
                        if (customMaterial.Descriptor.ReplaceMesh)  
                        {
                            MeshUtils.ReplaceMesh(coreRenderer.GetComponent<MeshFilter>(), customMaterial.MaterialMeshFilter, customMaterial.Descriptor.MeshScaleMultiplier);
                        }
                    }
                    ____obstaclePrefab = newObstacle;
                }
                catch (Exception ex)
                {
                    Logger.log.Error($"Unable to apply wall\n{ex}");
                    ____obstaclePrefab = originalObstacle;
                }
            }
        }

        private static void Postfix(ref ObstacleController ____obstaclePrefab)
        {
            ____obstaclePrefab = originalObstacle;
        }
    }
}
