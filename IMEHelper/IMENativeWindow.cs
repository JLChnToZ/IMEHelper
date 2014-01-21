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

    /// <summary>
    /// Special event arguemnt class stores new character that IME sends in.
    /// </summary>
    public class IMEResultEventArgs : EventArgs {

        internal IMEResultEventArgs(char result) {
            this.result = result;
        }

        /// <summary>
        /// The result character
        /// </summary>
        public char result { get; private set; }
    }

    /// <summary>
    /// Native window class that handles IME.
    /// </summary>
    public sealed class IMENativeWindow : NativeWindow, IDisposable {
        /// <summary>
        /// Gets the state if the IME should be enabled
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Composition String
        /// </summary>
        public IMMCompositionString CompositionString { get; private set; }

        /// <summary>
        /// Composition Clause
        /// </summary>
        public IMMCompositionString CompositionClause { get; private set; }

        /// <summary>
        /// Composition String Reads
        /// </summary>
        public IMMCompositionString CompositionReadString { get; private set; }

        /// <summary>
        /// Composition Clause Reads
        /// </summary>
        public IMMCompositionString CompositionReadClause { get; private set; }

        /// <summary>
        /// Result String
        /// </summary>
        public IMMCompositionString ResultString { get; private set; }

        /// <summary>
        /// Result Clause
        /// </summary>
        public IMMCompositionString ResultClause { get; private set; }

        /// <summary>
        /// Result String Reads
        /// </summary>
        public IMMCompositionString ResultReadString { get; private set; }

        /// <summary>
        /// Result Clause Reads
        /// </summary>
        public IMMCompositionString ResultReadClause { get; private set; }

        /// <summary>
        /// Caret position of the composition
        /// </summary>
        public IMMCompositionInt CompositionCursorPos { get; private set; }

        /// <summary>
        /// Array of the candidates
        /// </summary>
        public string[] Candidates { get; private set; }

        /// <summary>
        /// First candidate index of current page
        /// </summary>
        public uint CandidatesPageStart { get; private set; }

        /// <summary>
        /// How many candidates should display per page
        /// </summary>
        public uint CandidatesPageSize { get; private set; }

        /// <summary>
        /// The selected canddiate index
        /// </summary>
        public uint CandidatesSelection { get; private set; }

        private bool _disposed, _showIMEWin;
        private IntPtr _context;

        /// <summary>
        /// Called when the candidates updated
        /// </summary>
        public event EventHandler onCandidatesReceived;

        /// <summary>
        /// Called when the composition updated
        /// </summary>
        public event EventHandler onCompositionReceived;

        /// <summary>
        /// Called when a new result character is coming
        /// </summary>
        public event EventHandler<IMEResultEventArgs> onResultReceived;

        /// <summary>
        /// Constructor, must be called when the window create.
        /// </summary>
        /// <param name="handle">Handle of the window</param>
        /// <param name="showDefaultIMEWindow">True if you want to display the default IME window</param>
        public IMENativeWindow(IntPtr handle, bool showDefaultIMEWindow = false) {
            this._context = IntPtr.Zero;
            this.Candidates = new string[0];
            this.CompositionCursorPos = new IMMCompositionInt(IMM.GCSCursorPos);
            this.CompositionString = new IMMCompositionString(IMM.GCSCompStr);
            this.CompositionClause = new IMMCompositionString(IMM.GCSCompClause);
            this.CompositionReadString = new IMMCompositionString(IMM.GCSCompReadStr);
            this.CompositionReadClause = new IMMCompositionString(IMM.GCSCompReadClause);
            this.ResultString = new IMMCompositionString(IMM.GCSResultStr);
            this.ResultClause = new IMMCompositionString(IMM.GCSResultClause);
            this.ResultReadString = new IMMCompositionString(IMM.GCSResultReadStr);
            this.ResultReadClause = new IMMCompositionString(IMM.GCSResultReadClause);
            this._showIMEWin = showDefaultIMEWindow;
            AssignHandle(handle);
            CharMessageFilter.AddFilter();
        }

        /// <summary>
        /// Enable the IME
        /// </summary>
        public void enableIME() {
            IsEnabled = true;
            IMM.ImmAssociateContext(Handle, _context);
        }

        /// <summary>
        /// Disable the IME
        /// </summary>
        public void disableIME() {
            IsEnabled = false;
            IMM.ImmAssociateContext(Handle, IntPtr.Zero);
        }

        /// <summary>
        /// Dispose everything
        /// </summary>
        public void Dispose() {
            if (!_disposed) {
                ReleaseHandle();
                _disposed = true;
            }
        }

        protected override void WndProc(ref Message msg) {
            switch (msg.Msg) {
                case IMM.ImeSetContext: IMESetContext(ref msg); break;
                case IMM.InputLanguageChange: return;
                case IMM.ImeNotify: IMENotify(msg.WParam.ToInt32()); if(!_showIMEWin) return; break;
                case IMM.ImeStartCompostition: IMEStartComposion(msg.LParam.ToInt32()); if(!_showIMEWin) return; break;
                case IMM.ImeComposition: IMEComposition(msg.LParam.ToInt32()); break;
                case IMM.ImeEndComposition: IMEEndComposition(msg.LParam.ToInt32()); break;
                case IMM.Char: CharEvent(msg.WParam.ToInt32()); break;
            }
            base.WndProc(ref msg);
        }

        private void ClearComposition() {
            CompositionString.clear();
            CompositionClause.clear();
            CompositionReadString.clear();
            CompositionReadClause.clear();
        }

        private void ClearResult() {
            ResultString.clear();
            ResultClause.clear();
            ResultReadString.clear();
            ResultReadClause.clear();
        }

        #region IME Message Handlers
        private void IMESetContext(ref Message msg) {
            if (msg.WParam.ToInt32() == 1) {
                IntPtr ptr = IMM.ImmGetContext(Handle);
                if (_context == IntPtr.Zero)
                    _context = ptr;
                else if (ptr == IntPtr.Zero && IsEnabled)
                    enableIME();
                CompositionCursorPos.IMEHandle = _context;
                CompositionString.IMEHandle = _context;
                CompositionClause.IMEHandle = _context;
                CompositionReadString.IMEHandle = _context;
                CompositionReadClause.IMEHandle = _context;
                ResultString.IMEHandle = _context;
                ResultClause.IMEHandle = _context;
                ResultReadString.IMEHandle = _context;
                ResultReadClause.IMEHandle = _context;
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
                default: break;
            }
        }

        private void IMEChangeCandidate() {
            uint length = IMM.ImmGetCandidateList(_context, 0, IntPtr.Zero, 0);
            if (length > 0) {
                IntPtr pointer = Marshal.AllocHGlobal((int)length);
                length = IMM.ImmGetCandidateList(_context, 0, pointer, length);
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
                } else
                    IMECloseCandidate();
                Marshal.FreeHGlobal(pointer);
            }
        }

        private void IMECloseCandidate() {
            CandidatesSelection = CandidatesPageStart = CandidatesPageSize = 0;
            Candidates = new string[0];
            if (onCandidatesReceived != null)
                onCandidatesReceived(this, EventArgs.Empty);
        }

        private void IMEStartComposion(int lParam) {
            ClearComposition();
            ClearResult();
            if (onCompositionReceived != null)
                onCompositionReceived(this, EventArgs.Empty);
        }

        private void IMEComposition(int lParam) {
            if (CompositionString.update(lParam)) {
                CompositionClause.update();
                CompositionReadString.update();
                CompositionReadClause.update();
                CompositionCursorPos.update();
                if (onCompositionReceived != null)
                    onCompositionReceived(this, EventArgs.Empty);
            }
        }

        private void IMEEndComposition(int lParam) {
            ClearComposition();
            if (ResultString.update(lParam)) {
                ResultClause.update();
                ResultReadString.update();
                ResultReadClause.update();
            }
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
