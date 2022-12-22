using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FlatOut2.SDK.Functions;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Universal.Redirector.Structures;
using SharpZstd.Interop;
using static SharpZstd.Interop.Zstd;

namespace FlatOut2.Utils.ModLoader.Patches.Compression;

public static class SupportCustomCompressionPatch
{
    private static IAsmHook _storeCompressedFileHook;
    private static IAsmHook _initCompressedFileHook;
    private static IHook<Zlib.InflateFnPtr> _inflateHook;
    private static unsafe ZSTD_DCtx* _zstdContext;

    public static unsafe void Init(IReloadedHooks hooks)
    {
        var utils = hooks.Utilities;
        _zstdContext = ZSTD_createDStream();
        _storeCompressedFileHook = hooks.CreateAsmHook(new[]
        {
            "use32",
            
            // Store method for other hook
            $"mov al, [ebx]",
            $"mov [esi+404Ch], al" // stored as zlib msg, cleared if init as zlib
            
        }, 0x5604B1, new AsmHookOptions()
        {
            MaxOpcodeSize = 5,
            PreferRelativeJump = true,
            Behaviour = AsmHookBehaviour.ExecuteAfter
        }).Activate();
        
        _initCompressedFileHook = hooks.CreateAsmHook(new[]
        {
            "use32",

            // Get Method
            $"mov al, [esi+404Ch]", // depends on other hook

            // Try ZStd
            "test al, 8",
            "je tryzlib",
            $"{utils.GetAbsoluteCallMnemonics((nint)utils.GetFunctionPointer(typeof(SupportCustomCompressionPatch), nameof(InitZStd)), false)}",
            $"jmp exit",

            "tryzlib:",
            // Else default to original.
            $"{utils.GetAbsoluteCallMnemonics(0x5F7FE0, false)}",
            "exit:"

        }, 0x56052F, new AsmHookOptions()
        {
            MaxOpcodeSize = 5,
            PreferRelativeJump = true,
            Behaviour = AsmHookBehaviour.DoNotExecuteOriginal
        }).Activate();
        
        // Decompress hook
        _inflateHook = Zlib.Inflate.HookAs<Zlib.InflateFnPtr>(typeof(SupportCustomCompressionPatch), nameof(Inflate)).Activate();
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static unsafe int InitZStd(ZlibStream* zlibStream, byte* zlibVersion, int headerSize)
    {
        ZSTD_resetDStream(_zstdContext);
        zlibStream->Adler = (int)CompressionType.Zstd;
        return 0;
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static unsafe ZlibReturnValue Inflate(ZlibStream* stream, int flush)
    {
        if (stream->Adler == (int)CompressionType.Zstd)
        {
            var outputBuf = new ZSTD_outBuffer { dst = stream->NextOut, pos = 0, size = (nuint)stream->AvailableOut };
            var inputBuf = new ZSTD_inBuffer { src = stream->NextIn, pos = 0, size = (nuint)stream->AvailableIn };
            ZSTD_decompressStream(_zstdContext, &outputBuf, &inputBuf);
            
            stream->TotalIn += (int)inputBuf.pos;
            stream->NextIn += inputBuf.pos;
            stream->NextOut += outputBuf.pos;
            stream->AvailableIn -= (int)inputBuf.pos;
            stream->AvailableOut -= (int)outputBuf.pos;
            
            return ZlibReturnValue.Z_OK;
        }
        
        return _inflateHook.OriginalFunction.Value.Invoke(stream, flush);
    }

    private enum CompressionType
    {
        Zstd
    }
}