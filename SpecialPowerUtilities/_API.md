# C# AUTHOR API DOCUMENTATION

## Power Categories
If you are adding your powers with C#, then you will need to use this API to create a custom power category for your powers to be placed into, since you will not be able to use the content pipeline to do so. Special Power Utilities provides an [API interface](ISpecialPowerAPI.cs) for you to copy into your project to use with SMAPI's API helper.

Keep in mind that if your power IDs are not prefixed with your mod ID or their prefix does not match the mod ID that you pass into the API, you will still need to manually specify that they should go into your custom category, just as you would with Content Patcher. However, this is still done via the CustomFields on your powers and not through the API. [Please see the normal documentation for more details](_DOCS.md).

Once you have copied the API interface into your project, here is a full example of what registering a power category could look like:

```csharp
public override void Entry(IModHelper helper)
{
    Helper.Events.Content.AssetRequested += this.OnAssetRequested;
    Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
}

private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
{
    if (e.NameWithoutLocale.IsEquivalentTo("Data/Powers"))
    {
        e.Edit(asset =>
        {
            var powers = asset.AsDictionary<string, PowersData>();
            powers.Data["AuthorName.ModName_Power"] = new PowersData()
            {
                DisplayName = "Test Power",
                Description = "Test Description",
                TexturePath = "LooseSprites\\Cursors_1_6",
                TexturePosition = new Point(16, 340),
                UnlockedCondition = "TRUE"
            };
        });
    }
}

private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
{
    ISpecialPowerAPI spuApi = Helper.ModRegistry.GetApi<ISpecialPowerAPI>("Spiderbuttons.SpecialPowerUtilities");
    if (spuApi is not null)
    {
        bool res = spuApi.RegisterPowerCategory(ModManifest.UniqueID, () => "Test Category", "TileSheets\\Objects_2", new Point(80, 272), new Point(16, 16));
        if (res)
        {
            Log.Info("Power category successfully registered.");
        } else Log.Warning("Power category has already been registered.");
    }
}
```