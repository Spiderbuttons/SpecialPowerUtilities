using Microsoft.Xna.Framework.Content;

namespace SpecialPowerUtilities.Models;

public class PlacementData
{
    [ContentSerializer(Optional = true)] 
    public string BeforeID = null;
    
    [ContentSerializer(Optional = true)]
    public string AfterID = null;

    [ContentSerializer(Optional = true)]
    public string ToPosition = null;
}