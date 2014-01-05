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
using System.Text;

namespace JLChnToZ.IMEHelper {
    public abstract class IMMCompositionResultHandler {
        internal IntPtr IMEHandle { get; set; }
        public int flag { get; private set; }

        internal IMMCompositionResultHandler(int flag) {
            this.flag = flag;
            this.IMEHandle = IntPtr.Zero;
        }

        internal virtual void update() { }

        internal bool update(int lParam) {
            if ((lParam & flag) == flag) {
                update();
                return true;
            }
            return false;
        }
    }

    public class IMMCompositionString : IMMCompositionResultHandler {
        private StringBuilder resultHandler;
        public int resultLength { get; private set; }

        public override string ToString() {
            if (resultHandler.Length <= 0) return string.Empty;
            return resultHandler.ToString(0, Math.Min(resultLength / 2, resultHandler.Length));
        }

        internal IMMCompositionString(int flag) : base(flag) {
            this.resultHandler = new StringBuilder();
            this.resultLength = 0;
        }

        internal void clear() {
            resultHandler.Clear();
        }

        internal override void update() {
            clear();
            resultLength = IMM.ImmGetCompositionString(IMEHandle, flag, null, 0);
            IMM.ImmGetCompositionString(IMEHandle, flag, resultHandler, resultLength);
        }
    }

    public class IMMCompositionInt : IMMCompositionResultHandler {
        public int result { get; private set; }

        internal IMMCompositionInt(int flag) : base(flag) { }

        public override string ToString() {
            return result.ToString();
        }

        internal override void update() {
            result = IMM.ImmGetCompositionString(IMEHandle, flag, null, 0);
            System.Diagnostics.Debug.Print("H:{0}, F:{1}, R:{2}", IMEHandle, flag, result);
        }
    }
}
