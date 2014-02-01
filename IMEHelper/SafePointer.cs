using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace JLChnToZ.IMEHelper {
    public class SafePointer: IDisposable {
        private IntPtr _pointer;
        private int _length;
        private bool disposed;

        public IntPtr Pointer { get { return _pointer; } }

        public int Length { get { return _length; } }

        public SafePointer(int length) {
            _length = length;
            _pointer = Marshal.AllocHGlobal(_length);
            disposed = false;
        }

        ~SafePointer() {
            Dispose();
        }

        public void Dispose() {
            if (!disposed) {
                Marshal.FreeHGlobal(_pointer);
                disposed = true;
            }
        }

        public T ToStructure<T>() {
            return (T)Marshal.PtrToStructure(_pointer, typeof(T));
        }

        public override string ToString() {
            return Marshal.PtrToStringAuto(_pointer);
        }

        public IntPtr GetOffsetPointer(int Offset) {
            return (IntPtr)(_pointer.ToInt32() + Offset);
        }

        public byte[] GetBytes(int start = 0) {
            byte[] b = new byte[_length - start];
            Marshal.Copy(_pointer, b, start, _length - start);
            return b;
        }
    }
}
