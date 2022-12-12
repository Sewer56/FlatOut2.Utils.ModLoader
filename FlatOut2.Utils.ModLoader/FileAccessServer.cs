using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FlatOut2.SDK.Functions;
using FlatOut2.SDK.Utilities;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

namespace FlatOut2.Utils.ModLoader;

/// <summary>
/// All file access goes through the hooks here.
/// </summary>
public static class FileAccessServer
{
    private static Redirector _redirector = null!;
    private static RedirectorController _redirectorController = null!;
    private static Config _config = null!;
    private static ILogger _logger = null!;
    private static IHook<FileSystemFuncs.OpenFileFnPtr> _openFile = null!;
    private static IHook<FileSystemFuncs.OpenFileSystemFileFnPtr> _openFileFromFileSystem = null!;
    private static IHook<FileSystemFuncs.DoesFileExistFnPtr> _doesFileExist = null!;

    public static void Initialize(Redirector redirector, RedirectorController redirectorController, ILogger logger)
    {
        _redirector = redirector;
        _redirectorController = redirectorController;
        _logger = logger;
        _openFile = FileSystemFuncs.OpenFile.HookAs<FileSystemFuncs.OpenFileFnPtr>(typeof(FileAccessServer), nameof(OpenBfsFileImpl)).Activate();
        _openFileFromFileSystem = FileSystemFuncs.OpenFileSystemFile.HookAs<FileSystemFuncs.OpenFileSystemFileFnPtr>(typeof(FileAccessServer), nameof(OpenFileFromFileSystemPtrImpl)).Activate();
        _doesFileExist = FileSystemFuncs.DoesFileExist.HookAs<FileSystemFuncs.DoesFileExistFnPtr>(typeof(FileAccessServer), nameof(DoesFileExistImpl)).Activate();
    }

    public static void SetConfiguration(Config config) => _config = config;
    
    public static void Disable() => _openFile.Disable();

    public static void Enable() => _openFile.Enable();
    
    [UnmanagedCallersOnly(CallConvs = new []{ typeof(CallConvStdcall) })]
    private static unsafe nint DoesFileExistImpl(byte* filepath, byte checkBfsWrapper)
    {
        // Note: This function does a bit more than just checking for existence, so just in case we still call original,
        //       but with correct path.
        
        var path = Marshal.PtrToStringAnsi((nint)filepath);
        var fullPath = Path.GetFullPath(path!);
        
        if (!_redirector.TryRedirect(fullPath, out var newPath))
            return new nint(_doesFileExist.OriginalFunction.Value.Invoke(filepath, checkBfsWrapper));
        
        using var tempString = FinishFileRedirect(fullPath, newPath);
        return new nint(_doesFileExist.OriginalFunction.Value.Invoke((byte*)tempString.Address, byte.MaxValue)); // -1 = do not check BFS, since our file is from outside
    }

    [UnmanagedCallersOnly(CallConvs = new []{ typeof(CallConvStdcall) })]
    private static unsafe nint OpenBfsFileImpl(byte* filepath, void* thisptr, int flags)
    {
        var fullPath = InitFileRedirect(filepath);

        // Attempt Redirection
        if (!_redirector.TryRedirect(fullPath, out var newPath))
            return new nint(_openFile.OriginalFunction.Value.Invoke(filepath, (byte*)thisptr, flags));
        
        using var tempString = FinishFileRedirect(fullPath, newPath);
        return new nint(_openFileFromFileSystem.OriginalFunction.Value.Invoke(flags, (byte*)tempString.Address));
        
        // ^ Not a bug, we are loading from FS!
    }
    
    [UnmanagedCallersOnly(CallConvs = new []{ typeof(CallConvStdcall) })]
    private static unsafe nint OpenFileFromFileSystemPtrImpl(int flags, byte* filepath)
    {
        var fullPath = InitFileRedirect(filepath);
        
        // Attempt Redirection
        if (!_redirector.TryRedirect(fullPath, out var newPath))
            return new nint(_openFileFromFileSystem.OriginalFunction.Value.Invoke(flags, filepath));
        
        using var tempString = FinishFileRedirect(fullPath, newPath);
        return new nint(_openFileFromFileSystem.OriginalFunction.Value.Invoke(flags, (byte*)tempString.Address));
    }

    private static unsafe string InitFileRedirect(byte* filepath)
    {
        var path = Marshal.PtrToStringAnsi((nint)filepath);
        var fullPath = Path.GetFullPath(path!);

        _redirectorController.Loading?.Invoke(fullPath);
        if (_config.PrintLoadedFiles)
            _logger.WriteLineAsync(fullPath);
        
        return fullPath;
    }
    
    private static TemporaryNativeString FinishFileRedirect(string fullPath, string newPath)
    {
        _redirectorController.Redirecting?.Invoke(fullPath, newPath);
        if (_config.PrintRedirections)
            _logger.WriteLineAsync($"=> {newPath}");

        return new TemporaryNativeString(newPath);
    }
}