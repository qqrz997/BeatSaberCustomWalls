using CustomWalls.Data;
using CustomWalls.HarmonyPatches;
using CustomWalls.Settings;
using CustomWalls.Settings.UI;
using CustomWalls.Utilities;
using Hive.Versioning;
using IPA;
using IPA.Config;
using IPA.Loader;
using IPA.Utilities;
using IPA.Utilities.Async;
using System.IO;
using System.Threading.Tasks;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace CustomWalls
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        private readonly PluginMetadata metadata;
        
        [Init]
        public Plugin(IPALogger logger, Config config, PluginMetadata metadata, Zenjector zenjector)
        {
            this.metadata = metadata;
            
            Logger.log = logger;
            Configuration.Init(config);

            zenjector.Install<StandardGameplayInstaller>(container => { });
        }
        
        public static string PluginAssetPath => Path.Combine(UnityGame.InstallPath, "CustomWalls");

        [OnEnable]
        public void OnEnable() => UnityMainThreadTaskScheduler.Factory.StartNew(() => Load());
        [OnDisable]
        public void OnDisable() => Unload();

        private void OnGameSceneLoaded()
        {
            CustomMaterial customMaterial = MaterialAssetLoader.CustomMaterialObjects[MaterialAssetLoader.SelectedMaterial];
            if (customMaterial.Descriptor.DisablesScore
                || Configuration.UserDisabledScores)
            {
                BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(metadata.Name);
                Logger.log.Info("ScoreSubmission has been disabled.");
            }
        }

        private async Task Load()
        {
            Configuration.Load();
            await MaterialAssetLoader.Load();
            CustomMaterialsPatches.ApplyHarmonyPatches();
            SettingsUI.CreateMenu();
            AddEvents();

            Logger.log.Info($"{metadata.Name} v.{metadata.HVersion} has started.");
        }

        private void Unload()
        {
            RemoveEvents();
            CustomMaterialsPatches.RemoveHarmonyPatches();
            Configuration.Save();
            MaterialAssetLoader.Clear();
            SettingsUI.RemoveMenu();
        }

        private void AddEvents()
        {
            RemoveEvents();
            BS_Utils.Utilities.BSEvents.gameSceneLoaded += OnGameSceneLoaded;
        }

        private void RemoveEvents()
        {
            BS_Utils.Utilities.BSEvents.gameSceneLoaded -= OnGameSceneLoaded;
        }
    }
}
