namespace Reloaded.Universal.Redirector.Utility;

/// <summary>
/// Writes a temporary text file to disk.
/// </summary>
public struct TemporaryTextFile : IDisposable
{
    public string FilePath;

    public TemporaryTextFile(string text)
    {
        FilePath = Path.GetTempFileName();
        File.WriteAllText(FilePath, text);
    }
    
    public void Dispose() => File.Delete(FilePath);
}