using Reloaded.Memory;
using Reloaded.Memory.Enums;
using Reloaded.Memory.Interfaces;

namespace FlatOut2.Utils.ModLoader.Patches.Load;

public static class IncreaseLoadBufferSizePatch
{
    public static unsafe void Init(Config config)
    {
        Memory.Instance.ChangeProtection(0x559760, 4096, MemoryProtection.ReadWriteExecute);
        int bufferSize = 1 << config.ReadBufferSizeBits;
        
        *(int*)(0x55980E) = config.ReadBufferSizeBits; // second read buffer offset
        *(int*)(0x559807) = bufferSize; // read buffer size
        *(int*)(0x559815) = bufferSize - 1; // bytes avail in read buffer
        *(int*)(0x55982E) = bufferSize; // second read buffer offset
        *(int*)(0x559800) = (bufferSize * 2) + 63; // patch malloc for front and back buffer (the + is extra for alignment, required by game code)
    }
}