using FlatOut2.Utils.ModLoader.Interfaces;
using FlatOut2.Utils.ModLoader.Patches.Menu;
using FlatOut2.Utils.ModLoader.Template;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace FlatOut2.Utils.ModLoader;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private Redirector _redirector;
    private RedirectorController _redirectorController;

    public Mod(ModContext context)
    {
        var modLoader = context.ModLoader;
        var hooks = context.Hooks;
        var owner = context.Owner;
        _logger = context.Logger;
        _modConfig = context.ModConfig;
        
        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

        // If you want to implement e.g. unload support in your mod,
        // and some other neat features, override the methods in ModBase.

        // Init FlatOut SDK
        SDK.SDK.Init(hooks!);
        
        // Apply patches
        SupportCompressedMenuCarSkinsPatch.Init(hooks!);
        
        // Mod Logic Here
        var modConfigs  = modLoader.GetActiveMods().Select(x => x.Generic);
        _redirector           = new Redirector(modConfigs, modLoader);
        _redirectorController = new RedirectorController(_redirector);
        BfsLoader.Init(modConfigs, modLoader, _logger, hooks!);
        FileAccessServer.Initialize(_redirector, _redirectorController, _logger);
        FileAccessServer.SetConfiguration(context.Configuration);
        
        modLoader.AddOrReplaceController<IRedirectorController>(owner, _redirectorController);
        modLoader.ModLoading   += ModLoading;
        modLoader.ModUnloading += ModUnloading;
    }
    
    private void ModLoading(IModV1 mod, IModConfigV1 config)
    {
        _redirector.Add(config);
        BfsLoader.AddMod(config);
    }

    private void ModUnloading(IModV1 mod, IModConfigV1 config)
    {
        _logger.WriteLineAsync($"[ModLoader] Unloading mod. Please note this does not unload mod BFSes, just loose files.");
        _redirector.Remove(config);
        BfsLoader.RemoveMod(config);
    }

    #region Standard Overrides
    public override bool CanSuspend() => true;
    public override bool CanUnload() => false;
    public override void Suspend()
    {
        _logger.WriteLineAsync($"[ModLoader] Suspending. Please note this does not unload mod BFSes, just loose files.");
        FileAccessServer.Disable();
    }

    public override void Resume() => FileAccessServer.Enable();

    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        FileAccessServer.SetConfiguration(configuration);
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}