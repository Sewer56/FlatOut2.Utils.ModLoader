namespace Reloaded.Universal.Redirector.Utility;

/// <summary>
/// Utilities for working with relative paths.
/// </summary>
public static class RelativePaths
{
    /// <summary>
    /// Retrieves all relative file paths to a directory.
    /// </summary>
    /// <param name="directory">Absolute path to directory to get file paths from. </param>
    public static List<string> GetRelativeFilePaths(string directory)
    {
        return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Select(x => x.TrimStart(directory)).ToList();
    }
}