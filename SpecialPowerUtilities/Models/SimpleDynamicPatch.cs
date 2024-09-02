using Microsoft.Xna.Framework.Content;

namespace SpecialPowerUtilities.Models;

public class SimpleDynamicPatch
{
    [ContentSerializer(Optional = false)]
    public string Id;

    [ContentSerializer(Optional = false)]
    public TargetMethod Target;

    [ContentSerializer(Optional = false)]
    public string Type;

    [ContentSerializer(Optional = true)]
    public string Condition = null;
    
    [ContentSerializer(Optional = true)]
    public int Priority = 0;
    
    [ContentSerializer(Optional = true)]
    public string Action = null;

    [ContentSerializer(Optional = true)]
    public string[] Actions = null;
}