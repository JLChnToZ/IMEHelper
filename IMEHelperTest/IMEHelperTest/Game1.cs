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
                handler.enabled = !handler.enabled;
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
            spriteBatch.DrawString(font1, handler.Composition, new Vector2(15 + len.X, 30), Color.Yellow);
            for (uint i = handler.CandidatesPageStart;
                i < Math.Min(handler.CandidatesPageStart + handler.CandidatesPageSize, handler.Candidates.Length);
                i++) {
                spriteBatch.DrawString(font1,
                    String.Format("{0}.{1}", i + 1, handler.Candidates[i]),
                    new Vector2(15 + len.X, 50 + (i - handler.CandidatesPageStart) * 20),
                    i == handler.CandidatesSelection ? Color.Yellow : Color.White);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
