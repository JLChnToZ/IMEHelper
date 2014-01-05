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
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace JLChnToZ.IMEHelper {

    public class IMEResultEventArgs : EventArgs {
        internal IMEResultEventArgs(char result) {
            this.result = result;
        }
        public char result { get; private set; }
    }

    public sealed class IMENativeWindow : NativeWindow, IDisposable {
        public bool IsEnabled { get; private set; }

        public IMMCompositionString CompositionString { get; private set; }
        public IMMCompositionString ResultString { get; private set; }

        public IMMCompositionInt CompositionCursorPos { get; private set; }

        public string[] Candidates { get; private set; }
        public uint CandidatesPageStart { get; private set; }
        public uint CandidatesPageSize { get; private set; }
        public uint CandidatesSelection { get; private set; }

        private bool _disposed, _showIMEWin;

        private IntPtr Context;

        public IMENativeWindow(Game game, bool showDefaultIMEWindow = false) {
            this.Context = IntPtr.Zero;
            this.Candidates = new string[0];
            this.CompositionCursorPos = new IMMCompositionInt(IMM.GCSCursorPos);
            this.CompositionString = new IMMCompositionString(IMM.GCSCompStr);
            this.ResultString = new IMMCompositionString(IMM.GCSResultStr);
            this._showIMEWin = showDefaultIMEWindow;
            AssignHandle(game.Window.Handle);
            CharMessageFilter.AddFilter();
        }

        public event EventHandler onCandidatesReceived;
        public event EventHandler onCompositionReceived;
        public event EventHandler<IMEResultEventArgs> onResultReceived;

        public void disableIME() {
            IsEnabled = false;
            IMM.ImmAssociateContext(Handle, IntPtr.Zero);
        }

        public void Dispose() {
            if (!_disposed) {
                ReleaseHandle();
                _disposed = true;
            }
        }

        public void enableIME() {
            IsEnabled = true;
            IMM.ImmAssociateContext(Handle, Context);
        }

        protected override void WndProc(ref Message msg) {
            switch (msg.Msg) {
                case IMM.ImeSetContext: IMESetContext(ref msg); break;
                case IMM.InputLanguageChange: return;
                case IMM.ImeNotify: IMENotify(msg.WParam.ToInt32()); break;
                case IMM.ImeStartCompostition: IMEStartComposion(msg.LParam.ToInt32()); break;
                case IMM.ImeComposition: IMEComposition(msg.LParam.ToInt32()); break;
                case IMM.ImeEndComposition: IMEEndComposition(msg.LParam.ToInt32()); break;
                case IMM.Char: CharEvent(msg.WParam.ToInt32()); break;
            }
            base.WndProc(ref msg);
        }

        #region IME Message Handlers
        private void IMESetContext(ref Message msg) {
            if (msg.WParam.ToInt32() == 1) {
                IntPtr ptr = IMM.ImmGetContext(Handle);
                if (Context == IntPtr.Zero)
                    Context = ptr;
                CompositionCursorPos.IMEHandle = Context;
                CompositionString.IMEHandle = Context;
                ResultString.IMEHandle = Context;
                if (ptr == IntPtr.Zero && IsEnabled)
                    enableIME();
                if (!_showIMEWin)
                    msg.LParam = (IntPtr)0;
            }
        }

        private void IMENotify(int WParam) {
            switch (WParam) {
                case IMM.ImnOpenCandidate:
                case IMM.ImnChangeCandidate: IMEChangeCandidate(); break;
                case IMM.ImnCloseCandidate: IMECloseCandidate(); break;
                case IMM.ImnPrivate: break;
            }
        }

        private void IMEChangeCandidate() {
            uint length = IMM.ImmGetCandidateList(Context, 0, IntPtr.Zero, 0);
            if (length > 0) {
                IntPtr pointer = Marshal.AllocHGlobal((int)length);
                length = IMM.ImmGetCandidateList(Context, 0, pointer, length);
                IMM.CandidateList cList =
                    (IMM.CandidateList)Marshal.PtrToStructure(pointer, typeof(IMM.CandidateList));
                CandidatesSelection = cList.dwSelection;
                CandidatesPageStart = cList.dwPageStart;
                CandidatesPageSize = cList.dwPageSize;
                if (cList.dwCount > 1) {
                    Candidates = new string[cList.dwCount];
                    for (int i = 0; i < cList.dwCount; i++) {
                        int sOffset = Marshal.ReadInt32(pointer, 24 + 4 * i);
                        Candidates[i] = Marshal.PtrToStringUni((IntPtr)(pointer.ToInt32() + sOffset));
                    }
                    if (onCandidatesReceived != null)
                        onCandidatesReceived(this, EventArgs.Empty);
                }
                else
                    IMECloseCandidate();
            }
        }

        private void IMECloseCandidate() {
            CandidatesSelection = CandidatesPageStart = CandidatesPageSize = 0;
            Candidates = new string[0];
            if (onCandidatesReceived != null)
                onCandidatesReceived(this, EventArgs.Empty);
        }

        private void IMEStartComposion(int lParam) {
            CompositionString.clear();
            ResultString.clear();
            if (onCompositionReceived != null)
                onCompositionReceived(this, EventArgs.Empty);
        }

        private void IMEComposition(int lParam) {
            bool result = false;
            result = result || CompositionString.update(lParam);
            result = result || CompositionCursorPos.update(lParam);
            if (result && onCompositionReceived != null)
                onCompositionReceived(this, EventArgs.Empty);
        }

        private void IMEEndComposition(int lParam) {
            CompositionString.clear();
            ResultString.update(lParam);
            if (onCompositionReceived != null)
                onCompositionReceived(this, EventArgs.Empty);
        }

        private void CharEvent(int wParam) {
            if (onResultReceived != null)
                onResultReceived(this, new IMEResultEventArgs((char)wParam));
        }
        #endregion
    }
}
