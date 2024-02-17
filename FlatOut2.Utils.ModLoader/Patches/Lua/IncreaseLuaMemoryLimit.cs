using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory;
using Reloaded.Memory.Enums;
using Reloaded.Memory.Interfaces;
using Reloaded.Universal.Redirector.Structures;

namespace FlatOut2.Utils.ModLoader.Patches.Lua;

public static class IncreaseLuaMemoryLimit
{
    private static IAsmHook _notifyOnOutOfMemoryHook = null!;
    
    public static unsafe void Init(Config config, IReloadedHooks hooks)
    {
        Memory.Instance.ChangeProtection(0x6500A0, 4096, MemoryProtection.ReadWriteExecute);
        *(int*)(0x6500AF) = config.LuaMemoryLimit;

        var utils = hooks.Utilities;
        hooks.CreateAsmHook(new[]
        {
            "use32",
            $"{utils.GetAbsoluteCallMnemonics((nint)utils.GetFunctionPointer(typeof(IncreaseLuaMemoryLimit), nameof(WarnOnOutOfLuaMemory)), false)}",

        }, 0x525696, new AsmHookOptions()
        {
            PreferRelativeJump = true,
            Behaviour = AsmHookBehaviour.ExecuteFirst
        }).Activate();
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void WarnOnOutOfLuaMemory()
    {
        const string description = "It seems you've been cooking quite hard.\n" +
                                   "Just how many lines of Lua code have you written?\n\n" +
                                   "Anyway, game's kind out of Lua memory and will crash soon.\n" +
                                   "Just making sure you don't get a completely unrelated error down the road.\n\n" +
                                   "You can increase this memory limit ('LuaMemoryLimit') in the config for 'ModLoader' mod.\n\n" +
                                   "I let you know of this devastation, because I AM GIGACHAD";
        const string title = "!!! DANGER: Out of Lua Memory !!!";
        MessageBoxW(IntPtr.Zero,  description, title, 0);
    }
    
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hWnd, string lpText, string lpCaption, uint uType);
}