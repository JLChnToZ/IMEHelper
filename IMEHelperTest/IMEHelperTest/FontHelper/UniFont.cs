using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FontHelper {
    public static class ExtensionFunctions {
        public static void DrawString(this SpriteBatch spriteBatch, UniFont UniFont, string text, Vector2 position, Color color) {
            UniFont.Draw(text, spriteBatch, position, color: color);
        }
        public static void DrawString(this SpriteBatch spriteBatch, UniFont UniFont, StringBuilder text, Vector2 position, Color color) {
            UniFont.Draw(text.ToString(), spriteBatch, position, color: color);
        }
        public static void DrawString(this SpriteBatch spriteBatch, UniFont UniFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            UniFont.Draw(text, spriteBatch, position, center: origin, color: color, scale: new Vector2(scale), depth: layerDepth);
        }
        public static void DrawString(this SpriteBatch spriteBatch, UniFont UniFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            UniFont.Draw(text.ToString(), spriteBatch, position, center: origin, color: color, scale: new Vector2(scale), depth: layerDepth);
        }
        public static void DrawString(this SpriteBatch spriteBatch, UniFont UniFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            UniFont.Draw(text, spriteBatch, position, center: origin, color: color, scale: scale, depth: layerDepth);
        }
        public static void DrawString(this SpriteBatch spriteBatch, UniFont UniFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            UniFont.Draw(text.ToString(), spriteBatch, position, center: origin, color: color, scale: scale, depth: layerDepth);
        }
    }

    public class UniFont {
        private Texture2D baseTexture;
        public const int fontBaseSize = 16;
        public const int glyphColumnSize = 256;

        public UniFont(Texture2D baseTexture) {
            this.baseTexture = baseTexture;
        }

        public bool isFullWidth(char c) {
            uint i = (uint)c;
            if (i >= 0x0000 && i < 0x024F) return false;
            if (i >= 0x0370 && i < 0x03FF) return false;
            if (i >= 0x0400 && i < 0x052F) return false;
            if (i >= 0xFB29 && i < 0xFCDD) return false;
            if (i >= 0xFE50 && i < 0xFE6F) return false;
            if (i >= 0xFF60 && i < 0xFF91) return false;
            return true; // Stub
        }

        public Vector2 MeasureString(string content) {
            float maxWidth = 0F, Width = 0F, Height = 0F;
            foreach (char c in content) {
                switch (c) {
                    case '\r':
                        continue;
                    case '\n':
                        maxWidth = Math.Max(Width, maxWidth);
                        Width = 0F;
                        Height += fontBaseSize;
                        break;
                    default:
                        if (isFullWidth(c)) Width += fontBaseSize;
                        else Width += fontBaseSize / 2;
                        break;
                }
            }
            return new Vector2(Math.Max(Width, maxWidth), Height);
        }

        public void Draw(string content, SpriteBatch SB, Vector2 position, Vector2? clippingsize = null,
            Vector2? center = null, float rotation = 0, Vector2? scale = null, float depth = 0, Color? color = null) {
            if (!center.HasValue) center = Vector2.Zero;
            if (!scale.HasValue) scale = Vector2.One;
            if (!clippingsize.HasValue) clippingsize = MeasureString(content);
            if (!color.HasValue) color = Color.White;
            Matrix transform = Matrix.CreateTranslation(-center.Value.X * scale.Value.X, -center.Value.Y * scale.Value.Y, 0f) * Matrix.CreateRotationZ(rotation);
            int curX = 0, curY = 0;
            foreach (char c in content) {
                int posX = (int)c % glyphColumnSize;
                int posY = (int)c / glyphColumnSize;
                switch (c) {
                    case '\r':
                        continue;
                    case '\n':
                        curX = 0;
                        curY += fontBaseSize;
                        break;
                    default:
                        SB.Draw(baseTexture,
                            Vector2.Transform(new Vector2(curX, curY), transform) + position,
                            new Rectangle(posX * fontBaseSize, posY * fontBaseSize, fontBaseSize, fontBaseSize),
                            color.Value, rotation, Vector2.Zero, scale.Value.X, SpriteEffects.None, depth);
                        curX += isFullWidth(c) ? fontBaseSize : fontBaseSize / 2;
                        if (curX >= clippingsize.Value.X) {
                            curX = 0;
                            curY += fontBaseSize;
                        }
                        break;
                }
                if (curY > clippingsize.Value.Y) break;
            }
        }
    }
}
