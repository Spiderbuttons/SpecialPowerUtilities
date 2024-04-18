using GenericModConfigMenu;
using StardewModdingAPI;

namespace SpecialPowerUtilities.Config;

public sealed class ModConfig
{
    public bool EnableNewPowersTab { get; set; } = true;
    
    public bool ParseModNames { get; set; } = true;
    
    public bool EnableMiscCategory { get; set; } = true;

    public ModConfig()
    {
        Init();
    }

    private void Init()
    {
        this.EnableNewPowersTab = true;
        this.ParseModNames = true;
        this.EnableMiscCategory = true;
    }

    public void SetupConfig(IGenericModConfigMenuApi configMenu, IManifest ModManifest, IModHelper Helper)
    {
        configMenu.Register(
            mod: ModManifest,
            reset: Init,
            save: () => Helper.WriteConfig(this)
        );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: i18n.Config_EnableCategories_Name,
            tooltip: i18n.Config_EnableCategories_Description,
            getValue: () => this.EnableNewPowersTab,
            setValue: value => this.EnableNewPowersTab = value
        );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: i18n.Config_ParseModNames_Name,
            tooltip: i18n.Config_ParseModNames_Description,
            getValue: () => this.ParseModNames,
            setValue: value => this.ParseModNames = value
        );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: i18n.Config_EnableMiscCategory_Name,
            tooltip: i18n.Config_EnableMiscCategory_Description,
            getValue: () => this.EnableMiscCategory,
            setValue: value => this.EnableMiscCategory = value
        );
    }
}