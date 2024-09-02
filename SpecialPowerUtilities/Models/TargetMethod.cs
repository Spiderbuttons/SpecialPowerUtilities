using Microsoft.Xna.Framework.Content;

namespace SpecialPowerUtilities.Models;

public class TargetMethod
{
    [ContentSerializer(Optional = false)]
    public string Type;

    [ContentSerializer(Optional = false)]
    public string Method;

    [ContentSerializer(Optional = true)]
    public string[] Parameters = [];

    [ContentSerializer(Optional = true)]
    public bool PropertyGetter = false;
    
    [ContentSerializer(Optional = true)]
    public bool PropertySetter = false;

    [ContentSerializer(Optional = true)]
    public string Assembly = "Stardew Valley";
}