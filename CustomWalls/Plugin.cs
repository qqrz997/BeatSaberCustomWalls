using IPA;
using IPA.Config;
using IPA.Loader;
using IPA.Utilities;
using System.IO;
using CustomWalls.Installers;
using IPA.Config.Stores;
using IPA.Logging;
using SiraUtil.Zenject;

namespace CustomWalls;

[Plugin(RuntimeOptions.DynamicInit)]
internal class Plugin
{
    private readonly PluginMetadata metadata;
        
    [Init]
    public Plugin(Logger log, Config config, PluginMetadata metadata, Zenjector zenjector)
    {
        Log = log;
        this.metadata = metadata;
        var pluginConfig = config.Generated<PluginConfig>();
            
        zenjector.UseLogger(log);
        zenjector.Install<AppInstaller>(Location.App, pluginConfig);
        zenjector.Install<MenuInstaller>(Location.Menu);
        zenjector.Install<PlayerInstaller>(Location.Player);
    }
        
    public static Logger Log { get; private set; } = null!;
    public static string AssetPath => Path.Combine(UnityGame.InstallPath, "CustomWalls");
}