using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using JLChnToZ.IMEHelper;

namespace IMEHelperTest {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font1;
        IMEHandler handler;
        KeyboardState lastState;
        Texture2D whitePixel;
        string content;


        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            content = "";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            handler = new IMEHandler(this);

            handler.onResultReceived += (s, e) => {
                switch ((int)e.result) {
                    case 8:
                        if (content.Length > 0)
                            content = content.Remove(content.Length - 1, 1);
                        break;
                    case 27:
                    case 13:
                        content = "";
                        break;
                    default:
                        content += e.result;
                        break;
                }
            };
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font1 = Content.Load<SpriteFont>("chi_jp");
            whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            whitePixel.SetData<Color>(new Color[] { Color.White });
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {

            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.F12) && lastState.IsKeyUp(Keys.F12)) {
                handler.Enabled = !handler.Enabled;
            }
            lastState = ks;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            Vector2 len = font1.MeasureString(content);
            spriteBatch.DrawString(font1, "按下 F12 啟用 / 停用 IME", new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(font1, content, new Vector2(10, 30), Color.White);
            Vector2 drawPos = new Vector2(15 + len.X, 30);
            Vector2 measStr = new Vector2(0, 15);
            Color compColor = Color.White;
            if (handler.CompositionCursorPos == 0)
                spriteBatch.Draw(whitePixel, new Rectangle((int)drawPos.X, (int)drawPos.Y, 1, (int)measStr.Y), Color.White);
            for(int i = 0; i < handler.Composition.Length; i++) {
                string val = handler.Composition[i].ToString();
                switch (handler.GetCompositionAttr(i)) {
                    case CompositionAttributes.Converted: compColor = Color.LightGreen; break;
                    case CompositionAttributes.FixedConverted: compColor = Color.Gray; break;
                    case CompositionAttributes.Input: compColor = Color.Orange; break;
                    case CompositionAttributes.InputError: compColor = Color.Red; break;
                    case CompositionAttributes.TargetConverted: compColor = Color.Yellow; break;
                    case CompositionAttributes.TargetNotConverted: compColor = Color.SkyBlue; break;
                }
                spriteBatch.DrawString(font1, val, drawPos, compColor);
                measStr = font1.MeasureString(val);
                drawPos += new Vector2(measStr.X, 0);
                if ((i + 1) == handler.CompositionCursorPos)
                    spriteBatch.Draw(whitePixel, new Rectangle((int)drawPos.X, (int)drawPos.Y, 1, (int)measStr.Y), Color.White);
            }
            uint startIndex = handler.CandidatesPageStart;
            uint size = handler.CandidatesPageSize;
            uint sel = handler.CandidatesSelection;
            if (startIndex + size <= sel || startIndex > sel)
                startIndex = sel;
            for (uint i = handler.CandidatesPageStart;
                i < Math.Min(handler.CandidatesPageStart + handler.CandidatesPageSize, handler.Candidates.Length);
                i++) {
                spriteBatch.DrawString(font1,
                    String.Format("{0}.{1}", i + 1 - handler.CandidatesPageStart, handler.Candidates[i]),
                    new Vector2(15 + len.X, 50 + (i - handler.CandidatesPageStart) * 20),
                    i == handler.CandidatesSelection ? Color.Yellow : Color.White);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
