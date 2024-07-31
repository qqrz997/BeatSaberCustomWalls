using AssetBundleLoadingTools.Utilities;
using AssetBundleLoadingTools.Models.Shaders;
using CustomWalls.Data.CustomMaterialExtensions;
using CustomWalls.Utilities;
using System;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;

namespace CustomWalls.Data
{
    public class CustomMaterial
    {
        public string FileName { get; }
        public AssetBundle AssetBundle { get; }
        public MaterialDescriptor Descriptor { get; }
        public GameObject GameObject { get; }
        public Renderer MaterialRenderer { get; }
        public MeshFilter MaterialMeshFilter { get; }
        public string ErrorMessage { get; }

        private CustomMaterial(string fileName, AssetBundle assetBundle, MaterialDescriptor descriptor, GameObject gameObject, Renderer materialRenderer, MeshFilter materialMeshFilter, string errorMessage)
        {
            FileName = fileName ?? "Unknown";
            AssetBundle = assetBundle;
            Descriptor = descriptor;
            GameObject = gameObject;
            MaterialRenderer = materialRenderer;
            MaterialMeshFilter = materialMeshFilter;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        public static CustomMaterial DefaultMaterial =>
            new CustomMaterial("DefaultMaterials", null, new MaterialDescriptor
            {
                MaterialName = "Default",
                AuthorName = "Beat Saber",
                Description = "This is the default walls. (No preview available)",
                Icon = Utils.GetDefaultIcon()
            }, null, null, null, null);

        public static async Task<CustomMaterial> CreateAsync(string fileName)
        {
            if (fileName == "DefaultMaterials")
            {
                return DefaultMaterial;
            }

            try
            {
                AssetBundle assetBundle = await AssetBundleExtensions.LoadFromFileAsync(Path.Combine(Plugin.PluginAssetPath, fileName));
                GameObject gameObject = await AssetBundleExtensions.LoadAssetAsync<GameObject>(assetBundle, "Assets/_CustomMaterial.prefab");
                gameObject.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                await RepairObjectShaders(gameObject);

                MaterialDescriptor descriptor = gameObject.GetComponent<MaterialDescriptor>();
                Renderer materialRenderer = MaterialUtils.GetGameObjectRenderer(gameObject, "pixie");
                MeshFilter materialMeshFilter = MeshUtils.GetGameObjectMeshFilter(gameObject, "pixie");
                return new CustomMaterial(fileName, assetBundle, descriptor, gameObject, materialRenderer, materialMeshFilter, null);
            }
            catch (Exception ex)
            {
                Logger.log.Warn($"Something went wrong getting the AssetBundle for '{fileName}'!");
                Logger.log.Warn(ex);

                MaterialDescriptor descriptor = new MaterialDescriptor()
                {
                    MaterialName = "Invalid Wall (Delete it!)",
                    AuthorName = fileName,
                    Icon = Utils.GetErrorIcon()
                };

                string errorMessage = $"File: '{fileName}'" +
                                "\n\nThis file failed to load." +
                                "\n\nThis may have been caused by having duplicated files," +
                                " another wall with the same name already exists or that the custom wall is simply just broken." +
                                "\n\nThe best thing is probably just to delete it!";

                fileName = "DefaultMaterials";

                return new CustomMaterial(fileName, null, descriptor, null, null, null, errorMessage);
            }
        }

        public static async Task<CustomMaterial> CreateFromDataAsync(byte[] materialObject, string name)
        {
            if (materialObject == null)
            {
                throw new ArgumentNullException("materialObject cannot be null for the constructor!");
            }

            try
            {
                AssetBundle assetBundle = await AssetBundleExtensions.LoadFromMemoryAsync(materialObject);
                GameObject gameObject = await AssetBundleExtensions.LoadAssetAsync<GameObject>(assetBundle, "Assets/_CustomMaterial.prefab");
                gameObject.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                await RepairObjectShaders(gameObject);

                string fileName = $@"internalResource\{name}";
                MaterialDescriptor descriptor = gameObject.GetComponent<MaterialDescriptor>();
                Renderer materialRenderer = MaterialUtils.GetGameObjectRenderer(gameObject, "pixie");
                MeshFilter materialMeshFilter = MeshUtils.GetGameObjectMeshFilter(gameObject, "pixie");

                return new CustomMaterial(fileName, assetBundle, descriptor, gameObject, materialRenderer, materialMeshFilter, null);
            }
            catch (Exception ex)
            {
                Logger.log.Warn($"Something went wrong getting the AssetBundle from resource!");
                Logger.log.Warn(ex);

                MaterialDescriptor descriptor = new MaterialDescriptor
                {
                    MaterialName = "Internal Error (Report it!)",
                    AuthorName = $@"internalResource\{name}",
                    Icon = Utils.GetErrorIcon()
                };

                string errorMessage = $@"File: 'internalResource\\{name}'" +
                                "\n\nAn internal asset has failed to load." +
                                "\n\nThis shouldn't have happened and should be reported!" +
                                " Remember to include the log related to this incident." +
                                "\n\nDiscord: qqrz";

                string fileName = "DefaultMaterials";

                return new CustomMaterial(fileName, null, descriptor, null, null, null, errorMessage);
            }
        }

        private static async Task RepairObjectShaders(GameObject gameObject)
        {
            ShaderReplacementInfo shaderReplacementInfo = await ShaderRepair.FixShadersOnGameObjectAsync(gameObject);
            if (!shaderReplacementInfo.AllShadersReplaced)
            {
                foreach (string shaderName in shaderReplacementInfo.MissingShaderNames)
                {
                    Logger.log.Debug($"Failed to replace shader '{shaderName}' on {gameObject.name}");
                }
            }
        }

        public void Destroy()
        {
            if (AssetBundle != null)
            {
                AssetBundle.Unload(true);
            }
            else
            {
                UnityEngine.Object.Destroy(Descriptor);
            }
        }
    }
}
