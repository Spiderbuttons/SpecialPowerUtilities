using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewValley.GameData.Powers;

namespace SpecialPowerUtilities.Models;

public class SPUData
{
    [ContentSerializer(Optional = true)]
    public string Category = null;
    
    [ContentSerializer(Optional = true)]
    public int PlacementIndex = -1;

    [ContentSerializer(Optional = true)]
    public string Placement = "";
    
    public string DisplayName;
    
    [ContentSerializer(Optional = true)]
    public string Description = "";
    
    public string TexturePath;
    
    public Point TexturePosition;
    
    public string UnlockedCondition;
}