using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace FlatOut2.Utils.ModLoader.Patches.Menu;

/// <summary>
/// Patch that adds support for compressed menu car skins
/// </summary>
public static class SupportCompressedMenuCarSkinsPatch
{
    private static IAsmHook _loadCompressedModelHook;
    private static IAsmHook _loadCompressedSkinHook;

    public static void Init(IReloadedHooks hooks)
    {
        // By some miracle, the exact same code was generated, with exact same registers, so I can reuse asm code
        var loadCompressedMenuItemHook = new[]
        {
            "use32",
            "add esp, 4", // The code pushes a parameter a few instructions earlier, we need to pop it.
            "mov ecx, [esi+20h]", // Get IOpenedFile
            
            // Push params right to left.
            "push dword [ecx + 4010h]", // Push Uncompressed Size
            "push eax", // Push OutputBuffer
            "push esi", // Push OpenedFileReader
            
            // int __stdcall ReadNumBytesFromBfs(OpenedFileReader *openedFileReader, int outputBuffer, int numBytes)
            $"{hooks.Utilities.GetAbsoluteCallMnemonics(0x54C5A0, false)}", // push skin data addr
            
            "mov ecx, [esi+20h]", // Get IOpenedFile
            "mov dword [ecx + 0x407C], 2", // Set what I believe is async completion status.
        };
        
        _loadCompressedSkinHook = hooks.CreateAsmHook(loadCompressedMenuItemHook, 0x4A471B, AsmHookBehaviour.DoNotExecuteOriginal, 11).Activate();
        _loadCompressedModelHook = hooks.CreateAsmHook(loadCompressedMenuItemHook, 0x4A45CE, AsmHookBehaviour.DoNotExecuteOriginal, 11).Activate();
    }
}