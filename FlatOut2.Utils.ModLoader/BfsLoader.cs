using System.Text;
using FlatOut2.SDK.Functions;
using FlatOut2.SDK.Utilities;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Universal.Redirector.Utility;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace FlatOut2.Utils.ModLoader;

/// <summary>
/// Class that allows for loading BFSes at runtime.
/// </summary>
public static class BfsLoader
{
    private static IModLoader _modLoader = null!;
    private static ILogger _logger = null!;
    private static List<string> _modIds = new List<string>();
    private static IAsmHook _startupHook = null!;
    private static IReverseWrapper<Action> _startupInitFn = null!;

    public static void Init(IEnumerable<IModConfigV1> configs, IModLoader modLoader, ILogger logger,
        IReloadedHooks hooks)
    {
        _modLoader = modLoader;
        _logger = logger;
        _modIds.AddRange(configs.Select(x => x.ModId));
        var utilities = hooks.Utilities;
        _startupHook = hooks.CreateAsmHook(new[]
        {
            "use32",
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"{utilities.GetAbsoluteCallMnemonics<Action>(InitModBfs, out _startupInitFn)}",
            $"{utilities.PopCdeclCallerSavedRegisters()}"
        }, 0x520F7E).Activate();
    }

    public static void AddMod(IModConfigV1 modConfig) => _modIds.Add(modConfig.ModId);
    public static void RemoveMod(IModConfigV1 modConfig) => _modIds.Remove(modConfig.ModId);

    private static unsafe void InitModBfs()
    {
        // Note: This code could be faster, but I'm not too bothered in this specific case.
        var stringBuilder = new StringBuilder(4096);
        foreach (var modId in _modIds)
        {
            var bfsFolder = _modLoader.GetDirectoryForModId(modId) + "\\BFS";
            if (!Directory.Exists(bfsFolder))
                continue;

            var bfsFiles  = Directory.GetFiles(bfsFolder, "*.bfs");
            foreach (var bfsFile in bfsFiles.OrderBy(f => f))
            {
                _logger.WriteLineAsync($"[ModLoader] Adding BFS File: {bfsFile}");
                stringBuilder.AppendLine(bfsFile);
            }
        }
        
        // Load BFSes
        if (stringBuilder.Length > 0)
        {
            using var tempFile = new TemporaryTextFile(stringBuilder.ToString().Trim());
            using var tempString = new TemporaryNativeString(tempFile.FilePath);
            FileSystemFuncs.LoadBfsList.GetWrapper()((byte*)tempString.Address);
        }
        
        // Cleanup
        _startupHook.Disable();
        _modIds = null!;
        _startupInitFn = null!;
    }

    [Function(CallingConventions.Stdcall)]
    private delegate void Action();
}