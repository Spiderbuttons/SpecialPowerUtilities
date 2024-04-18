using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SpecialPowerUtilities.Components;

public class RClickableTextureComponent : ClickableTextureComponent
{
    public RClickableTextureComponent(string name, Rectangle bounds, string label, string hoverText, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false) : base(name, bounds, label, hoverText, texture, sourceRect, scale, drawShadow)
    {
    }

    public RClickableTextureComponent(Rectangle bounds, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false) : base(bounds, texture, sourceRect, scale, drawShadow)
    {
    }

    public virtual void draw(SpriteBatch b, float rotation = 0f)
    {
        if (base.visible)
        {
            this.draw(b, Color.White, 0.86f + (float)base.bounds.Y / 20000f, rotation);
        }
    }
    
    public virtual void draw(SpriteBatch b, Color c, float layerDepth, float rotation, int frameOffset = 0, int xOffset = 0)
    {
        if (!base.visible)
        {
            return;
        }
        if (this.texture != null)
        {
            Rectangle r = this.sourceRect;
            if (frameOffset != 0)
            {
                r = new Rectangle(this.sourceRect.X + this.sourceRect.Width * frameOffset, this.sourceRect.Y, this.sourceRect.Width, this.sourceRect.Height);
            }
            if (this.drawShadow)
            {
                Utility.drawWithShadow(b, this.texture, new Vector2((float)(base.bounds.X + xOffset) + (float)(this.sourceRect.Width / 2) * this.baseScale, (float)base.bounds.Y + (float)(this.sourceRect.Height / 2) * this.baseScale), r, c, rotation, new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2), base.scale, flipped: false, layerDepth);
            }
            else
            {
                b.Draw(this.texture, new Vector2((float)(base.bounds.X + xOffset) + (float)(this.sourceRect.Width / 2) * this.baseScale, (float)base.bounds.Y + (float)(this.sourceRect.Height / 2) * this.baseScale), r, c, rotation, new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2), base.scale, SpriteEffects.None, layerDepth);
            }
        }
        if (!string.IsNullOrEmpty(base.label))
        {
            if (this.drawLabelWithShadow)
            {
                Utility.drawTextWithShadow(b, base.label, Game1.smallFont, new Vector2(base.bounds.X + xOffset + base.bounds.Width, (float)base.bounds.Y + ((float)(base.bounds.Height / 2) - Game1.smallFont.MeasureString(base.label).Y / 2f)), Game1.textColor);
            }
            else
            {
                b.DrawString(Game1.smallFont, base.label, new Vector2(base.bounds.X + xOffset + base.bounds.Width, (float)base.bounds.Y + ((float)(base.bounds.Height / 2) - Game1.smallFont.MeasureString(base.label).Y / 2f)), Game1.textColor);
            }
        }
    }
}