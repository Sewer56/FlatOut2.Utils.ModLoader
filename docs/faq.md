# FAQ

!!! info

    Contains any questions you may possible have.

## When are mod files loaded.  

Mod files are directly loaded after the base game loads the `patch` file, i.e. any Reloaded mods will have priority over those defined in `patch` file.  

## What is the Mod Load Order

Reloaded loads mods in top to bottom order (you can drag & drop mods in list). As such, your mod order should be same as when using classic `patch` file method.

## What is the File Load Order

Files are loaded in the following order:

- Loose files in Reloaded mods.  
- BFSes loaded by Reloaded mods.  
- BFSes in FlatOut's `patch` file.  
- BFSes in FlatOut's `filesystem` file.  