using CustomWalls.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SiraUtil.Zenject;

namespace CustomWalls.Utilities;

internal class MaterialAssetLoader : IAsyncInitializable
{
    private readonly PluginConfig config;

    public MaterialAssetLoader(PluginConfig config)
    {
        this.config = config;
    }
    
    public int SelectedMaterialIdx { get; internal set; } = 0;
    public IList<CustomMaterial> CustomMaterialObjects { get; private set; }
    public CustomMaterial GetSelectedMaterial() => CustomMaterialObjects[SelectedMaterialIdx];
    
    private bool isLoaded;
    private IEnumerable<string> customMaterialFiles = [];

    public async Task InitializeAsync(CancellationToken token)
    {
        await LoadAllCustomMaterials();
    }
    
    private async Task LoadAllCustomMaterials()
    {
        if (!isLoaded)
        {
            Directory.CreateDirectory(Plugin.AssetPath);

            IEnumerable<string> materialFilter = new List<string> { "*.pixie", "*.wall", };
            customMaterialFiles = Utils.GetFileNames(Plugin.AssetPath, materialFilter, SearchOption.AllDirectories, true);
            Plugin.Log.Debug($"{customMaterialFiles.Count()} external wall(s) found.");

            CustomMaterialObjects = await LoadCustomMaterials(customMaterialFiles);
            Plugin.Log.Debug($"{CustomMaterialObjects.Count} total wall(s) loaded.");

            if (config.SelectedWallMaterial != null)
            {
                int numberOfMaterials = CustomMaterialObjects.Count;
                for (int i = 0; i < numberOfMaterials; i++)
                {
                    if (CustomMaterialObjects[i].FileName == config.SelectedWallMaterial)
                    {
                        SelectedMaterialIdx = i;
                        break;
                    }
                }
            }

            isLoaded = true;
        }
    }

    /// <summary>
    /// Reload all CustomMaterials
    /// </summary>
    public async Task Reload()
    {
        Plugin.Log.Debug("Reloading the MaterialAssetLoader");
        Clear();
        await LoadAllCustomMaterials();
    }

    /// <summary>
    /// Clear all loaded CustomMaterials
    /// </summary>
    public void Clear()
    {
        int numberOfObjects = CustomMaterialObjects.Count;
        for (int i = 0; i < numberOfObjects; i++)
        {
            CustomMaterialObjects[i].Destroy();
            CustomMaterialObjects[i] = null;
        }

        isLoaded = false;
        SelectedMaterialIdx = 0;
        CustomMaterialObjects = new List<CustomMaterial>();
        customMaterialFiles = [];
    }

    private static async Task<IList<CustomMaterial>> LoadCustomMaterials(IEnumerable<string> customMaterialFiles)
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
                Plugin.Log.Warn($"Failed to load Custom Wall with name '{customMaterialFile}'.");
                Plugin.Log.Warn(ex);
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
            Plugin.Log.Warn($"Failed to load an internal file: '{fileName}'");
            Plugin.Log.Warn(ex);
        }

        return customMaterial;
    }
}