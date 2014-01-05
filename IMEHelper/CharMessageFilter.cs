// -----------------------------------------------------------------------
// <copyright file="CharMessageFilter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

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
