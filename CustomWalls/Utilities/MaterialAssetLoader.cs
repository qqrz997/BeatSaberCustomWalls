using CustomWalls.Data;
using CustomWalls.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CustomWalls.Utilities
{
    public class MaterialAssetLoader
    {
        public static bool IsLoaded { get; private set; }
        public static int SelectedMaterial { get; internal set; } = 0;
        public static IList<CustomMaterial> CustomMaterialObjects { get; private set; }
        public static IEnumerable<string> CustomMaterialFiles { get; private set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Load all CustomMaterials
        /// </summary>
        internal static async Task Load()
        {
            if (!IsLoaded)
            {
                Directory.CreateDirectory(Plugin.PluginAssetPath);

                IEnumerable<string> materialFilter = new List<string> { "*.pixie", "*.wall", };
                CustomMaterialFiles = Utils.GetFileNames(Plugin.PluginAssetPath, materialFilter, SearchOption.AllDirectories, true);
                Logger.log.Debug($"{CustomMaterialFiles.Count()} external wall(s) found.");

                CustomMaterialObjects = await LoadCustomMaterials(CustomMaterialFiles);
                Logger.log.Debug($"{CustomMaterialObjects.Count} total wall(s) loaded.");

                if (Configuration.CurrentlySelectedMaterial != null)
                {
                    int numberOfMaterials = CustomMaterialObjects.Count;
                    for (int i = 0; i < numberOfMaterials; i++)
                    {
                        if (CustomMaterialObjects[i].FileName == Configuration.CurrentlySelectedMaterial)
                        {
                            SelectedMaterial = i;
                            break;
                        }
                    }
                }

                IsLoaded = true;
            }
        }

        /// <summary>
        /// Reload all CustomMaterials
        /// </summary>
        internal static async Task Reload()
        {
            Logger.log.Debug("Reloading the MaterialAssetLoader");
            Clear();
            await Load();
        }

        /// <summary>
        /// Clear all loaded CustomMaterials
        /// </summary>
        internal static void Clear()
        {
            int numberOfObjects = CustomMaterialObjects.Count;
            for (int i = 0; i < numberOfObjects; i++)
            {
                CustomMaterialObjects[i].Destroy();
                CustomMaterialObjects[i] = null;
            }

            IsLoaded = false;
            SelectedMaterial = 0;
            CustomMaterialObjects = new List<CustomMaterial>();
            CustomMaterialFiles = Enumerable.Empty<string>();
        }

        private async static Task<IList<CustomMaterial>> LoadCustomMaterials(IEnumerable<string> customMaterialFiles)
        {
            IList<CustomMaterial> customMaterials = new List<CustomMaterial>
            {
                CustomMaterial.DefaultMaterial
            };

            IEnumerable<string> embeddedFiles = new List<string>
            {
                "MysticalSnowWalls.pixie",
                "PixelWalls.pixie",
                "PlainWalls.pixie",
                "TransparentWalls.pixie"
            };

            foreach (string embeddedFile in embeddedFiles)
            {
                CustomMaterial customMaterial = await LoadEmbeddedMaterial(embeddedFile);
                if (customMaterial != null)
                {
                    customMaterials.Add(customMaterial);
                }
            }

            foreach (string customMaterialFile in customMaterialFiles)
            {
                try
                {
                    CustomMaterial newMaterial = await CustomMaterial.CreateAsync(customMaterialFile);
                    if (newMaterial != null)
                    {
                        customMaterials.Add(newMaterial);
                    }
                }
                catch (Exception ex)
                {
                    Logger.log.Warn($"Failed to load Custom Wall with name '{customMaterialFile}'.");
                    Logger.log.Warn(ex);
                }
            }

            return customMaterials;
        }

        private static async Task<CustomMaterial> LoadEmbeddedMaterial(string fileName)
        {
            CustomMaterial customMaterial = null;

            try
            {
                byte[] resource = Utils.LoadFromResource($"CustomWalls.Resources.Materials.{fileName}");
                customMaterial = await CustomMaterial.CreateFromDataAsync(resource, fileName);
            }
            catch (Exception ex)
            {
                Logger.log.Warn($"Failed to load an internal file: '{fileName}'");
                Logger.log.Warn(ex);
            }

            return customMaterial;
        }
    }
}
