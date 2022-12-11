﻿namespace FlatOut2.Utils.ModLoader.Interfaces;

public interface IRedirectorController
{
    /// <summary>
    /// This event happens when a file is about to be redirected to another file.
    /// </summary>
    Redirecting? Redirecting { get; set; }
    
    /// <summary>
    /// This even happens when a file is about to be loaded.
    /// </summary>
    Loading? Loading { get; set; }

    /// <summary>
    /// Adds a file to be redirected.
    /// </summary>
    /// <param name="oldFilePath">The absolute path of the file to be replaced. Tip: Use Path.GetFullPath()</param>
    /// <param name="newFilePath">The absolute path to the new file. Tip: Use Path.GetFullPath()</param>
    void AddRedirect(string oldFilePath, string newFilePath);

    /// <summary>
    /// Removes a file from being redirected.
    /// </summary>
    /// <param name="oldFilePath">The absolute path of the file to no longer be replaced. Tip: Use Path.GetFullPath()</param>
    void RemoveRedirect(string oldFilePath);
    
    /// <summary>
    /// Adds a folder for file redirection.
    /// The files inside the folder are mapped relative to the executable directory of the modified application's executable.
    /// </summary>
    /// <param name="folderPath">The full path of the folder to redirect files to.</param>
    /// <returns>True if the operation succeeds, else false. Operation fails if a folder is already redirected.</returns>
    void AddRedirectFolder(string folderPath);

    /// <summary>
    /// Removes a folder to be redirected.
    /// The files inside the folder are mapped relative to the executable directory of the modified application's executable.
    /// </summary>
    /// <returns>True if the operation succeeds, else false.</returns>
    void RemoveRedirectFolder(string folderPath);
    
    /// <summary>
    /// Adds a folder for file redirection, specifying what folder inside the game directory the folder should map to.
    /// </summary>
    /// <param name="folderPath">The full path of the folder to redirect files to.</param>
    /// <param name="sourceFolder">Folder path relative to game's directory to redirect files from. Path should start with back or forward slash.</param>
    /// <returns>True if the operation succeeds, else false. Operation fails if a folder is already redirected.</returns>
    void AddRedirectFolder(string folderPath, string sourceFolder);

    /// <summary>
    /// Removes a folder to be redirected.
    /// The files inside the folder are mapped relative to the executable directory of the modified application's executable.
    /// </summary>
    /// <param name="folderPath">The full path of the folder to redirect files to.</param>
    /// <param name="sourceFolder">Folder path relative to game's directory to redirect files from. Path should start with back or forward slash.</param>
    /// <returns>True if the operation succeeds, else false.</returns>
    void RemoveRedirectFolder(string folderPath, string sourceFolder);
    
    /// <summary>
    /// Temporarily disables the redirector.
    /// </summary>
    void Disable();

    /// <summary>
    /// Re-enables the redirector after a temporary disable.
    /// </summary>
    void Enable();
}

/// <summary>
/// Called when a file is about to be redirected.
/// </summary>
/// <param name="oldPath">The path that was originally going to be opened.</param>
/// <param name="newPath">The new path of the file.</param>
public delegate void Redirecting(string oldPath, string newPath);

/// <summary>
/// Called when a file with a specific file is going to be loaded.
/// Note: This is before redirection takes place, see <see cref="Redirecting"/> if you want to know redirected paths.
/// </summary>
/// <param name="path">The path to be loaded.</param>
public delegate void Loading(string path);