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
            Char = 0x0102,
            MouseWheel = 0x020A,
            
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
            GCSResultClause = 0x1000,
            
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
            ImeKeyUp = 0x0291,
            
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
            ImnPrivate = 0x000E,
            
            InputLanguageChange = 0x0051;
        #endregion

        [DllImport("imm32.dll", SetLastError = true, EntryPoint = "ImmAssociateContext")]
        public static extern IntPtr AssociateContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("imm32.dll", CharSet = CharSet.Unicode, EntryPoint = "ImmGetCandidateList")]
        private static extern uint _GetCandidateList(IntPtr hIMC, uint dwIndex, IntPtr candidateList, uint dwBufLen);

        [DllImport("imm32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "ImmGetCompositionString")]
        private static extern int _GetCompositionString(IntPtr hIMC, int CompositionStringFlag, IntPtr buffer, int bufferLength);

        [DllImport("imm32.dll", SetLastError = true, EntryPoint = "ImmGetContext")]
        public static extern IntPtr GetContext(IntPtr hWnd);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern bool TranslateMessage(IntPtr message);

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct _candidateList {
            public uint sz, style, cnt, sel, pgstart, pgsz;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.U4)]
            public uint[] offset;
        }

        public struct CandidateList {
            public uint Style, Selection, PageStart, PageSize;
            public string[] Candidates;
        }

        public static CandidateList GetCandidateList(IntPtr hIMC, uint index) {
            uint _length = _GetCandidateList(hIMC, index, IntPtr.Zero, 0);
            CandidateList output = new CandidateList();
            if (_length > 0)
                using (SafePointer _ptr = new SafePointer((int)_length)) {
                    _length = _GetCandidateList(hIMC, index, _ptr.Pointer, _length);
                    _candidateList _list = _ptr.ToStructure<_candidateList>();
                    output.PageSize = _list.pgsz;
                    output.PageStart = _list.pgstart;
                    output.Selection = _list.sel;
                    output.Style = _list.style;
                    output.Candidates = new string[_list.cnt];
                    for (int i = 0; i < _list.cnt; i++) {
                        int sOffset = Marshal.ReadInt32(_ptr.Pointer, 24 + 4 * i);
                        output.Candidates[i] = Marshal.PtrToStringUni(_ptr.GetOffsetPointer(sOffset));
                    }
                }
            return output;
        }

        public static int GetCompositionInt(IntPtr hIMC, int CompositionStringFlag) {
            return _GetCompositionString(hIMC, CompositionStringFlag, IntPtr.Zero, 0);
        }

        public static byte[] GetCompositionString(IntPtr hIMC, int CompositionStringFlag) {
            int _length = GetCompositionInt(hIMC, CompositionStringFlag);
            byte[] output = new byte[0];
            if (_length > 0)
                using (SafePointer _ptr = new SafePointer(_length)) {
                    _length = _GetCompositionString(hIMC, CompositionStringFlag, _ptr.Pointer, _length);
                    output = _ptr.GetBytes();
                }
            return output;
        }
    }
}
