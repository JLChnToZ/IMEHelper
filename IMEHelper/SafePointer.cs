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
using System.Runtime.InteropServices;

namespace JLChnToZ.IMEHelper {
    /// <summary>
    /// Allocate an unmanaged handle pointer with specific length.
    /// </summary>
    public class SafePointer: IDisposable {
        private IntPtr _pointer;
        private int _length;
        private bool disposed;

        /// <summary>
        /// The handle pointer
        /// </summary>
        public IntPtr Pointer { get { return _pointer; } }

        /// <summary>
        /// Handle length
        /// </summary>
        public int Length { get { return _length; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">The length of the handle</param>
        public SafePointer(int length) {
            _length = length;
            _pointer = Marshal.AllocHGlobal(_length);
            disposed = false;
        }

        ~SafePointer() {
            Dispose();
        }

        /// <summary>
        /// Dispose the handle
        /// </summary>
        public void Dispose() {
            if (!disposed) {
                Marshal.FreeHGlobal(_pointer);
                disposed = true;
            }
        }

        /// <summary>
        /// Gets the structure content the handle stored.
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <returns>The structure</returns>
        public T ToStructure<T>() {
            return (T)Marshal.PtrToStructure(_pointer, typeof(T));
        }

        /// <summary>
        /// Gets the string the handle stored until the NULL character reads.
        /// </summary>
        /// <returns>The string of the handle</returns>
        public override string ToString() {
            return Marshal.PtrToStringAuto(_pointer);
        }

        /// <summary>
        /// Gets the byte array content the handle stored.
        /// </summary>
        /// <param name="start">Start index</param>
        /// <returns>An array of bytes</returns>
        public byte[] GetBytes(int start = 0) {
            byte[] b = new byte[_length - start];
            Marshal.Copy(_pointer, b, start, _length - start);
            return b;
        }
    }
}
