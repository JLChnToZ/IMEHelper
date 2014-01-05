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
using System.Windows.Forms;

namespace JLChnToZ.IMEHelper {
    internal static class CharMessageFilter {
        public static bool Added { get; private set; }

        public static void AddFilter() {
            if (!Added)
                Application.AddMessageFilter(new filter());
            Added = true;
        }

        private class filter : IMessageFilter {
            public bool PreFilterMessage(ref Message m) {
                if (m.Msg == IMM.KeyDown) {
                    IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(m));
                    Marshal.StructureToPtr(m, intPtr, true);
                    IMM.TranslateMessage(intPtr);
                }
                return false;
            }
        }
    }
}
