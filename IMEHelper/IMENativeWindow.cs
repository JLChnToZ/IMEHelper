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

    internal sealed class IMENativeWindow : NativeWindow, IDisposable {
        public string[] _candidates;
        public StringBuilder _compositionStr;
        public int _compositionStrLength;
        public bool _enabled;
        public uint _pgStart, _pgSize, _pgSel;

        private bool _disposed, _showIMEWin;

        private IntPtr Context;

        public IMENativeWindow(Game game, bool showDefaultIMEWindow = false) {
            this.Context = IntPtr.Zero;
            this._candidates = new string[0];
            this._compositionStr = new StringBuilder();
            this._compositionStrLength = 0;
            this._showIMEWin = showDefaultIMEWindow;
            AssignHandle(game.Window.Handle);
            Application.AddMessageFilter(new keyFilter());
        }

        public event EventHandler onCandidatesReceived;
        public event EventHandler onCompositionReceived;
        public event EventHandler<IMEResultEventArgs> onResultReceived;

        public void disableIME() {
            _enabled = false;
            IMM.ImmAssociateContext(Handle, IntPtr.Zero);
        }

        public void Dispose() {
            if (!_disposed) {
                ReleaseHandle();
                _disposed = true;
            }
        }

        public void enableIME() {
            _enabled = true;
            IMM.ImmAssociateContext(Handle, Context);
        }

        protected override void WndProc(ref Message msg) {
            switch (msg.Msg) {
                case IMM.ImeSetContext: IMESetContext(ref msg); break;
                case IMM.InputLanguageChange: return;
                case IMM.ImeNotify: IMENotify(msg.WParam.ToInt32()); break;
                case IMM.ImeStartCompostition: ClearComposition(); break;
                case IMM.ImeComposition: IMEComposition(msg.LParam.ToInt32()); break;
                case IMM.ImeEndComposition: ClearComposition(); break;
                case IMM.Char: CharEvent(msg.WParam.ToInt32()); break;
            }
            base.WndProc(ref msg);
        }

        private class keyFilter : IMessageFilter {
            public bool PreFilterMessage(ref Message m) {
                if (m.Msg == IMM.KeyDown) {
                    IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(m));
                    Marshal.StructureToPtr(m, intPtr, true);
                    IMM.TranslateMessage(intPtr);
                }
                return false;
            }
        }

        #region IME Message Handlers
        private void IMESetContext(ref Message msg) {
            if (msg.WParam.ToInt32() == 1) {
                IntPtr ptr = IMM.ImmGetContext(Handle);
                if (Context == IntPtr.Zero)
                    Context = ptr;
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
                _pgSel = cList.dwSelection;
                _pgStart = cList.dwPageStart;
                _pgSize = cList.dwPageSize;
                if (cList.dwCount > 1) {
                    _candidates = new string[cList.dwCount];
                    for (int i = 0; i < cList.dwCount; i++) {
                        int sOffset = Marshal.ReadInt32(pointer, 24 + 4 * i);
                        _candidates[i] = Marshal.PtrToStringUni((IntPtr)(pointer.ToInt32() + sOffset));
                    }
                    if (onCandidatesReceived != null)
                        onCandidatesReceived(this, EventArgs.Empty);
                }
                else
                    IMECloseCandidate();
            }
        }

        private void IMECloseCandidate() {
            _pgSel = _pgStart = _pgSize = 0;
            _candidates = new string[0];
            if (onCandidatesReceived != null)
                onCandidatesReceived(this, EventArgs.Empty);
        }

        private void ClearComposition() {
            _compositionStr.Clear();
            _compositionStrLength = 0;
            if (onCompositionReceived != null)
                onCompositionReceived(this, EventArgs.Empty);
        }

        private void IMEComposition(int lParam) {
            if ((lParam & IMM.GCSCompStr) == IMM.GCSCompStr) {
                _compositionStr.Clear();
                _compositionStrLength = IMM.ImmGetCompositionString(Context,
                    IMM.GCSCompStr, null, 0);
                IMM.ImmGetCompositionString(Context,
                    IMM.GCSCompStr,
                    _compositionStr, _compositionStrLength);
                if (onCompositionReceived != null)
                    onCompositionReceived(this, EventArgs.Empty);
            }
        }

        private void CharEvent(int wParam) {
            if (onResultReceived != null)
                onResultReceived(this, new IMEResultEventArgs((char)wParam));
        }
        #endregion
    }
}
