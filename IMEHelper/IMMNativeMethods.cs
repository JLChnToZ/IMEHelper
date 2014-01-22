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
using System.Text;

namespace JLChnToZ.IMEHelper {
    internal static class IMM {

        #region Constants
        public const int
            KeyDown = 0x0100,
            Char = 0x0102;

        public const int
            GCSCompReadStr = 0x0001,
            GCSCompReadAttr = 0x0002,
            GCSCompReadClause = 0x0004,
            GCSCompStr = 0x0008,
            GCSCompAttr = 0x0010,
            GCSCompClause = 0x0020,
            GCSCursorPos = 0x0080,
            GCSDeltaStart = 0x0100,
            GCSResultReadStr = 0x0200,
            GCSResultReadClause = 0x0400,
            GCSResultStr = 0x0800,
            GCSResultClause = 0x1000;

        public const int
            ImeStartCompostition = 0x010D,
            ImeEndComposition = 0x010E,
            ImeComposition = 0x010F,
            ImeKeyLast = 0x010F,
            ImeSetContext = 0x0281,
            ImeNotify = 0x0282,
            ImeControl = 0x0283,
            ImeCompositionFull = 0x0284,
            ImeSelect = 0x0285,
            ImeChar = 0x286,
            ImeRequest = 0x0288,
            ImeKeyDown = 0x0290,
            ImeKeyUp = 0x0291;

        public const int
            ImnCloseStatusWindow = 0x0001,
            ImnOpenStatusWindow = 0x0002,
            ImnChangeCandidate = 0x0003,
            ImnCloseCandidate = 0x0004,
            ImnOpenCandidate = 0x0005,
            ImnSetConversionMode = 0x0006,
            ImnSetSentenceMode = 0x0007,
            ImnSetOpenStatus = 0x0008,
            ImnSetCandidatePos = 0x0009,
            ImnSetCompositionFont = 0x000A,
            ImnSetCompositionWindow = 0x000B,
            ImnSetStatusWindowPos = 0x000C,
            ImnGuideLine = 0x000D,
            ImnPrivate = 0x000E;

        public const int InputLanguageChange = 0x0051;
        #endregion

        [DllImport("imm32.dll", SetLastError = true)]
        public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
        public static extern uint ImmGetCandidateList(IntPtr hIMC, uint deIndex, IntPtr candidateList, uint dwBufLen);

        [DllImport("imm32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int ImmGetCompositionString(IntPtr hIMC, int CompositionStringFlag, StringBuilder buffer, int bufferLength);

        [DllImport("imm32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int ImmGetCompositionString(IntPtr hIMC, int CompositionStringFlag, IntPtr buffer, int bufferLength);

        [DllImport("imm32.dll", SetLastError = true)]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern bool TranslateMessage(IntPtr message);

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct CandidateList {
            public uint dwSize;
            public uint dwStyle;
            public uint dwCount;
            public uint dwSelection;
            public uint dwPageStart;
            public uint dwPageSize;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.U4)]
            public uint[] dwOffset;
        }
    }
}
