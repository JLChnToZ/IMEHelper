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
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

namespace JLChnToZ.IMEHelper {
    internal abstract class IMMCompositionResultHandler {
        internal IntPtr IMEHandle { get; set; }

        public int Flag { get; private set; }

        internal IMMCompositionResultHandler(int flag) {
            this.Flag = flag;
            this.IMEHandle = IntPtr.Zero;
        }

        internal virtual void Update() { }

        internal bool Update(int lParam) {
            if ((lParam & Flag) == Flag) {
                Update();
                return true;
            }
            return false;
        }
    }

    internal class IMMCompositionString : IMMCompositionResultHandler, IEnumerable<byte> {
        private byte[] _values;

        public int Length { get; private set; }

        public byte[] Values { get { return _values; } }

        public byte this[int index] { get { return _values[index]; } }

        internal IMMCompositionString(int flag) : base(flag) {
            Clear();
        }

        public IEnumerator<byte> GetEnumerator() {
            foreach (byte b in _values)
                yield return b;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public override string ToString() {
            if (Length <= 0) return String.Empty;
            return Encoding.Unicode.GetString(_values, 0, Length);
        }

        internal void Clear() {
            _values = new byte[0];
            Length = 0;
        }

        internal override void Update() {
            _values = IMM.GetCompositionString(IMEHandle, Flag);
            Length = _values.Length;
        }
    }

    internal class IMMCompositionInt : IMMCompositionResultHandler {
        public int Value { get; private set; }

        internal IMMCompositionInt(int flag) : base(flag) { }

        public override string ToString() {
            return Value.ToString();
        }

        internal override void Update() {
            Value = IMM.GetCompositionInt(IMEHandle, Flag);
        }
    }
}
