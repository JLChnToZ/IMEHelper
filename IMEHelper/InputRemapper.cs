/* 
 * IME Helper Library
 * Copyright (c) 2013-2014, Jeremy Lam [JLChnToZ], All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JLChnToZ.IMEHelper {
    /// <summary>
    /// Helper class for getting correct mouse / keyboard state
    /// </summary>
    public class InputRemapper: GameComponent {

        /// <summary>
        /// Current Mouse State
        /// </summary>
        public MouseState mouseState { get; private set; }

        /// <summary>
        /// Current Keyboard State
        /// </summary>
        public KeyboardState keyboardState { get; private set; }

        internal InputRemapper(Game game)
            : base(game) {
                game.Components.Add(this);
        }

        /// <summary>
        /// Called when the game updates.
        /// </summary>
        /// <param name="gameTime">Game time snapshot</param>
        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            MouseState ms = Mouse.GetState();
            mouseState = new MouseState(
                ms.X,
                ms.Y,
                // Mouse wheel is not correct if the IME helper is used, thus it is needed to remap here.
                CharMessageFilter.Added ? CharMessageFilter.MouseWheel : ms.ScrollWheelValue,
                ms.LeftButton,
                ms.MiddleButton,
                ms.RightButton,
                ms.XButton1,
                ms.XButton2);
            keyboardState = Keyboard.GetState();
            // TODO: I don't know if there has any input events need to remap, please add them if it is neccessary.
        }

    }
}
