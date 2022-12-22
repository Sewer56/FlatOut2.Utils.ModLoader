using System.ComponentModel;
using FlatOut2.Utils.ModLoader.Template.Configuration;

namespace FlatOut2.Utils.ModLoader;

public class Config : Configurable<Config>
{
    /*
        User Properties:
            - Please put all of your configurable properties here.

        By default, configuration saves as "Config.json" in mod user config folder.    
        Need more config files/classes? See Configuration.cs

        Available Attributes:
        - Category
        - DisplayName
        - Description
        - DefaultValue

        // Technically Supported but not Useful
        - Browsable
        - Localizable

        The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
    */

    [DisplayName("Print Loaded Files")]
    [Description("Prints files loaded by the game to console.")]
    [DefaultValue(false)]
    public bool PrintLoadedFiles { get; set; } = false;
    
    [DisplayName("Print Redirections")]
    [Description("Prints files replaced by the mod to console.\n" +
                 "Applies to unpacked files only.")]
    [DefaultValue(false)]
    public bool PrintRedirections { get; set; } = false;


    [DisplayName("Buffer Size")]
    [Description("Size of buffer for read operations from disk. Each number represents a power of 2, i.e.\n" +
                 "15: 32K Reads\n" +
                 "16: 64K Reads\n" +
                 "17: 128K Reads\n" +
                 "etc. Vanilla game uses 15. Do not change unless you know what you're doing.")]
    [DefaultValue(17)]
    public int ReadBufferSizeBits { get; set; } = 17;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}