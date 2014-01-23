/* 
 * The MIT License (MIT)
 * 
 * Copyright (c) NyaRuRu
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Runtime.InteropServices;
using TSF;

namespace JLChnToZ.IMEHelper.TSF {

    internal class CTfThreadMgr : IDisposable {
        private ITfThreadMgr _ThreadMgr;
        private ITfThreadMgrEx _ThreadMgrEx;
        private ITfSource _Source;
        private ITfSourceSingle _SourceSingle;
        private ITfUIElementMgr _UIElementMgr;
        private ITfCompartmentMgr _CompartmentMgr;

        public ITfThreadMgr ThreadMgr { get { return _ThreadMgr; } }
        public ITfThreadMgrEx ThreadMgrEx { get { return _ThreadMgrEx; } }
        public ITfSource Source { get { return _Source; } }
        public ITfSourceSingle SourceSingle { get { return _SourceSingle; } }
        public ITfUIElementMgr UIElementMgr { get { return _UIElementMgr; } }
        public ITfCompartmentMgr CompartmentMgr { get { return _CompartmentMgr; } }

        private CTfThreadMgr(ITfThreadMgr threadMgr) {
            this._ThreadMgr = threadMgr;
            this._ThreadMgrEx = threadMgr as ITfThreadMgrEx;
            this._Source = threadMgr as ITfSource;
            this._SourceSingle = threadMgr as ITfSourceSingle;
            this._UIElementMgr = threadMgr as ITfUIElementMgr;
            this._CompartmentMgr = threadMgr as ITfCompartmentMgr;
        }

        public static CTfThreadMgr GetThreadMgr() {
            ITfThreadMgr mgr;
            TextFrameworkFunctions.TF_GetThreadMgr(out mgr);
            if (mgr == null) {
                Type clsid = Type.GetTypeFromCLSID(TextFrameworkDeclarations.CLSID_TF_ThreadMgr);
                mgr = Activator.CreateInstance(clsid) as ITfThreadMgr;
            }
            return new CTfThreadMgr(mgr);
        }

        public void Dispose() {
            if (_ThreadMgr != null) {
                Marshal.ReleaseComObject(_ThreadMgr);
                _ThreadMgr = null;
                _ThreadMgrEx = null;
                _Source = null;
                _SourceSingle = null;
                _UIElementMgr = null;
                _CompartmentMgr = null;
            }
        }
    }
}
